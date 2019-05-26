using Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace RoutingApp.Services
{
    public class LoggerService : ILogger, INotifyPropertyChanged
    {

        #region LogTypeMapping

        private static Dictionary<LogType, SolidColorBrush> _logColor = new Dictionary<LogType, SolidColorBrush>
        {
            {LogType.ControllerLog, new SolidColorBrush(Colors.Red) },
            {LogType.RouterLog, new SolidColorBrush(Colors.Green) },
            {LogType.PathsLog, new SolidColorBrush(Colors.Brown)},
            {LogType.SimulationLog, (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffaacc"))
    }
        };

        private static Dictionary<LogType, string> _logTypeHeader = new Dictionary<LogType, string>
        {
            {LogType.ControllerLog, "Controller logging" },
            {LogType.RouterLog, "Router logging" },
            {LogType.PathsLog, "Paths logging" },
            {LogType.SimulationLog, "SimulationLogging" }
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

        //TODO: debug stub. Delete after fix
        public int CurrentStep = 3;
        public List<LogStep> LogStepsList = new List<LogStep>
        {
           new LogStep{
               StepNumber = 1,
               ControllerLogs = new List<string>{"controllerLog1", "controllerLog2", "controllerLog3"},
               PathsLog = new List<string>{ "PathsLog1", "PathsLog2", "PathsLog3" }               
           },
           new LogStep{
               StepNumber = 2,
               ControllerLogs = new List<string>{"controllerLogsStep2_1", "controllerLogsStep2_2", "controllerLogsStep2_3" },
               PathsLog = new List<string>{ "PathsLogStep2_1", "PathsLogStep2_2", "PathsLogStep2_3" }               
           }
        };

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region cstor

        private LoggerService()
        {

        }

        #endregion

        #region Public methods

        public void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        //action - output code must be here
        public void Log(LogType logType, string message)
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
                    OutPutTextBox.ScrollToEnd();

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
                    OutPutTextBox.ScrollToEnd();

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
                    OutPutTextBox.ScrollToEnd();
                }), DispatcherPriority.ContextIdle);
            }
        }
        //used for adding start header
        public void StartLog(int stepcount)
        {
            LogStepsList.Add(new LogStep
            {
                StepNumber = CurrentStep,
                ControllerLogs = new List<string> { $"controllerLog. Step {CurrentStep}, log 1", $"controllerLog. Step {CurrentStep}, log 2", $"controllerLog. Step {CurrentStep}, log 3" },
                PathsLog = new List<string> { $"PathsLog. Step {CurrentStep}. Log1", $"PathsLog. Step {CurrentStep}. Log2", $"PathsLog. Step {CurrentStep}. Log3" }
            });
            CurrentStep++;
        }

        public void SubmitLog()
        {
            OnPropertyChanged("LogStepsList");
        }

        #endregion
    }

    public class LogStep
    {
        public int StepNumber { get; set; }
        public List<string> ControllerLogs { get; set; }
        public List<string> PathsLog { get; set; }
    }
}