using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using AsyncNet.Tcp.Client;
using AsyncNet.Tcp.Remote;
using AsyncNet.Tcp.Remote.Events;
using AsyncNet.Tcp.Server;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;
using TSST.Shared.Model.Rows;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.Subnetwork.Service.ConnectivityService
{
    public class ConnectivityService : IConnectivityService
    {
        private readonly ILogService _logService;
        private readonly IObjectSerializerService _objectSerializerService;
        private IRemoteTcpPeer _client;

        public string SubdomainName { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly ConcurrentDictionary<string, IRemoteTcpPeer> _socketOfNode = new ConcurrentDictionary<string, IRemoteTcpPeer>();
        private readonly ConcurrentDictionary<IRemoteTcpPeer, string> _nodeOfSocket = new ConcurrentDictionary<IRemoteTcpPeer, string>();

        public ConnectivityService(ILogService logService, IObjectSerializerService objectSerializerService)
        {
            _logService = logService;
            _objectSerializerService = objectSerializerService;
        }

        public async void ConnectToNCCorDomain(string ip, int port, string nodeName)
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

        public void SendMessage(ISignalingMessage message, string nodeName)
        {
            var handler = _socketOfNode[nodeName];
            handler.Post(_objectSerializerService.Serialize(message));
        }

        public void SendMessageToNCCorDomain(ISignalingMessage message)
        {
            _client.Post(_objectSerializerService.Serialize(message));
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
                                SubdomainName = parts[1];

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

        private void OnDataReceived(TcpFrameArrivedEventArgs message)
        {
            var args = new MessageReceivedEventArgs { Message = (ISignalingMessage)_objectSerializerService.Deserialize(message.FrameData) };
            MessageReceived?.Invoke(this, args);
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

        private void ProcessMessage(ISignalingMessage signalingMessage)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs { Message = signalingMessage });
        }

        public void SendRowInfo(string nodeName, RowInfo rowInfo)
        {
            if (nodeName == null)
                return;

            if (!_socketOfNode.ContainsKey(nodeName))
            {
                _logService.LogWarning($"{nodeName} is not connected");
                return;
            }

            _logService.LogInfo($"Sending {rowInfo.Action} to {nodeName}");

            var handler = _socketOfNode[nodeName];

            var byteData = _objectSerializerService.Serialize(rowInfo);

            handler.Post(byteData);

        }
    }
}
