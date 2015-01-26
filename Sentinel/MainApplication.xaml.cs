#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Windows;
using Sentinel.Classification;
using Sentinel.Classification.Interfaces;
using Sentinel.Filters;
using Sentinel.Filters.Interfaces;
using Sentinel.Highlighters;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Images;
using Sentinel.Images.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Logger;
using Sentinel.Logs;
using Sentinel.Logs.Interfaces;
using Sentinel.Preferences;
using Sentinel.Properties;
using Sentinel.Providers;
using Sentinel.Providers.Interfaces;
using Sentinel.Services;
using Sentinel.Views;
using Sentinel.Views.Gui;
using Sentinel.Views.Interfaces;
using Sentinel.Extractors.Interfaces;
using Sentinel.Extractors;
using Sentinel.Services.Interfaces;
using System;

#endregion

namespace Sentinel
{
    /// <summary>
    /// Interaction logic for MainApplication.xaml
    /// </summary>
    public partial class MainApplication : Application
    {
        /// <summary>
        /// Initializes a new instance of the MainApplication class.
        /// </summary>
        public MainApplication()
        {
            AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionHandler;
            Settings.Default.Upgrade();

            ServiceLocator locator = ServiceLocator.Instance;
            locator.ReportErrors = true;
            
            locator.Register<ISessionManager>(new SessionManager());

            // Request that the application close on main window close.
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void FirstChanceExceptionHandler(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (e.Exception is System.Net.Sockets.SocketException) return;

            string errorString = string.Format("Sender: {0} FirstChanceException raised in {1} : Message -- {2} :: InnerException -- {3} :: TargetSite -- {4} :: StackTrace -- {5} :: HelpLink -- {6} ",
                                                    sender,
                                                    AppDomain.CurrentDomain.FriendlyName,
                                                    e.Exception.Message,
                                                    (e.Exception.InnerException != null) ? e.Exception.InnerException.Message : "",
                                                    (e.Exception.TargetSite != null) ? e.Exception.TargetSite.Name : "",
                                                    (e.Exception.StackTrace != null) ? e.Exception.StackTrace : "",
                                                    (e.Exception.HelpLink != null) ? e.Exception.HelpLink : "");

            MessageBox.Show(errorString, "Error " + e.Exception.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        /// <summary>
        /// Override of the <c>Application.OnExit</c> method.
        /// </summary>
        /// <param name="e">Exit event arguments.</param>
        protected override void OnExit(ExitEventArgs e)
        {          
            base.OnExit(e);
        }
    }
}