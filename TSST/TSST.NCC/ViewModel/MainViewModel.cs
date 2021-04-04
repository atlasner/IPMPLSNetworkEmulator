using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using TSST.NCC.Model;
using TSST.NCC.Service.ConfigReaderService;
using TSST.NCC.Service.NCCService;
using TSST.Shared.Service.LogService;

namespace TSST.NCC.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private readonly IConfigReaderService _configReaderService;
        private readonly INccService _nccService;
        private readonly ILogService _logService;

        public string WindowTitle => NCCConfig.Name;

        public ObservableCollection<string> Logs => _logService.Logs;
        private static object _lock = new object();

        public NccConfigDto NCCConfig { get; }

        public MainViewModel(IConfigReaderService configReaderService, ILogService logService, INccService nccService)
        {
            _configReaderService = configReaderService;
            _nccService = nccService;
            _logService = logService;

            try
            {
                NCCConfig = configReaderService.ReadNccConfig();
            }
            catch (Exception e)
            {
                _logService.LogError("WRONG CONFIG: " + e.Message);
            }



            BindingOperations.EnableCollectionSynchronization(Logs, _lock);

            if(NCCConfig.DomainSlave == false)
            {
                _nccService.StartListening("127.0.0.1", NCCConfig.ServerPort);
            }
            else
            {
                _nccService.ConnectToNcc("127.0.0.1", NCCConfig.ClientPort, NCCConfig.Name);
                _nccService.StartListening("127.0.0.1", NCCConfig.ServerPort);
            }
        }


    }
}