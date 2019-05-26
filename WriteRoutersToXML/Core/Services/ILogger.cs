using System;

namespace Core.Services
{
    public enum LogType
    {
        ControllerLog,
        RouterLog,
        PathsLog,
        SimulationLog,
        LinesStateLog
    }

    public interface ILogger
    {
        void StartLog(int stepcount);
        void Log(LogType logType, string message);
        void Log(string message);
        void LogError(string message);
        void SubmitLog();
    }
}
