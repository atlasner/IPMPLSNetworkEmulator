using System;
using System.Threading;
using AsyncNet.Tcp.Client;
using AsyncNet.Tcp.Remote;
using AsyncNet.Tcp.Remote.Events;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.Host.Service.CPCCService
{
    public class CpccService : ICPCCService
    {
        private readonly IObjectSerializerService _objectSerializerService;
        private readonly ILogService _logService;

        private IRemoteTcpPeer _client;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;



        public CpccService(IObjectSerializerService objectSerializerService, ILogService logService)
        {
            _objectSerializerService = objectSerializerService;
            _logService = logService;
            _logService.LogInfo("Starting CPCC");
        }

        public async void ConnectToNcc(string ip, int port, string nodeName)
        {
            var client = new AsyncNetTcpClient(ip, port);
            client.ConnectionEstablished += (s, e) =>
            {
                _client = e.RemoteTcpPeer;
                Console.WriteLine($"Connected to CableCloud");

                var hello = $"INIT {nodeName}";
                var bytes = _objectSerializerService.Serialize(hello);
                _client.Post(bytes);
            };
            client.FrameArrived += (s, e) =>
            {
                OnDataReceived(e);
                //Console.WriteLine($"Client received: " +
                //                  $"{System.Text.Encoding.UTF8.GetString(e.FrameData)}");
            };
            await client.StartAsync(CancellationToken.None);

            //_client = new SimpleTcpClient();
            //_client.Connect(ip, port);

            //_logService.LogInfo("Connected to NCC");

            //_client.Write($"INIT {nodeName}");

            //_client.DataReceived += OnDataReceived;
        }



        public void SendMessage(ISignalingMessage signalingMessage)
        {
            //_logService.LogInfo("Sending signalling message to NCC");
            _client.Post(_objectSerializerService.Serialize(signalingMessage));
        }

        private void OnDataReceived(TcpFrameArrivedEventArgs message)
        {
            var args = new MessageReceivedEventArgs { Message = (ISignalingMessage)_objectSerializerService.Deserialize(message.FrameData) };
            MessageReceived?.Invoke(this, args);
        }

        public Guid SendCallRequest(string fromNode, string toNode, int capacity)
        {
            _logService.LogInfo("Sending CallRequest to NCC");
            var message = new CallRequest_req { From = fromNode, To = toNode, Capacity = capacity};
            SendMessage(message);
            return message.Guid;
        }

        public void SendCallTeardownRequest(string fromNode, string toNode, Guid currentGuid)
        {
            var message = new CallTeardown_req {From = fromNode, To = toNode, Guid = currentGuid};
            _logService.LogInfo($"Sending {message}");
            SendMessage(message);
        }
    }
}
