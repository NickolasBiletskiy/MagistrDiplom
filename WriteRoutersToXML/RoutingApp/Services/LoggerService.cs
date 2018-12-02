using Core.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace RoutingApp.Core.Services
{
    public class LoggerService : ILogger
    {

        #region LogTypeMapping

        private static Dictionary<LogType, SolidColorBrush> _logColor = new Dictionary<LogType, SolidColorBrush>
        {
            {LogType.ControllerLog, new SolidColorBrush(Colors.Red) },
            {LogType.RouterLog, new SolidColorBrush(Colors.Green) },
            {LogType.Paths, new SolidColorBrush(Colors.Brown)},
            {LogType.Simulation, (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffaacc"))
    }
        };

        private static Dictionary<LogType, string> _logTypeHeader = new Dictionary<LogType, string>
        {
            {LogType.ControllerLog, "Controller logging" },
            {LogType.RouterLog, "Router logging" },
            {LogType.Paths, "Paths logging" },
            {LogType.Simulation, "SimulationLogging" }
        };

        public RichTextBox OutPutTextBox { get; set; }

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
        public void CustomizeOutput(LogType logType, string message)
        {
            if (OutPutTextBox != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var paragraph = new Paragraph();
                    paragraph.Foreground = _logColor[logType];
                    paragraph.Inlines.Add(string.Format(_logHeaderPatern, _logTypeHeader[logType]));
                    paragraph.Inlines.Add(new LineBreak());
                    paragraph.Inlines.Add(message);
                    paragraph.Inlines.Add(string.Format(_logFooterPatern, _logTypeHeader[logType]));
                    paragraph.Inlines.Add(new LineBreak());
                    OutPutTextBox.Document.Blocks.Add(paragraph);

                }), DispatcherPriority.ContextIdle);
            }
        }

        public void LogError(string message)
        {
            if (OutPutTextBox != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var paragraph = new Paragraph();
                    paragraph.Foreground = new SolidColorBrush(Colors.Red);
                    paragraph.Inlines.Add(message);
                    paragraph.Inlines.Add(new LineBreak());
                    OutPutTextBox.Document.Blocks.Add(paragraph);

                }), DispatcherPriority.ContextIdle);
            }
        }

        public void Log(string message)
        {
            if (OutPutTextBox != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(message);
                    paragraph.Inlines.Add(new LineBreak());
                    OutPutTextBox.Document.Blocks.Add(paragraph);
                }), DispatcherPriority.ContextIdle);
            }
        }

        #endregion
    }
}