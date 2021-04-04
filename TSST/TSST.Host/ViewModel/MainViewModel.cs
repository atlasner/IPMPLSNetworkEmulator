using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TSST.Host.Model;
using TSST.Host.Service.ConfigReaderService;
using TSST.Shared.Model;
using TSST.Shared.Service.CableCloudConnectionService;
using TSST.Shared.Service.LogService;
using TSST.Host.Service.CPCCService;
using TSST.Shared.Model.Messages;

namespace TSST.Host.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ICableCloudConnectionService _cableCloudConnectionService;
        private readonly ICPCCService _cpccService;
        private readonly ILogService _logService;

        private string _sendButtonText = "Send";
        public string SendButtonText {
            get => _sendButtonText;
            set {
                _sendButtonText = value;
                RaisePropertyChanged();
            }

        }

        private string _callButtonText = "Call";

        public string CallButtonText {
            get => _callButtonText;
            set {
                _callButtonText = value;
                RaisePropertyChanged();
            }
        }

        public MainViewModel(ICableCloudConnectionService cableCloudConnectionService, IConfigReaderService configReaderService, ILogService logService, ICPCCService cpccService)
        {
            _cableCloudConnectionService = cableCloudConnectionService;
            _cableCloudConnectionService.PackageReceived += OnPackageReceived;
            _cpccService = cpccService;
            _cpccService.MessageReceived += OnMessageReceived;
            _logService = logService;

            try
            {
                _hostConfig = configReaderService.ReadHostConfig();
            }
            catch (Exception e)
            {
                _logService.LogError("WRONG CONFIG: " + e.Message);
            }
            Initialize();
        }

        private void Initialize()
        {
            BindingOperations.EnableCollectionSynchronization(Logs, Lock);

            _cableCloudConnectionService.StartClient(_hostConfig.CloudIp, _hostConfig.CloudPort, _hostConfig.Name);
            _cpccService.ConnectToNcc("127.0.0.1", _hostConfig.CpccPort, _hostConfig.CpccName);

        }

        public IEnumerable<int> Capacities { get; } = new List<int> { 1, 10, 40, 100 };

        private CancellationTokenSource _cancellationTokenSource;
        public string WindowTitle => _hostConfig.Name;
        public IEnumerable<string> HostList => _hostConfig.NameToIp.Keys.Where(h => !h.Equals(_hostConfig.Name));

        public IEnumerable<string> Periods { get; } = new List<string> { "1s", "2s", "5s" };

        public string SelectedHost { get; set; }
        public string SelectedPeriod { get; set; }
        public int SelectedCapacity { get; set; }

        private bool _sendPeriodically;
        public bool SendPeriodically {
            get => _sendPeriodically;
            set {
                _sendPeriodically = value;
                RaisePropertyChanged();
            }
        }

        public string Message { get; set; }

        public ObservableCollection<string> Logs => _logService.Logs;
        private static readonly object Lock = new object();

        private readonly HostConfigDto _hostConfig;

        private RelayCommand _sendRequest;
        public RelayCommand SendCallRequest => _sendRequest ?? (_sendRequest = new RelayCommand(SendCallOrTear));

        

        private void SendCallOrTear()
        {
            if (CallButtonText == "Call")
            {
                _currentGuid = _cpccService.SendCallRequest(_hostConfig.Name, SelectedHost, SelectedCapacity);

            }
            else
            {
                _cpccService.SendCallTeardownRequest(_hostConfig.Name, _currentCallNode, _currentGuid);
                _currentCallNode = null;
                CallButtonText = "Call";
            }
        }

        private RelayCommand<EonPacket> _sendMplsPackage;
        private string _currentCallNode;
        private Guid _currentGuid;
        private List<int> _currentSlots = new List<int>();

        public RelayCommand<EonPacket> SendMplsPackage {
            get {
                return _sendMplsPackage ?? (_sendMplsPackage = new RelayCommand<EonPacket>(_ =>
                {
                    var package = new EonPacket
                    {
                        SourceAddress = _hostConfig.NameToIp[_hostConfig.Name],
                        DestinationAddress = _hostConfig.NameToIp[SelectedHost],
                        Port = _hostConfig.OutputPort,
                        OccupiedSlots = _currentSlots,
                        OccupiedCapacity = 100,
                        Ttl = 255,
                        Content = Message
                    };

                    SendPackage(package);
                }));

            }

        }

        private void SendPackage(EonPacket package)
        {
            if (SendPeriodically)
            {
                if (SendButtonText == "Stop")
                {
                    _cancellationTokenSource?.Cancel();
                    SendButtonText = "Send";
                    SendPeriodically = !SendPeriodically;
                    return;
                }
                SendButtonText = "Stop";

                Task.Run(() =>
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _cableCloudConnectionService.SendPeriodically(package, SelectedPeriod,
                        _cancellationTokenSource.Token);
                });
            }
            else
            {
                SendButtonText = "Send";
                Task.Run(() => _cableCloudConnectionService.Send(package));
                _cancellationTokenSource?.Cancel();
            }
        }

        private void OnPackageReceived(object sender, PackageReceivedEventArgs e)
        {
            _logService.LogInfo($"Getting packet: {e.Packet.Content}");
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            _logService.LogInfo("Getting SignalingMessage");
            ProcessMessage(e.Message);
        }

        private void ProcessMessage(ISignalingMessage message)
        {
            try
            {
                switch (message)
                {
                    case CallAccept_req callAcceptReq:
                        var messageBoxResult = MessageBox.Show(
                            $"Do You want to accept the call from {callAcceptReq.From}?"
                            , "Confirmation"
                            , MessageBoxButton.YesNo);

                        var msg = new CallAccept_rsp
                        {
                            Capacity = callAcceptReq.Capacity,
                            From = callAcceptReq.From,
                            Slots = callAcceptReq.Slots,
                            To = callAcceptReq.To,
                            Guid = callAcceptReq.Guid
                        };
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            msg.Result = RequestResult.Confirmed;
                            CallButtonText = "Tear";
                            _currentCallNode = callAcceptReq.From;
                            _currentGuid = callAcceptReq.Guid;
                        }
                        else
                        {
                            msg.Result = RequestResult.Rejected;
                        }

                        _logService.LogInfo($"Sending {msg}");

                        _cpccService.SendMessage(msg);
                        break;
                    case CallTeardown_req callTeardownReq:
                        _logService.LogInfo($"Connection with {callTeardownReq.From} is now closed.");
                        CallButtonText = "Call";
                        break;
                    case CallRequest_rsp callRequestRsp:
                    {
                        if (callRequestRsp.Result == RequestResult.Confirmed)
                        {
                            CallButtonText = "Tear";
                            _currentCallNode = callRequestRsp.To;
                            _currentSlots = callRequestRsp.Slots;
                        }

                        _logService.LogInfo(callRequestRsp.Result == RequestResult.Confirmed
                            ? "Call accepted"
                            : "Call rejected");

                        break;
                    }
                    case NoRouteMessage noRouteMessage:
                    {
                        _currentCallNode = null;
                        _currentSlots = null;
                        _currentGuid = new Guid();
                        CallButtonText = "Call";
                        _logService.LogInfo("The connection has been broken. We can offer you discounts for our wonderful services.");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                _logService.LogError(e.StackTrace);
            }
        }

    }
}