using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TSST.Shared.Service.LogService;
using TSST.Subnetwork.Service.ConfigReaderService;
using TSST.Subnetwork.Service.RCService;

namespace TSST.Subnetwork.ViewModel
{
    public class RCViewModel
    {
        private readonly IConfigReaderService _configReaderService;
        private readonly ILogService _logService;
        public ObservableCollection<string> Logs => _logService.Logs;
        private static object _lock = new object();

        public RCViewModel(IConfigReaderService configReaderService)
        {
            WindowTitle = configReaderService.ReadSubnetworkConfig().Name.Remove(0, 2);
            _configReaderService = configReaderService;
            _logService = new LogService();

            BindingOperations.EnableCollectionSynchronization(Logs, _lock);

        }
        public string WindowTitle { get; set; }
        public void AddSmthToLogs(string message)
        {
            _logService.LogInfo(message);
        }
    }
}
