using System.Collections.ObjectModel;

namespace TSST.Shared.Service.LogService
{
    public interface ILogService
    {
        ObservableCollection<string> Logs { get; }
        void LogError(string message);
        void LogWarning(string message);
        void LogInfo(string message);
    }
}