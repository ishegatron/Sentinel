namespace Sentinel.FileMonitor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    using Interfaces;
    using Interfaces.Providers;

    public class FileMonitoringProvider : ILogProvider
    {
        public static readonly IProviderRegistrationRecord ProviderRegistrationInformation = new ProviderRegistrationInformation(new ProviderInfo());
        protected BackgroundWorker PurgeWorker;

        private long _bytesRead;
        private BackgroundWorker _worker;
        private readonly bool _loadExistingContent;
        private readonly Regex _patternMatching;
        private readonly Queue<ILogEntry> _pendingQueue = new Queue<ILogEntry>();
        private readonly int _refreshInterval = 250;
        private readonly List<string> _usedGroupNames = new List<string>();

        public FileMonitoringProvider(IProviderSettings settings)
        {
            Debug.Assert(
                settings is IFileMonitoringProviderSettings,
                "The FileMonitoringProvider class expects configuration information " + "to be of IFileMonitoringProviderSettings type");

            var fileSettings = (IFileMonitoringProviderSettings)settings;
            ProviderSettings = fileSettings;
            FileName = fileSettings.FileName;
            Name = fileSettings.Name;
            Information = settings.Info;
            _refreshInterval = fileSettings.RefreshPeriod;
            _loadExistingContent = fileSettings.LoadExistingContent;
            _patternMatching = new Regex(fileSettings.MessageDecoder, RegexOptions.Singleline | RegexOptions.Compiled);

            PredetermineGroupNames(fileSettings.MessageDecoder);

            _worker = new BackgroundWorker();
            _worker.DoWork += DoWork;
            _worker.RunWorkerCompleted += DoWorkComplete;

            PurgeWorker = new BackgroundWorker { WorkerReportsProgress = true };
            PurgeWorker.DoWork += PurgeWorkerDoWork;
        }

        public string FileName { get; private set; }

        #region Implementation of ILogProvider

        public IProviderInfo Information { get; private set; }

        public IProviderSettings ProviderSettings { get; private set; }

        public ILogger Logger { get; set; }

        public void Start()
        {
            Debug.Assert(!string.IsNullOrEmpty(FileName), "Filename not specified");
            Debug.Assert(Logger != null, "No logger has been registered, this is required before starting a provider");

            Trace.WriteLine(string.Format("Starting of file-monitor upon {0}", FileName));
            _worker.RunWorkerAsync();
            PurgeWorker.RunWorkerAsync();
        }

        public void Close()
        {
            _worker.CancelAsync();
            PurgeWorker.CancelAsync();
        }

        public void Pause()
        {
            if (_worker != null)
            {
                // TODO: need a better pause mechanism...
                Close();
            }
        }

        public string Name { get; set; }

        public bool IsActive
        {
            get
            {
                //TODO:
                return !string.IsNullOrWhiteSpace(FileName) && new FileInfo(FileName).Exists && _worker.IsBusy;
            }
        }

        #endregion

        private void PurgeWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                // Go to sleep.
                Thread.Sleep(_refreshInterval);
                lock (_pendingQueue)//TODO: no ConcurrentQueue?
                {
                    if (_pendingQueue.Any())
                    {
                        Trace.WriteLine(string.Format("Adding a batch of {0} entries to the logger", _pendingQueue.Count()));
                        Logger.AddBatch(_pendingQueue);
                        Trace.WriteLine("Done adding the batch");
                    }
                }
            }
        }

        private void PredetermineGroupNames(string messageDecoder)
        {
            string decoder = messageDecoder.ToLower();
            if (decoder.Contains("(?<description>")) _usedGroupNames.Add("Description");
            if (decoder.Contains("(?<datetime>")) _usedGroupNames.Add("DateTime");
            if (decoder.Contains("(?<type>")) _usedGroupNames.Add("Type");
            if (decoder.Contains("(?<logger>")) _usedGroupNames.Add("Logger");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            var fi = new FileInfo(FileName);

            // Keep hold of incomplete lines, if any.
            var incomplete = string.Empty;
            var sb = new StringBuilder();

            if (!_loadExistingContent)
                _bytesRead = fi.Length;

            while (!e.Cancel)
            {
                fi.Refresh();

                if (fi.Exists)
                {
                    var length = fi.Length;
                    if (length < _bytesRead)
                    {
                        // File has been cleared
                        _bytesRead = 0;
                        _pendingQueue.Clear();
                        Logger.Clear();
                    }

                    if (length > _bytesRead)
                    {
                        using (var fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Write))
                        {
                            var position = fs.Seek(_bytesRead, SeekOrigin.Begin);
                            Debug.Assert(position == _bytesRead, "Seek did not go to where we asked.");

                            // Calculate length of file.
                            var bytesToRead = length - position;
                            Debug.Assert(bytesToRead < Int32.MaxValue, "Too much data to read using this method!");

                            var buffer = new byte[bytesToRead];

                            var bytesSuccessfullyRead = fs.Read(buffer, 0, (int)bytesToRead);
                            Debug.Assert(bytesSuccessfullyRead == bytesToRead, "Did not get as much as expected!");

                            // Put results into a buffer (prepend any unprocessed data retained from last read).
                            sb.Length = 0;
                            sb.Append(incomplete);
                            sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesSuccessfullyRead));

                            using (var sr = new StringReader(sb.ToString()))
                            {
                                while (sr.Peek() != -1)
                                {
                                    var line = sr.ReadLine();

                                    DecodeAndQueueMessage(line);
                                }
                            }

                            // Can we determine whether any tailing data was unprocessed?
                            _bytesRead = position + bytesSuccessfullyRead;
                        }
                    }
                }

                Thread.Sleep(_refreshInterval);
            }
        }

        private void DecodeAndQueueMessage(string message)
        {
            Debug.Assert(_patternMatching != null, "Regular expression has not be set");
            var m = _patternMatching.Match(message);

            if (!m.Success)
            {
                Trace.WriteLine("Message decoding did not work!");
                return;
            }

            lock (_pendingQueue)
            {
                var entry = new LogEntry();

                if (_usedGroupNames.Contains("Description"))
                {
                    entry.Description = m.Groups["Description"].Value;
                }

                if (_usedGroupNames.Contains("DateTime"))
                {
                    DateTime dt;
                    if (!DateTime.TryParse(m.Groups["DateTime"].Value, out dt))
                    {
                        Trace.WriteLine("Failed to parse date " + m.Groups["DateTime"].Value);
                    }
                    entry.DateTime = dt;
                }

                if (_usedGroupNames.Contains("Type"))
                {
                    entry.Type = m.Groups["Type"].Value;
                }

                if (_usedGroupNames.Contains("Logger"))
                {
                    entry.Source = m.Groups["Logger"].Value;
                }

                _pendingQueue.Enqueue(entry);
            }
        }

        private void DoWorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // TODO: brutal...
            _worker = null;
        }

        public class ProviderInfo : IProviderInfo
        {
            public Guid Identifier
            {
                get
                {
                    return new Guid("1a2f8249-b390-4baa-ba5e-3d67804ba1ed");
                }
            }

            public string Name
            {
                get
                {
                    return "File Monitoring Provider";
                }
            }

            public string Description
            {
                get
                {
                    return "Monitor a text file for new log entries.";
                }
            }
        }
    }
}