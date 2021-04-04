using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsyncNet.Tcp.Client;
using AsyncNet.Tcp.Remote;
using AsyncNet.Tcp.Remote.Events;
using TSST.NetworkNode.Model;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;
using TSST.Shared.Model.Rows;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.NetworkNode.Service.LRMService
{
    public class LRMService : ILRMService
    {
        private readonly ILogService _logService;
        private readonly IObjectSerializerService _objectSerializerService;
        private IRemoteTcpPeer _client;
        private IRemoteTcpPeer _clientDomain;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<RowInfoReceivedEventArgs> RowInfoReceived;


        public LRMService(ILogService logService, IObjectSerializerService objectSerializerService)
        {
            _logService = logService;
            _objectSerializerService = objectSerializerService;
        }

        public async void StartClient(string ip, int port, string nodeName)
        {
            var client = new AsyncNetTcpClient(ip, port);
            client.ConnectionEstablished += (s, e) =>
            {
                _client = e.RemoteTcpPeer;
                _logService.LogInfo($"Connected to CC and RC");

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

        public async void StartClientDomain(string ip, int port, string nodeName)
        {
            var clientDomain = new AsyncNetTcpClient(ip, port);
            clientDomain.ConnectionEstablished += (s, e) =>
            {
                _clientDomain = e.RemoteTcpPeer;
                _logService.LogInfo($"Connected to Domain CC and RC");

                var hello = $"INIT {nodeName}";
                var bytes = _objectSerializerService.Serialize(hello);
                _clientDomain.Post(bytes);
            };
            clientDomain.FrameArrived += (s, e) =>
            {
                OnDataReceived(e);
            };
            await clientDomain.StartAsync(CancellationToken.None);
        }

        public void SendMessageToCCRC(ISignalingMessage message)
        {
            _client.Post(_objectSerializerService.Serialize(message));
        }

        public void SendMessageToDomainCCRC(ISignalingMessage message)
        {
            _logService.LogInfo("Sending message to Domain");
            _clientDomain.Post(_objectSerializerService.Serialize(message));
        }

        private void OnDataReceived(TcpFrameArrivedEventArgs message)
        {
            var deserialized = _objectSerializerService.Deserialize(message.FrameData);
            switch (deserialized)
            {
                case ISignalingMessage signalingMessage:
                {
                    var args = new MessageReceivedEventArgs
                        {Message = signalingMessage};
                    MessageReceived?.Invoke(this, args);
                    break;
                }
                case RowInfo rowInfo:
                {
                    var args = new RowInfoReceivedEventArgs
                    {
                        RowInfo = rowInfo
                    };
                    RowInfoReceived?.Invoke(this, args);
                    break;
                }
            }
        }
    }
}
