﻿using System;

namespace Core.Services
{
    public enum LogType
    {
        ControllerLog,
        RouterLog,
        Paths,
        Simulation
    }

    public interface ILogger
    {
        void CustomizeOutput(LogType logType, string message);
        void LogError(string message);
        void Log(string message);
    }
}
