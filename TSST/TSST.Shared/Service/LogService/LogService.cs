using System;
using System.Collections.ObjectModel;
using TSST.Shared.Model;

namespace TSST.Shared.Service.LogService
{
    public class LogService : ILogService
    {
        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();


        private void AddToLogs(LogType logType, string message)
        {
            Logs.Add($"[{DateTime.Now.TimeOfDay}] [{logType.ToString().ToUpper()}]: " +
                     message);
        }

        public void LogError(string message)
        {
            AddToLogs(LogType.Error, message);
        }

        public void LogWarning(string message)
        {
            AddToLogs(LogType.Warning, message);
        }

        public void LogInfo(string message)
        {
            AddToLogs(LogType.Info, message);
        }
    }
}
