using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RoutingApp.Core.Services
{
    public class LoggerService
    {

        #region LogTypeMapping

        private static Dictionary<LogType, ConsoleColor> _logColor = new Dictionary<LogType, ConsoleColor>
        {
            {LogType.ControllerLog, ConsoleColor.Red },
            {LogType.RouterLog, ConsoleColor.Green },
            {LogType.Paths, ConsoleColor.Yellow }
        };

        private static Dictionary<LogType, string> _logTypeHeader = new Dictionary<LogType, string>
        {
            {LogType.ControllerLog, "Controller logging" },
            {LogType.RouterLog, "Router logging" },
            {LogType.Paths, "Paths logging" }
        };

        public TextBox OutPutTextBox { get; set; }

        #endregion

        #region Fields

        private static string _logHeaderPatern = "********** {0} **********";
        private static string _logFooterPatern = "********** End of {0} **********";

        private static LoggerService _instance;
        private static object _loggerLock = new object();

        public static LoggerService Instance
        {
            get
            {
                lock (_loggerLock)
                {
                    if (_instance == null)
                    {
                        _instance = new LoggerService();
                    }
                    return _instance;
                }
            }
            private set { }
        }

        #endregion

        #region cstor

        private LoggerService()
        {

        }

        #endregion

        #region Public methods

        //action - output code must be here
        public void CustomizeOutput(LogType logType, Action action)
        {
            Console.ForegroundColor = _logColor[logType];

            Console.WriteLine(string.Format(_logHeaderPatern, _logTypeHeader[logType]));

            Console.ForegroundColor = ConsoleColor.White;

            action();

            Console.ForegroundColor = _logColor[logType];

            Console.WriteLine(string.Format(_logFooterPatern, _logTypeHeader[logType]));

            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(message);

            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogToTextBox(string message)
        {
            if (OutPutTextBox != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    OutPutTextBox.Text += message + "\n";
                    //OutPutTextBox.ScrollToEnd();
                    //OutPutTextBox.Text += ;
                }), DispatcherPriority.ContextIdle);
            }
        }

        #endregion
    }
}