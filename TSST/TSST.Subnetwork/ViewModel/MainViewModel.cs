using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xaml;
using GalaSoft.MvvmLight;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;
using TSST.Shared.Model.Rows;
using TSST.Shared.Service.CableCloudConnectionService;
using TSST.Shared.Service.LogService;
using TSST.Subnetwork.Model;
using TSST.Subnetwork.Service.CCService;
using TSST.Subnetwork.Service.ConfigReaderService;
using TSST.Subnetwork.Service.ConnectivityService;
using TSST.Subnetwork.Service.DataHolder;
using TSST.Subnetwork.Service.RCService;

namespace TSST.Subnetwork.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private readonly IConfigReaderService _configReaderService;
        private readonly ILogService _logService;
        private readonly IRCService _rcService;
        private readonly ICCService _ccService;
        private readonly IConnectivityService _connectivityService;

        public string WindowTitle => SubnetworkConfig.Name.Remove(2, 2);

        public ObservableCollection<string> Logs => _logService.Logs;
        private static object _lock = new object();

        public SubnetworkConfigDto SubnetworkConfig { get; }

        private CableAction _lastCableAction;
        private RCViewModel x;
        public MainViewModel(RCViewModel rcViewModel, IConfigReaderService configReaderService, ILogService logService, IRCService rcService, ICCService ccService, IConnectivityService connectivityService)
        {
            x = rcViewModel;
            _configReaderService = configReaderService;
            _logService = logService;
            _rcService = rcService;
            _ccService = ccService;
            _connectivityService = connectivityService;
            _connectivityService.MessageReceived += OnMessageReceived;
            BindingOperations.EnableCollectionSynchronization(Logs, _lock);

            try
            {
                SubnetworkConfig = configReaderService.ReadSubnetworkConfig();
            }
            catch (Exception e)
            {
                _logService.LogError("WRONG CONFIG: " + e.Message);
            }

            if (SubnetworkConfig.IsDomain)
            {
                _connectivityService.ConnectToNCCorDomain("127.0.0.1", SubnetworkConfig.NCCPort, SubnetworkConfig.Name);
                _connectivityService.StartListening("127.0.0.1", SubnetworkConfig.ServerPort);
            }
            else
            {
                //_cableCloudConnectionService.StartClient("127.0.0.1", SubnetworkConfig.CableCloudPort, SubnetworkConfig.Name);
                _connectivityService.ConnectToNCCorDomain("127.0.0.1", SubnetworkConfig.ClientPort, SubnetworkConfig.Name);
                _connectivityService.StartListening("127.0.0.1", SubnetworkConfig.ServerPort);
            }

            Task.Run(async () =>
            {
                while (true)
                { 
                    await CheckList();
                    Thread.Sleep(50);
                }

            });

        }

        private readonly ConcurrentQueue<ISignalingMessage> messages = new ConcurrentQueue<ISignalingMessage>();

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message is SNPLinkConnectionRequest_rsp rsp && rsp.Releasing)
            {
                _logService.LogInfo($"Getting {rsp}");
            }
            else
            {
                messages.Enqueue(e.Message);
            }

        }

        private async Task<bool> CheckList()
        {
            if (messages.IsEmpty) return true;
            ISignalingMessage message;
            if (messages.TryDequeue(out message))
            {
            }
            await ProcessMessage(message);
            return true;
        }

        private Task<bool> ProcessMessage(ISignalingMessage message)
        {
            try
            {
                switch (message)
                {
                    case ConnectionRequest_rsp connectionRequestRsp when SubnetworkConfig.IsDomain:
                        {
                            _logService.LogInfo($"Getting {connectionRequestRsp}");
                            var model = DataHolder.GetDataModel(connectionRequestRsp.Guid);
                            model.ConnectionRequestRsp.Add(connectionRequestRsp);

                            var msgrsp = new ConnectionRequest_rsp
                            {
                                AddressFrom = model.From,
                                AddressTo = model.To,
                                Capacity = model.Capacity,
                                Guid = connectionRequestRsp.Guid,
                                Result = connectionRequestRsp.Result,
                                Slots = connectionRequestRsp.Slots,
                            };

                            _logService.LogInfo($"Sending {msgrsp} to NCC or Domain");
                            _connectivityService.SendMessageToNCCorDomain(msgrsp);

                            model.Slots = connectionRequestRsp.Slots;

                            if (connectionRequestRsp.Result == RequestResult.Rejected)
                            {
                                SendLinkConnectionRelease(model);
                                DataHolder.Data.Remove(model);
                            }

                            break;
                        }
                    case ConnectionRequest_req connectionRequestReq when SubnetworkConfig.IsDomain:
                        {
                            _logService.LogInfo($"Getting {connectionRequestReq}");

                            if (connectionRequestReq.Action == AllocationAction.Setup)
                            {
                                var newModel = new DataModel
                                {
                                    Capacity = connectionRequestReq.Capacity,
                                    From = connectionRequestReq.AddressFrom,
                                    To = connectionRequestReq.AddressTo,
                                    Slots = connectionRequestReq.Slots
                                };

                                newModel.ConnectionRequestReq.Add(connectionRequestReq);


                                _logService.LogInfo($"Sending RouteTableQueryRequest from {connectionRequestReq.AddressFrom} to {connectionRequestReq.AddressTo} to RC");
                                x.AddSmthToLogs($"Getting RouteTableQueryRequest from {connectionRequestReq.AddressFrom} to {connectionRequestReq.AddressTo}");

                                x.AddSmthToLogs($"Preparing path from {connectionRequestReq.AddressFrom} to {connectionRequestReq.AddressTo}");

                                var route = _rcService.GetRouteTableQuery(connectionRequestReq.AddressFrom, connectionRequestReq.AddressTo, connectionRequestReq.EstimatedDistance, connectionRequestReq.Capacity);
                                x.AddSmthToLogs($"Sending RouteTableQueryResponse");
                                _logService.LogInfo($"Getting RouteTableQueryResponse");
                                if (route == null)
                                {
                                    _connectivityService.SendMessageToNCCorDomain(new ConnectionRequest_rsp
                                    {
                                        AddressFrom = connectionRequestReq.AddressFrom,
                                        AddressTo = connectionRequestReq.AddressTo,
                                        Capacity = connectionRequestReq.Capacity,
                                        Guid = connectionRequestReq.Guid,
                                        Result = RequestResult.Rejected,
                                        Slots = connectionRequestReq.Slots
                                    });

                                    _logService.LogWarning($"There's no route from {connectionRequestReq.AddressFrom} to {connectionRequestReq.AddressTo}");
                                    return Task.FromResult(true);
                                }
                                _logService.LogInfo($"Found route: {string.Join(", ", route.Nodes)}");
                                newModel.RouteTableQuery = route;
                                newModel.Slots = route.Slots;

                                if (route.Nodes.Last().StartsWith("H"))
                                {
                                    var penultimateBorderNode = route.Nodes[route.Nodes.Count - 2];

                                    var rowInfo = new RowInfo
                                    {
                                        Action = ManagementAction.AddEonRow,
                                        Row = new EonRow
                                        {
                                            Node = penultimateBorderNode,
                                            OutPort = SubnetworkConfig.Clients.First(c => c.Name == route.Nodes.Last()).Port,
                                            FirstSlotIndex = route.Slots.First(),
                                            LastSlotIndex = route.Slots.Last()
                                        }
                                    };

                                    newModel.SentRowInfos.Add(rowInfo);

                                    _connectivityService.SendRowInfo(penultimateBorderNode, rowInfo);

                                    var firstNode = route.Nodes.First().StartsWith("H") ? route.Nodes[1] : route.Nodes[0];

                                    var msg = new ConnectionRequest_req
                                    {
                                        AddressFrom = firstNode,
                                        AddressTo = penultimateBorderNode,
                                        Capacity = connectionRequestReq.Capacity,
                                        Slots = newModel.Slots,
                                        Action = AllocationAction.Setup,
                                        Guid = connectionRequestReq.Guid
                                    };

                                    _logService.LogInfo($"Sending {msg} to {_connectivityService.SubdomainName}");

                                    _connectivityService.SendMessage(msg, _connectivityService.SubdomainName);

                                }
                                else
                                {
                                    var penultimateBorderNode = route.Nodes[route.Nodes.Count - 2];
                                    var lastBorderNode = route.Nodes.Last();

                                    var snpLinkConnectionRequest_req = new SNPLinkConnectionRequest_req
                                    {
                                        From = penultimateBorderNode,
                                        To = lastBorderNode,
                                        Slots = route.Slots,
                                        Action = AllocationAction.Setup,
                                        Guid = connectionRequestReq.Guid,
                                        RequestFromDomain = true
                                    };
                                    _logService.LogInfo($"Sending {snpLinkConnectionRequest_req}");

                                    _connectivityService.SendMessage(snpLinkConnectionRequest_req, penultimateBorderNode);
                                    newModel.SnpLinkConnectionRequestReq.Add(snpLinkConnectionRequest_req);

                                }
                                DataHolder.Data.Add(newModel);
                            }
                            else
                            {
                                SendReleaseAndDeleteModel(connectionRequestReq);
                            }

                            break;
                        }
                    case ConnectionRequest_req connectionRequestReq:
                        {

                            _logService.LogInfo(
                                $"Getting {connectionRequestReq}");

                            if (connectionRequestReq.Action == AllocationAction.Setup)
                            {
                                var newModel = new DataModel
                                {
                                    Capacity = connectionRequestReq.Capacity,
                                    From = connectionRequestReq.AddressFrom,
                                    To = connectionRequestReq.AddressTo,
                                    Slots = connectionRequestReq.Slots
                                };
                                newModel.ConnectionRequestReq.Add(connectionRequestReq);


                                _logService.LogInfo($"Sending RouteTableQueryRequest from {connectionRequestReq.AddressFrom} to {connectionRequestReq.AddressTo} to RC");
                                x.AddSmthToLogs($"Getting RouteTableQueryRequest from {connectionRequestReq.AddressFrom} to {connectionRequestReq.AddressTo}");

                                x.AddSmthToLogs($"Preparing path from {connectionRequestReq.AddressFrom} to {connectionRequestReq.AddressTo}");


                                var route = _rcService.GetRouteTableQuery(connectionRequestReq.AddressFrom,
                                    connectionRequestReq.AddressTo, connectionRequestReq.Slots);

                                x.AddSmthToLogs($"Sending RouteTableQueryResponse");
                                _logService.LogInfo($"Getting RouteTableQueryResponse");

                                newModel.RouteTableQuery = route;

                                if (route == null)
                                {
                                    _connectivityService.SendMessageToNCCorDomain(new ConnectionRequest_rsp
                                    {
                                        AddressFrom = connectionRequestReq.AddressFrom,
                                        AddressTo = connectionRequestReq.AddressTo,
                                        Capacity = connectionRequestReq.Capacity,
                                        Guid = connectionRequestReq.Guid,
                                        Result = RequestResult.Rejected,
                                        Slots = connectionRequestReq.Slots
                                    });

                                    _logService.LogWarning($"There's no route from {connectionRequestReq.AddressFrom} to {connectionRequestReq.AddressTo}");
                                    return Task.FromResult(true);
                                }

                                _logService.LogInfo($"Computed route {string.Join(", ", route.Nodes)}");

                                var startingIndex = route.Nodes.First().StartsWith("H") ? 1 : 0;

                                for (var i = startingIndex; i < route.Nodes.Count - 1; i++)
                                {
                                    var msg = new SNPLinkConnectionRequest_req
                                    {
                                        From = route.Nodes[i],
                                        To = route.Nodes[i + 1],
                                        Slots = route.Slots,
                                        Action = AllocationAction.Setup,
                                        Guid = connectionRequestReq.Guid
                                    };
                                    _logService.LogInfo(
                                        $"Sending {msg}");
                                    _connectivityService.SendMessage(msg, route.Nodes[i]);
                                    newModel.SnpLinkConnectionRequestReq.Add(msg);
                                }

                                newModel.RequestSentCount = route.Nodes.Count - 1;
                                _logService.LogInfo($"Sent {route.Nodes.Count - 1} requests");
                                DataHolder.Data.Add(newModel);
                            }
                            else
                            {
                                SendReleaseAndDeleteModel(connectionRequestReq);
                            }

                            break;
                        }
                    case CallTeardown_req callTeardownReq:
                        {
                            _logService.LogInfo($"Getting {callTeardownReq}");
                            SendReleaseAndDeleteModel(callTeardownReq);
                            break;
                        }
                    case SNPLinkConnectionRequest_rsp snplcrequest_rsp when SubnetworkConfig.IsDomain:
                        {
                            _logService.LogInfo($"Getting {snplcrequest_rsp}");

                            if (snplcrequest_rsp.Releasing)
                            {
                                return Task.FromResult(true);
                            }

                            var dataModel = DataHolder.GetDataModel(snplcrequest_rsp.Guid);
                            dataModel.SnpLinkConnectionRequestRsp.Add(snplcrequest_rsp);

                            
                            var firstNode = dataModel.RouteTableQuery.Nodes.First().StartsWith("H") ? dataModel.RouteTableQuery.Nodes[1] : dataModel.RouteTableQuery.Nodes[0];


                            var msg = new ConnectionRequest_req
                            {
                                AddressFrom = firstNode,
                                AddressTo = snplcrequest_rsp.From,
                                Capacity = dataModel.Capacity,
                                Slots = dataModel.RouteTableQuery.Slots,
                                Action = AllocationAction.Setup,
                                Guid = snplcrequest_rsp.Guid
                            };

                            _logService.LogInfo($"Sending ConnectionRequest_req to {_connectivityService.SubdomainName}");

                            _connectivityService.SendMessage(msg, _connectivityService.SubdomainName);


                            _logService.LogInfo($"Sending EonRow to border router {snplcrequest_rsp.From}");

                            var msgToRouter = new RowInfo
                            {
                                Action = ManagementAction.AddEonRow,
                                Row = new EonRow
                                {
                                    FirstSlotIndex = snplcrequest_rsp.Slots.First(),
                                    IncomingPort = 0,
                                    LastSlotIndex = snplcrequest_rsp.Slots.Last(),
                                    Node = snplcrequest_rsp.From,
                                    OutPort = snplcrequest_rsp.FromPort
                                }
                            };
                            _connectivityService.SendRowInfo(snplcrequest_rsp.From, msgToRouter);
                            dataModel.SentRowInfos.Add(msgToRouter);


                            break;
                        }
                    case SNPLinkConnectionRequest_rsp snplcrequest_rsp:
                        _logService.LogInfo($"Getting {snplcrequest_rsp}");

                        if (snplcrequest_rsp.Releasing)
                        {
                            return Task.FromResult(true);
                        }

                        var relatedConnection = DataHolder.GetDataModel(snplcrequest_rsp.Guid);
                        relatedConnection.SnpLinkConnectionRequestRsp.Add(snplcrequest_rsp);

                        _logService.LogInfo($"Checking if all responses arrived {relatedConnection.From}, {relatedConnection.To}, RspCount {relatedConnection.SnpLinkConnectionRequestRsp.Count}");


                        if (relatedConnection.SnpLinkConnectionRequestRsp.Count == relatedConnection.RequestSentCount)
                        {
                            var dataModel = DataHolder.GetDataModel(snplcrequest_rsp.Guid);
                            if (dataModel == null)
                                break;
                            foreach (var rowInfo in dataModel.SnpLinkConnectionRequestRsp.Select(req => new RowInfo
                            {
                                Action = ManagementAction.AddEonRow,
                                Row = new EonRow
                                {
                                    FirstSlotIndex = req.Slots.First(),
                                    LastSlotIndex = req.Slots.Last(),
                                    Node = req.From,
                                    OutPort = req.FromPort,
                                    //TODO incoming port
                                    IncomingPort = 0
                                }
                            }))
                            {
                                dataModel.SentRowInfos.Add(rowInfo);
                                _connectivityService.SendRowInfo(((EonRow)rowInfo.Row).Node, rowInfo);
                            }

                            if (!snplcrequest_rsp.Rerouting)
                            {

                                var msg = new ConnectionRequest_rsp
                                {
                                    Result = RequestResult.Confirmed,
                                    AddressFrom = dataModel.From,
                                    AddressTo = dataModel.To,
                                    Slots = dataModel.Slots,
                                    Capacity = dataModel.Capacity,
                                    Guid = snplcrequest_rsp.Guid
                                };
                                _logService.LogInfo($"Sending {msg} to NCC or Domain");

                                _connectivityService.SendMessageToNCCorDomain(msg);
                            }
                        }
                        else
                        {
                            _logService.LogInfo($"Still waiting for the rest of responses");
                        }

                        break;

                    case CableAction cableAction:
                        {

                            if (cableAction.Guid == _lastCableAction?.Guid)
                            {
                                _logService.LogInfo($"Getting duplicated LocalTopology");
                                x.AddSmthToLogs($"Getting duplicated LocalTopology");
                                return Task.FromResult(true);
                            }

                            _logService.LogInfo($"Getting LocalTopology");
                            x.AddSmthToLogs($"Getting LocalTopology");

                            _lastCableAction = cableAction;
                            var edge = _rcService.SwitchEdgeStatus(cableAction.From, cableAction.To, cableAction.Status);

                            if (edge == null)
                                return Task.FromResult(true);

                            x.AddSmthToLogs($"Switching status {cableAction.From}-{cableAction.To} to {cableAction.Status}");
                            var dataModels = DataHolder.GetDataModel(edge);

                            ReleaseAndReroute(dataModels);
                            break;
                        }

                    case NoRouteMessage noRouteMessage:
                        {
                            if (!SubnetworkConfig.IsDomain)
                                return Task.FromResult(true);

                            //var dataModel = DataHolder.GetDataModel(noRouteMessage.Guid);
                            //SendLinkConnectionRelease(dataModel);
                            _connectivityService.SendMessageToNCCorDomain(noRouteMessage);
                            break;
                        }

                    default:
                        _logService.LogWarning("Getting unknown SignallingMessage");
                        break;

                }
            }
            catch (Exception e)
            {
                _logService.LogError(e.StackTrace);
            }
            return Task.FromResult(true);
        }

        private void ReleaseAndReroute(List<DataModel> dataModels)
        {
            foreach (var dataModel in dataModels)
            {
                //Thread.Sleep(500);
                _logService.LogInfo($"Rerouting {dataModel.From} to {dataModel.To}");

                SendLinkConnectionRelease(dataModel);


                _logService.LogInfo($"Sending RouteTableQueryRequest from {dataModel.From} to {dataModel.To} to RC");
                x.AddSmthToLogs($"Getting RouteTableQueryRequest from {dataModel.From} to {dataModel.To}");

                x.AddSmthToLogs($"Preparing path from {dataModel.From} to {dataModel.To}");

                var route = _rcService.GetRouteTableQuery(dataModel.From,
                    dataModel.To, dataModel.Slots);

                x.AddSmthToLogs($"Sending RouteTableQueryResponse");
                _logService.LogInfo($"Getting RouteTableQueryResponse");

                var guid = dataModel.ConnectionRequestReq.First().Guid;
                if (route == null)
                {
                    _logService.LogInfo("There is no path :(");
                    _connectivityService.SendMessageToNCCorDomain(new NoRouteMessage { Guid = guid });
                    DataHolder.Data.Remove(dataModel);
                    continue;
                }

                dataModel.RouteTableQuery = route;
                _logService.LogInfo($"Computed route {string.Join(", ", route.Nodes)}");

                var startingIndex = route.Nodes.First().StartsWith("H") ? 1 : 0;

                for (var i = startingIndex; i < route.Nodes.Count - 1; i++)
                {
                    var msg = new SNPLinkConnectionRequest_req
                    {
                        From = route.Nodes[i],
                        To = route.Nodes[i + 1],
                        Slots = route.Slots,
                        Action = AllocationAction.Setup,
                        Guid = guid,
                        Rerouting = true
                    };
                    _logService.LogInfo(
                        $"Sending {msg}");
                    _connectivityService.SendMessage(msg, route.Nodes[i]);
                    dataModel.RequestSentCount = route.Nodes.Count - 1;
                    dataModel.SnpLinkConnectionRequestReq.Add(msg);
                }
            }
        }

        private void SendLinkConnectionRelease(DataModel dataModel)
        {
            foreach (var lcreq in dataModel.SnpLinkConnectionRequestRsp)
            {
                Thread.Sleep(10);
                var msg = new SNPLinkConnectionRequest_req
                {
                    From = lcreq.From,
                    To = lcreq.To,
                    Action = AllocationAction.Release,
                    FromPort = lcreq.FromPort,
                    ToPort = lcreq.ToPort,
                    Guid = lcreq.Guid,
                    RequestFromDomain = lcreq.RequestFromDomain,
                    Rerouting = lcreq.Rerouting,
                    Slots = lcreq.Slots,
                    Releasing = true
                };

                _logService.LogInfo($"Sending {msg}");
                _connectivityService.SendMessage(msg, msg.From);
            }

            Thread.Sleep(300);

            // TODO rozdzielic zeby po otrzymaniu responsow z releasem wykonalo sie to co dalej


            foreach (var rowInfo in dataModel.SentRowInfos)
            {
                rowInfo.Action = ManagementAction.DeleteEonRow;
                _logService.LogInfo($"Sending {(EonRow)(rowInfo.Row)}");
                _connectivityService.SendRowInfo(((EonRow)rowInfo.Row).Node, rowInfo);
            }

            dataModel.SentRowInfos.Clear();
            dataModel.SnpLinkConnectionRequestReq.Clear();
            dataModel.SnpLinkConnectionRequestRsp.Clear();
        }

        private void SendReleaseAndDeleteModel(ISignalingMessage signalingMessage)
        {

            var dataModel = DataHolder.GetDataModel(signalingMessage.Guid);

            if (dataModel == null)
                return;

            SendLinkConnectionRelease(dataModel);

            foreach (var connRequest in dataModel.ConnectionRequestReq)
            {
                connRequest.Action = AllocationAction.Release;
                if (SubnetworkConfig.IsDomain)
                {
                    _logService.LogInfo($"Sending {connRequest}");
                    _connectivityService.SendMessage(connRequest, _connectivityService.SubdomainName);
                }
            }

            DataHolder.Data.Remove(dataModel);
        }
    }
}
