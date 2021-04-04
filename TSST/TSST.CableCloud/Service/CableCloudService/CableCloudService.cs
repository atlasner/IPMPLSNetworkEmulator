using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsyncNet.Tcp.Remote;
using AsyncNet.Tcp.Remote.Events;
using AsyncNet.Tcp.Server;
using TSST.CableCloud.Model;
using TSST.CableCloud.Service.ConfigReaderService;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.CableCloud.Service.CableCloudService
{
    public class CableCloudService : ICableCloudService
    {
        private readonly ILogService _logService;
        private readonly IObjectSerializerService _objectSerializerService;
        private readonly List<string> _ccrcList = new List<string>();
        public CableCloudConfigDto CableCloudConfig { get; }

        private readonly ConcurrentDictionary<string, IRemoteTcpPeer> _socketOfNode = new ConcurrentDictionary<string, IRemoteTcpPeer>();
        private readonly ConcurrentDictionary<IRemoteTcpPeer, string> _nodeOfSocket = new ConcurrentDictionary<IRemoteTcpPeer, string>();

        public CableCloudService(IConfigReaderService configReaderService, ILogService logService, IObjectSerializerService objectSerializerService)
        {
            _logService = logService;
            _objectSerializerService = objectSerializerService;

            try
            {
                CableCloudConfig = configReaderService.ReadFromFile();
            }
            catch (Exception e)
            {
                _logService.LogError("WRONG CONFIG: " + e.Message);
            }
        }

        public async void StartListening()
        {
            var server = new AsyncNetTcpServer(CableCloudConfig.Port);
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
                            _ccrcList.Add(parts[1]);
                        }
                        AddToTranslationDictionary(e.RemoteTcpPeer, parts);
                        break;
                    }
                    case ISignalingMessage signalingMessage:
                    {
                        ProcessMessage(e, signalingMessage);
                        break;
                    }
                    case EonPacket eonPacket:
                    {
                        ProcessPackage(e.RemoteTcpPeer, eonPacket);
                        break;
                    }
                }
            };
            await server.StartAsync(CancellationToken.None);

        }

        private void ProcessMessage(TcpFrameArrivedEventArgs e, ISignalingMessage signalingMessage)
        {

            string incomingNode;
            int incomingPort;

            switch (signalingMessage)
            {
                case SNPNegotiation_req req:
                {
                    incomingNode = _nodeOfSocket[e.RemoteTcpPeer];
                    incomingPort = req.Port;
                    var cable = GetCable(incomingNode, incomingPort);


                    if (cable == null)
                    {
                        return;
                    }

                    var nextNode = cable.Node2;
                    var nextPort = cable.Port2;

                    if (nextNode == incomingNode)
                    {
                        nextNode = cable.Node1;
                        nextPort = cable.Port1;
                    }

                    if (cable.Status)
                    {
                        req.Port = nextPort;
                        if (!_socketOfNode.ContainsKey(nextNode))
                        {
                            _logService.LogWarning($"{nextNode} is not connected to CableCloud");
                            return;
                        }
                        SendMessage(_socketOfNode[nextNode], signalingMessage);
                        _logService.LogInfo("Sending package from: " + incomingNode + ":" + incomingPort + " to: " + nextNode + ":" + nextPort);
                    }
                    else
                    {
                        _logService.LogInfo("Discarding package (cable disabled) from: " + incomingNode + ":" + incomingPort + " to: " + nextNode + ":" + nextPort);
                    }
                    break;
                }
                case SNPNegotiation_rsp rsp:
                {
                    incomingNode = _nodeOfSocket[e.RemoteTcpPeer];
                    incomingPort = rsp.Port;
                    var cable = GetCable(incomingNode, incomingPort);


                    if (cable == null)
                    {
                        return;
                    }

                    var nextNode = cable.Node2;
                    var nextPort = cable.Port2;

                    if (nextNode == incomingNode)
                    {
                        nextNode = cable.Node1;
                        nextPort = cable.Port1;
                    }

                    if (cable.Status)
                    {
                        rsp.Port = nextPort;
                        if (!_socketOfNode.ContainsKey(nextNode))
                        {
                            _logService.LogWarning($"{nextNode} is not connected to CableCloud");
                            return;
                        }
                        SendMessage(_socketOfNode[nextNode], signalingMessage);
                        _logService.LogInfo("Sending package from: " + incomingNode + ":" + incomingPort + " to: " + nextNode + ":" + nextPort);
                    }
                    else
                    {
                        _logService.LogInfo("Discarding package (cable disabled) from: " + incomingNode + ":" + incomingPort + " to: " + nextNode + ":" + nextPort);
                    }
                    break;
                    }
            }

        }

        private void AddToTranslationDictionary(IRemoteTcpPeer handler, IReadOnlyList<string> parts)
        {
            while (true)
            {
                var success = _nodeOfSocket.TryAdd(handler, parts[1]);
                if (success)
                {
                    break;
                }
                Thread.Sleep(100);
            }

            while (true)
            {
                var success = _socketOfNode.TryAdd(parts[1], handler);
                if (success)
                {
                    break;
                }
                Thread.Sleep(100);
            }
        }

        private void ProcessPackage(IRemoteTcpPeer handler, EonPacket package)
        {
            var incomingNode = _nodeOfSocket[handler];
            var incomingPort = package.Port;

            var cable = GetCable(incomingNode, incomingPort);


            _logService.LogInfo($"Received package from {incomingNode}:{incomingPort}, Content: {package.Content}");

            if (cable == null)
            {
                _logService.LogWarning($"There is no cable, from {incomingNode}:{incomingPort}");
                return;
            }


            var nextNode = cable.Node2;
            var nextPort = cable.Port2;

            if (nextNode == incomingNode)
            {
                nextNode = cable.Node1;
                nextPort = cable.Port1;
            }

            if (cable.Status)
            {
                package.Port = nextPort;
                if (!_socketOfNode.ContainsKey(nextNode))
                {
                    _logService.LogWarning($"{nextNode} is not connected to CableCloud");
                    return;
                }
                SendPacket(_socketOfNode[nextNode], package);
                _logService.LogInfo("Sending package from: " + incomingNode + ":" + incomingPort + " to: " + nextNode + ":" + nextPort);
            }
            else
            {
                _logService.LogInfo("Discarding package (cable disabled) from: " + incomingNode + ":" + incomingPort + " to: " + nextNode + ":" + nextPort);
            }
        }

        public void SendPacket(IRemoteTcpPeer handler, EonPacket package)
        {
            handler.Post(_objectSerializerService.Serialize(package));
        }

        public void SendMessage(IRemoteTcpPeer handler, ISignalingMessage message)
        {
            handler.Post(_objectSerializerService.Serialize(message));
        }


        private ForwardingInfoDto GetCable(string node, int port)
        {
            return CableCloudConfig.ForwardingTable.FirstOrDefault(c => (c.Node1 == node && c.Port1 == port) || (c.Node2 == node && c.Port2 == port));
        }

        public void SwitchCableStatus(ForwardingInfoDto forwardingInfo)
        {
            if (forwardingInfo == null)
            {
                _logService.LogWarning("You need to pick cable first.");
                return;
            }

            var cableInfo = CableCloudConfig.ForwardingTable.FirstOrDefault(c => c.Id == forwardingInfo.Id);

            if (cableInfo == null)
            {
                _logService.LogError("No cable found");
                return;
            }

            cableInfo.Status = !cableInfo.Status;
            SendCableStatusChangeInfoToLrms(cableInfo);
            _logService.LogInfo($"Switching {forwardingInfo.Id} cable status to {cableInfo.Status}");
        }

        private void SendCableStatusChangeInfoToLrms(ForwardingInfoDto cableInfo)
        {
            var message = new CableAction
            {
                From = cableInfo.Node1,
                To = cableInfo.Node2,
                Guid = Guid.NewGuid(),
                Status = cableInfo.Status
            };
            SendMessage(_socketOfNode[cableInfo.Node1], message);
            SendMessage(_socketOfNode[cableInfo.Node2], message);
        }
    }
}
