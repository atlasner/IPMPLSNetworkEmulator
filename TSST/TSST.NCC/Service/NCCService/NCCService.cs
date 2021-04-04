using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using AsyncNet.Tcp.Client;
using AsyncNet.Tcp.Remote;
using AsyncNet.Tcp.Remote.Events;
using AsyncNet.Tcp.Server;
using TSST.NCC.Model;
using TSST.NCC.Service.ConfigReaderService;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;
using TSST.Subnetwork.Service.DataHolder;

namespace TSST.NCC.Service.NCCService
{
    public class NccService : INccService
    {
        private readonly ILogService _logService;
        private readonly IObjectSerializerService _objectSerializerService;

        private NccConfigDto _nccConfig;
        private IRemoteTcpPeer _client;
        private string _domainCcrcName;
        private string _secondNccName;

        private readonly ConcurrentDictionary<string, Socket> _socketOfNode = new ConcurrentDictionary<string, Socket>();
        private readonly ConcurrentDictionary<Socket, string> _nodeOfSocket = new ConcurrentDictionary<Socket, string>();

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;


        public NccService(ILogService logService, IObjectSerializerService objectSerializerService, IConfigReaderService configReaderService)
        {
            _logService = logService;
            _objectSerializerService = objectSerializerService;
            _nccConfig = configReaderService.ReadNccConfig();

        }

        public async void ConnectToNcc(string ip, int port, string nodeName)
        {
            var client = new AsyncNetTcpClient(ip, port);
            client.ConnectionEstablished += (s, e) =>
            {
                _client = e.RemoteTcpPeer;
                Console.WriteLine($"Connected to NCC");

                var hello = $"INIT {nodeName}";
                var bytes = _objectSerializerService.Serialize(hello);
                _client.Post(bytes);
            };
            client.FrameArrived += (s, e) =>
            {
                OnDataReceived(e);
            };
            await client.StartAsync(CancellationToken.None);
        }

        public async void StartListening(string ip, int port)
        {
            var server = new AsyncNetTcpServer(port);
            server.ServerStarted += (s, e) => Console.WriteLine($"Server started on port: " +
                                                                $"{e.ServerPort}");
            server.ConnectionEstablished += (s, e) =>
            {
            };
            server.FrameArrived += (s, e) =>
            {
                var message = _objectSerializerService.Deserialize(e.FrameData);

                switch (message)
                {
                    case string init:
                    {
                        var parts = init.Split(' ');
                        _logService.LogInfo($"Connected with {parts[1]}");

                        if (parts[1].StartsWith("CCRC"))
                        {
                            _domainCcrcName = parts[1];
                        }
                        else if (parts[1].StartsWith("NCC"))
                        {
                            _secondNccName = parts[1];
                        }
                        AddToTranslationDictionary(e.RemoteTcpPeer, parts);
                        break;
                    }
                    case ISignalingMessage signalingMessage:
                    {
                        ProcessMessage(signalingMessage);
                        break;
                    }
                }
            };
            await server.StartAsync(CancellationToken.None);
        }


        private void AddToTranslationDictionary(IRemoteTcpPeer handler, IReadOnlyList<string> parts)
        {
            while (true)
            {
                var success = _nodeOfSocket.TryAdd(handler.TcpClient.Client, parts[1]);
                if (success)
                {
                    break;
                }
                Thread.Sleep(100);
            }

            while (true)
            {
                var success = _socketOfNode.TryAdd(parts[1], handler.TcpClient.Client);
                if (success)
                {
                    break;
                }
                Thread.Sleep(100);
            }
        }

        public void SendMessage(ISignalingMessage message, string nodeName)
        {
            var handler = _socketOfNode[nodeName];
            handler.Send(_objectSerializerService.Serialize(message));
        }

        public void SendMessageToMaster(ISignalingMessage message)
        {
            _client.Post(_objectSerializerService.Serialize(message));
        }

        private void ProcessMessage(ISignalingMessage signalingMessage) // wiadomosc przychodzaca do NCC jako do serwera (MASTER)
        {
            try
            {
                switch (signalingMessage)
                {
                    case CallRequest_req callRequestReq:
                        {
                            _logService.LogInfo(
                                $"Getting {callRequestReq}");

                            // TODO CHECK ALL POSSIBILITIES
                            if (DataHolder.GetDataModel(callRequestReq.To).Count > 0)
                            {
                                var callRequestRsp = new CallRequest_rsp()
                                {
                                    From = callRequestReq.From,
                                    To = callRequestReq.To,
                                    Capacity = callRequestReq.Capacity,
                                    Guid = callRequestReq.Guid,
                                    Result = RequestResult.Rejected,
                                    Slots = new List<int>()
                                };
                                SendMessage(callRequestRsp, "CPCC" + callRequestReq.From[1]);
                                return;
                            }
                            // TODO

                            var directoryEntry = DirectoryGetToNode(callRequestReq.To);
                            var node = directoryEntry.ToNode;

                            var newModel = new DataModel
                            {
                                From = callRequestReq.From,
                                To = callRequestReq.To,
                                Capacity = callRequestReq.Capacity
                            };

                            var connectionRequestReq = new ConnectionRequest_req
                            {
                                AddressFrom = callRequestReq.From,
                                AddressTo = node,
                                Capacity = callRequestReq.Capacity,
                                Action = AllocationAction.Setup,
                                Guid = callRequestReq.Guid,
                                EstimatedDistance = directoryEntry.EstimatedDistance
                            };

                            newModel.CallRequestReq.Add(callRequestReq);

                            newModel.ConnectionRequestReq.Add(connectionRequestReq);
                            DataHolder.Data.Add(newModel);

                            _logService.LogInfo(
                                $"Sending ConnectionRequest_req to {_domainCcrcName}, AddresFrom: {callRequestReq.From}, AddressTo: {node}, Capacity: {callRequestReq.Capacity}, Action: {AllocationAction.Setup}, guid: {callRequestReq.Guid}");
                            SendMessage(connectionRequestReq, _domainCcrcName);
                            break;
                        }
                    case ConnectionRequest_rsp connectionRequestRsp:
                        {
                            _logService.LogInfo(
                                $"Getting {connectionRequestRsp}");

                            var datamodel = DataHolder.GetDataModel(connectionRequestRsp.Guid);

                            var client = DirectoryGetToNode(datamodel.To);
                            
                            var dataModel = DataHolder.GetDataModel(connectionRequestRsp.Guid);
                            dataModel.ConnectionRequestRsp.Add(connectionRequestRsp);

                            if (connectionRequestRsp.Result == RequestResult.Confirmed)
                            {

                                if (client.LocalClient)
                                {
                                    var callAcceptReq = new CallAccept_req
                                    {
                                        From = datamodel.From,
                                        To = datamodel.To,
                                        Capacity = connectionRequestRsp.Capacity,
                                        Slots = connectionRequestRsp.Slots,
                                        Guid = connectionRequestRsp.Guid
                                    };

                                    datamodel.CallAcceptReq.Add(callAcceptReq);
                                    _logService.LogInfo($"Sending {callAcceptReq}");
                                    SendMessage(callAcceptReq, "CPCC" + datamodel.To[1]);

                                    return;
                                }

                                var msg = new CallCoordination_req
                                {
                                    From = dataModel.From,
                                    To = dataModel.To,
                                    Guid = connectionRequestRsp.Guid,
                                    Capacity = connectionRequestRsp.Capacity,
                                    Slots = connectionRequestRsp.Slots,
                                    StartingNode = connectionRequestRsp.AddressTo
                                };


                                dataModel.CallCoordinationReq.Add(msg);

                                if (_nccConfig.DomainSlave)
                                {
                                    _logService.LogInfo($"Sending {msg} to NCC1");
                                    _client.Post(_objectSerializerService.Serialize(msg));

                                }
                                else
                                {
                                    _logService.LogInfo($"Sending {msg} to {_secondNccName}");
                                    SendMessage(msg, _secondNccName);
                                }
                            }
                            else
                            {
                                if (dataModel.CallCoordinationReq.Count == 0)
                                {
                                    var req = dataModel.CallRequestReq.First();
                                    var message = new CallRequest_rsp
                                    {
                                        Capacity = req.Capacity,
                                        From = req.From,
                                        Guid = req.Guid,
                                        Result = RequestResult.Rejected,
                                        To = req.To
                                    };
                                    SendMessage(message, "CPCC" + req.From[1]);
                                }
                                else
                                {
                                    var req = dataModel.CallCoordinationReq.First();
                                    var message = new CallCoordination_rsp
                                    {
                                        Capacity = req.Capacity,
                                        From = req.From,
                                        Guid = req.Guid,
                                        Result = RequestResult.Rejected,
                                        Slots = req.Slots,
                                        StartingNode = req.StartingNode,
                                        To = req.To
                                    };
                                    if (_nccConfig.DomainSlave)
                                    {
                                        SendMessageToMaster(message);
                                    }
                                    else
                                    {
                                        SendMessage(message, _secondNccName);
                                    }
                                }
                                DataHolder.Data.Remove(dataModel);
                            }

                            break;
                        }
                    case CallCoordination_req callCoordinationReq:
                        {
                            _logService.LogInfo($"Getting {callCoordinationReq}");

                            // TODO CHECK ALL POSSIBILITIES
                            if (DataHolder.GetDataModel(callCoordinationReq.To).Count > 0)
                            {
                                var callCoordinationRsp = new CallCoordination_rsp
                                {
                                    From = callCoordinationReq.From,
                                    To = callCoordinationReq.To,
                                    Capacity = callCoordinationReq.Capacity,
                                    Guid = callCoordinationReq.Guid,
                                    Result = RequestResult.Rejected,
                                    Slots = new List<int>(),
                                    StartingNode = "H0"
                                };

                                _logService.LogInfo($"Sending {callCoordinationRsp}");

                                if (_nccConfig.DomainSlave)
                                {
                                    SendMessageToMaster(callCoordinationRsp);
                                }
                                else
                                {
                                    SendMessage(callCoordinationRsp, _secondNccName);
                                }

                                return;
                            }
                            // TODO

                            var dataModel = new DataModel
                            {
                                From = callCoordinationReq.From,
                                To = callCoordinationReq.To,
                                Slots = callCoordinationReq.Slots
                            };

                            var connectionRequest = new ConnectionRequest_req()
                            {
                                Action = AllocationAction.Setup,
                                AddressFrom = callCoordinationReq.StartingNode,
                                AddressTo = callCoordinationReq.To,
                                Capacity = callCoordinationReq.Capacity,
                                Guid = callCoordinationReq.Guid,
                                Slots = callCoordinationReq.Slots
                            };

                            dataModel.ConnectionRequestReq.Add(connectionRequest);
                            DataHolder.Data.Add(dataModel);

                            _logService.LogInfo($"Sending {connectionRequest}");
                            SendMessage(connectionRequest, _domainCcrcName);
                            break;
                        }
                    case CallAccept_rsp callAcceptRsp:
                        {
                            _logService.LogInfo($"Getting {callAcceptRsp}");
                            var dataModel = DataHolder.GetDataModel(callAcceptRsp.Guid);
                            dataModel.CallAcceptRsp.Add(callAcceptRsp);

                            if (!DirectoryGetToNode(callAcceptRsp.To).LocalClient || !DirectoryGetToNode(callAcceptRsp.From).LocalClient)
                            {
                                var callCoordinationRsp = new CallCoordination_rsp
                                {
                                    From = callAcceptRsp.From,
                                    Guid = callAcceptRsp.Guid,
                                    Result = callAcceptRsp.Result,
                                    Slots = callAcceptRsp.Slots,
                                    To = callAcceptRsp.To
                                };

                                _logService.LogInfo($"Sending {callCoordinationRsp}");

                                if (_nccConfig.DomainSlave)
                                {
                                    SendMessageToMaster(callCoordinationRsp);
                                }
                                else
                                {
                                    SendMessage(callCoordinationRsp, _secondNccName);
                                }
                            }
                            else
                            {
                                var callRequestRsp = new CallRequest_rsp
                                {
                                    Capacity = callAcceptRsp.Capacity,
                                    From = callAcceptRsp.From,
                                    Guid = callAcceptRsp.Guid,
                                    Slots = callAcceptRsp.Slots,
                                    To = callAcceptRsp.To
                                };

                                if (callAcceptRsp.Result == RequestResult.Confirmed)
                                {
                                    callRequestRsp.Result = RequestResult.Confirmed;
                                }
                                else
                                {
                                    callRequestRsp.Result = RequestResult.Rejected;
                                    SendReleaseAndDeleteModel(callAcceptRsp);
                                }

                                _logService.LogInfo($"Sending {callRequestRsp}");
                                SendMessage(callRequestRsp, "CPCC" + callAcceptRsp.From[1]);
                            }

                            // TODO: CHECK
                            if (callAcceptRsp.Result == RequestResult.Rejected)
                            {
                                SendReleaseAndDeleteModel(callAcceptRsp);
                            }

                            break;
                        }
                    case CallCoordination_rsp callCoordinationRsp:
                        {
                            _logService.LogInfo($"Getting {callCoordinationRsp}");

                            var callRequestRsp = new CallRequest_rsp
                            {
                                Capacity = callCoordinationRsp.Capacity,
                                From = callCoordinationRsp.From,
                                Guid = callCoordinationRsp.Guid,
                                Slots = callCoordinationRsp.Slots,
                                To = callCoordinationRsp.To
                            };

                            DataHolder.GetDataModel(callCoordinationRsp.Guid).CallCoordinationRsp.Add(callCoordinationRsp);

                            if (callCoordinationRsp.Result == RequestResult.Confirmed)
                            {
                                callRequestRsp.Result = RequestResult.Confirmed;
                            }
                            else
                            {
                                callRequestRsp.Result = RequestResult.Rejected;
                                SendReleaseAndDeleteModel(callCoordinationRsp);
                            }

                            _logService.LogInfo($"Sending {callRequestRsp}");
                            SendMessage(callRequestRsp, "CPCC" + callCoordinationRsp.From[1]);

                            break;
                        }
                    case CallTeardown_req callTeardownReq:
                        {
                            var dataModel = DataHolder.GetDataModel(callTeardownReq.Guid);
                            if (dataModel == null)
                                return;

                            _logService.LogInfo($"Getting {callTeardownReq}");


                            _logService.LogInfo($"Sending callTeardownReq to {callTeardownReq.To}");
                            if (DirectoryGetToNode(callTeardownReq.To).LocalClient)
                            {
                                SendMessage(callTeardownReq, "CPCC" + callTeardownReq.To[1]);
                            }
                            else
                            {

                                if (_nccConfig.DomainSlave)
                                {
                                    SendMessageToMaster(callTeardownReq);
                                }
                                else
                                {
                                    SendMessage(callTeardownReq, _secondNccName);
                                }
                            }



                            SendReleaseAndDeleteModel(callTeardownReq);

                            break;
                        }
                    case NoRouteMessage noRouteMessage:
                        {
                            _logService.LogInfo($"Getting {nameof(NoRouteMessage)}");
                            var dataModel = DataHolder.GetDataModel(noRouteMessage.Guid);

                            if (dataModel == null)
                                return;

                            _logService.LogInfo($"Sending {nameof(NoRouteMessage)}");

                            if (DirectoryGetToNode(dataModel.To).LocalClient)
                            {
                                SendMessage(noRouteMessage, "CPCC" + dataModel.To[1]);
                            }
                            if(DirectoryGetToNode(dataModel.From).LocalClient)
                            {
                                SendMessage(noRouteMessage, "CPCC" + dataModel.From[1]);
                            }

                            if (_nccConfig.DomainSlave)
                            {
                                SendMessageToMaster(noRouteMessage);
                            }
                            else
                            {
                                SendMessage(noRouteMessage, _secondNccName);
                            }

                            SendReleaseAndDeleteModel(noRouteMessage);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                _logService.LogError(e.StackTrace);
            }
        }

        private void SendReleaseAndDeleteModel(ISignalingMessage signalingMessage)
        {
            var dataModel = DataHolder.GetDataModel(signalingMessage.Guid);
            if (dataModel == null)
                return;

            foreach (var connRequest in dataModel.ConnectionRequestReq)
            {
                connRequest.Action = AllocationAction.Release;
                _logService.LogInfo($"Sending {connRequest}");
                SendMessage(connRequest, _domainCcrcName);
            }

            DataHolder.Data.Remove(dataModel);
        }

        private DirectoryEntry DirectoryGetToNode(string toNode)
        {
            return _nccConfig.Directory.FirstOrDefault(d => d.Name == toNode);
        }

        private void OnDataReceived(TcpFrameArrivedEventArgs message)
        {
            var args = new MessageReceivedEventArgs { Message = (ISignalingMessage)_objectSerializerService.Deserialize(message.FrameData) };
            ProcessMessage((ISignalingMessage)_objectSerializerService.Deserialize(message.FrameData));
            MessageReceived?.Invoke(this, args);
        }
    }
}
