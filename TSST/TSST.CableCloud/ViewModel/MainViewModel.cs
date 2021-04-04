using System.Collections.ObjectModel;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TSST.CableCloud.Model;
using TSST.CableCloud.Service.CableCloudService;
using TSST.Shared.Service.LogService;

namespace TSST.CableCloud.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILogService _logService;
        private readonly ICableCloudService _cableCloudService;
        private static readonly object Lock = new object();

        private void Initialize()
        {
            BindingOperations.EnableCollectionSynchronization(Logs, Lock);

            _cableCloudService.StartListening();
        }

        public string WindowTitle => "CableCloud";

        public ObservableCollection<string> Logs => _logService.Logs;

        public ObservableCollection<ForwardingInfoDto> ForwardingInfo =>
            new ObservableCollection<ForwardingInfoDto>(_cableCloudService.CableCloudConfig.ForwardingTable);

        public ForwardingInfoDto SelectedCable { get; set; }

        private RelayCommand<ForwardingInfoDto> _switchCableStatusCommand;

        public RelayCommand<ForwardingInfoDto> SwitchCableStatusCommand
        {
            get
            {
                return _switchCableStatusCommand ?? (_switchCableStatusCommand = new RelayCommand<ForwardingInfoDto>( _ =>
                           {
                               _cableCloudService.SwitchCableStatus(SelectedCable);
                           }));
            }
        }

        public MainViewModel(ILogService logService, ICableCloudService cableCloudService)
        {
            _logService = logService;
            _cableCloudService = cableCloudService;
            Initialize();
        }
    }
}