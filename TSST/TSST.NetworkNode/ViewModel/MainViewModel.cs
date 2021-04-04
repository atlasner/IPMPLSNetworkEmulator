using System;
using System.Collections.Concurrent;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using TSST.NetworkNode.Model;
using TSST.NetworkNode.Service.ConfigReaderService;
using TSST.NetworkNode.Service.ManagementAgentService;
using TSST.NetworkNode.Service.RoutingService;
using TSST.Shared.Model;
using TSST.Shared.Service.CableCloudConnectionService;
using TSST.Shared.Service.LogService;
using TSST.NetworkNode.Service.LRMService;
using TSST.Shared.Model.Messages;

namespace TSST.NetworkNode.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private readonly ICableCloudConnectionService _cableCloudConnectionService;
        private readonly IManagementAgentService _managementAgentService;
        private readonly ILogService _logService;
        private readonly IRoutingService _routingService;
        private readonly ILRMService _lrmService;

        public string WindowTitle => NetworkNodeConfig.Name;

        public ObservableCollection<string> Logs => _logService.Logs;
        private static object _lock = new object();

        public NetworkNodeConfigDto NetworkNodeConfig { get; }

        public MainViewModel(ICableCloudConnectionService cableCloudConnectionService, IManagementAgentService managementAgentService, IConfigReaderService configReaderService, ILogService logService, IRoutingService routingService, ILRMService lrmService)
        {
            _cableCloudConnectionService = cableCloudConnectionService;
            _cableCloudConnectionService.PackageReceived += OnPackageReceived;
            _cableCloudConnectionService.MessageReceived += OnMessageReceived;
            _managementAgentService = managementAgentService;
            //_managementAgentService.RowInfoReceived += OnRowInfoReceived;

            _logService = logService;
            _routingService = routingService;
            _lrmService = lrmService;
            _lrmService.RowInfoReceived += OnRowInfoReceived;
            _lrmService.MessageReceived += OnMessageReceived;

            NetworkNodeConfig = configReaderService.ReadHostConfig();
            try
            {
                NetworkNodeConfig = configReaderService.ReadHostConfig();
            }
            catch (Exception e)
            {
                _logService.LogError("WRONG CONFIG: " + e.Message);
            }

            BindingOperations.EnableCollectionSynchronization(Logs, _lock);

            StartClients();
            Task.Run(async () =>
            {
                while (true)
                {
                    await CheckList();
                    Thread.Sleep(50);
                }

            });
        }

        private void StartClients()
        {
            _cableCloudConnectionService.StartClient(NetworkNodeConfig.CloudIp, NetworkNodeConfig.CloudPort,
                NetworkNodeConfig.Name);

            _lrmService.StartClient(NetworkNodeConfig.CloudIp, NetworkNodeConfig.CCRCPort, NetworkNodeConfig.Name);
            if (NetworkNodeConfig.DomainCCRCPort != 0)
            {
                _lrmService.StartClientDomain(NetworkNodeConfig.CloudIp, NetworkNodeConfig.DomainCCRCPort, NetworkNodeConfig.Name);
            }


        }

        void OnPackageReceived(object sender, PackageReceivedEventArgs e)
        {
            _routingService.ProcessPackage(e.Packet);
        }

        void OnRowInfoReceived(object sender, RowInfoReceivedEventArgs e)
        {
            _routingService.ApplyRowInfo(e.RowInfo);
        }

        private ConcurrentQueue<ISignalingMessage> messages = new ConcurrentQueue<ISignalingMessage>();

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            messages.Enqueue(e.Message);
        }

        private async Task<bool> CheckList()
        {
            if (messages.IsEmpty) return true;
            ISignalingMessage message;
            while(!messages.TryDequeue(out message))
            {
            }
            await ProcessMessage(message);
            return true;
        }

        Task<bool> ProcessMessage(ISignalingMessage message)
        {
            try
            {
                switch (message)
                { 

                    case SNPLinkConnectionRequest_req req:
                    { 
                            _logService.LogInfo($"Getting {req}");
                            var port = NetworkNodeConfig.Neighbors.First(n => n.Name == req.To).Port;

                            var msg = new SNPNegotiation_req
                            {
                                From = req.From,
                                FromPort = port,
                                ToPort = req.ToPort,
                                To = req.To,
                                Port = port,
                                Slots = req.Slots,
                                Guid = req.Guid,
                                RequestFromDomain = req.RequestFromDomain,
                                Rerouting = req.Rerouting,
                                Action = req.Action,
                                Releasing = req.Releasing
                            };

                            if (msg.Releasing)
                            {
                                var snpLinkConnectionRequest_rsp = new SNPLinkConnectionRequest_rsp
                                {
                                    From = req.From,
                                    FromPort = port,
                                    To = req.To,
                                    ToPort = req.ToPort,
                                    Slots = req.Slots,
                                    Guid = req.Guid,
                                    RequestFromDomain = req.RequestFromDomain,
                                    Result = RequestResult.Confirmed,
                                    Rerouting = req.Rerouting,
                                    Releasing = req.Releasing
                                };

                                if (msg.RequestFromDomain)
                                {
                                    _lrmService.SendMessageToDomainCCRC(snpLinkConnectionRequest_rsp);
                                }
                                else
                                {
                                    _lrmService.SendMessageToCCRC(snpLinkConnectionRequest_rsp);
                                }

                                return Task.FromResult(true);
                            }

                            _logService.LogInfo($"Sending {msg}");
                            _cableCloudConnectionService.SendSNPNegotiation(msg);
                            break;
                        }
                    case SNPNegotiation_req negreq:
                        {

                            _logService.LogInfo($"Getting {negreq}");

                            var port = NetworkNodeConfig.Neighbors.First(n => n.Name == negreq.From).Port;

                            var areFree = _routingService.CheckFreeSlots(negreq.Slots);

                            var msg = new SNPNegotiation_rsp
                            {
                                From = negreq.From,
                                FromPort = negreq.FromPort,
                                To = negreq.To,
                                ToPort = port,
                                Port = port,
                                Slots = negreq.Slots,
                                Result = areFree ? RequestResult.Confirmed : RequestResult.Rejected,
                                Guid = negreq.Guid,
                                RequestFromDomain = negreq.RequestFromDomain,
                                Rerouting = negreq.Rerouting,
                                Releasing = negreq.Releasing
                            };

                            _logService.LogInfo($"Sending {msg}");

                            _cableCloudConnectionService.SendSNPNegotiation(msg);
                            break;
                        }
                    case SNPNegotiation_rsp negrsp:
                        {

                            _logService.LogInfo($"Getting {negrsp}");

                            var port = NetworkNodeConfig.Neighbors.First(n => n.Name == negrsp.To).Port;

                            var snpLinkConnectionRequest_rsp = new SNPLinkConnectionRequest_rsp
                            {
                                From = negrsp.From,
                                FromPort = port,
                                To = negrsp.To,
                                ToPort = negrsp.ToPort,
                                Slots = negrsp.Slots,
                                Guid = negrsp.Guid,
                                RequestFromDomain = negrsp.RequestFromDomain,
                                Result = negrsp.Result == RequestResult.Confirmed
                                    ? RequestResult.Confirmed
                                    : RequestResult.Rejected,
                                Rerouting = negrsp.Rerouting,
                                Releasing = negrsp.Releasing
                            };


                            _logService.LogInfo($"Sending {snpLinkConnectionRequest_rsp}");

                            if (negrsp.RequestFromDomain)
                            {
                                _lrmService.SendMessageToDomainCCRC(snpLinkConnectionRequest_rsp);
                            }
                            else
                            {
                                _lrmService.SendMessageToCCRC(snpLinkConnectionRequest_rsp);
                            }

                            break;
                        }
                    case CableAction cableAction:
                        {
                            _logService.LogInfo("Sending LocalTopology to CC and RC");
                            _lrmService.SendMessageToCCRC(cableAction);
                            break;
                        }
                }
            } catch(Exception ex)
            {
                _logService.LogError(ex.StackTrace);
            }

            return Task.FromResult(true);
        }
    }
}
