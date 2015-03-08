#region License

//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//

#endregion License

#region Using directives

using Sentinel.Classification.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Services;
using Sentinel.Support.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;

#endregion Using directives

namespace Sentinel.Logs
{
    public class Log : ViewModelBase, ILogger
    {
        private readonly IClassifyingService<IClassifier> _classifier;
        private readonly List<ILogEntry> _entries = new List<ILogEntry>();
        private readonly List<ILogEntry> _newEntries = new List<ILogEntry>();
        private bool _enabled = true;
        private string _name;

        public Log()
        {
            Entries = _entries;
            NewEntries = _newEntries;

            _classifier = ServiceLocator.Instance.Get<IClassifyingService<IClassifier>>();

            // Observe the NewEntries to maintain a full history.
            PropertyChanged += OnPropertyChanged;
        }

        #region ILogger Members

        public IEnumerable<ILogEntry> Entries { get; private set; }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public IEnumerable<ILogEntry> NewEntries { get; private set; }

        public void Clear()
        {
            lock (_entries)
            {
                _entries.Clear();
            }

            lock (_newEntries)
            {
                _newEntries.Clear();
            }

            OnPropertyChanged("Entries");
            OnPropertyChanged("NewEntries");
            GC.Collect();// Do we really need to do this?
        }

        public void AddBatch(Queue<ILogEntry> entries)
        {
            if (!_enabled || entries.Count <= 0)
                return;

            var processed = new Queue<ILogEntry>();
            while (entries.Count > 0)
            {
                if (_classifier != null)
                {
                    var entry = _classifier.Classify(entries.Dequeue());
                    processed.Enqueue(entry);
                }
            }

            lock (_newEntries)
            {
                _newEntries.Clear();
                _newEntries.AddRange(processed);
            }

            OnPropertyChanged("NewEntries");
        }

        #endregion ILogger Members

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NewEntries")
            {
                lock (_newEntries)
                {
                    lock (_entries)
                    {
                        _entries.AddRange(_newEntries);
                    }

                    OnPropertyChanged("Entries");
                }
            }
        }
    }
}