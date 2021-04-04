using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncNet.Tcp.Client;
using AsyncNet.Tcp.Remote;
using AsyncNet.Tcp.Remote.Events;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.Shared.Service.CableCloudConnectionService
{
    public sealed class CableCloudConnectionService : ICableCloudConnectionService
    {
        private readonly IObjectSerializerService _objectSerializerService;
        private readonly ILogService _logService;

        private IRemoteTcpPeer _client;

        public CableCloudConnectionService(IObjectSerializerService objectSerializerService, ILogService logService)
        {
            _objectSerializerService = objectSerializerService;
            _logService = logService;
        }
        public event EventHandler<PackageReceivedEventArgs> PackageReceived;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Send(EonPacket package)
        {
            _logService.LogInfo($"Sending {package.Content}");
            _client.Post(_objectSerializerService.Serialize(package));
        }

        public void SendSNPNegotiation(ISignalingMessage request)
        {
            _client.Post(_objectSerializerService.Serialize(request));
        }

        public async void StartClient(string ip, int port, string nodeName)
        {
            var client = new AsyncNetTcpClient(ip, port);
            client.ConnectionEstablished += (s, e) =>
            {
                _client = e.RemoteTcpPeer;
                _logService.LogInfo($"Connected to CableCloud");

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

        public async void SendPeriodically(EonPacket package, string time, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                Send(package);
                var timeDelay = int.Parse(time.TrimEnd('s'));

                await Task.Delay(timeDelay * 1000);
            }
        }

        private void OnDataReceived(TcpFrameArrivedEventArgs message)
        {
            var deserialized = _objectSerializerService.Deserialize(message.FrameData);
            if (deserialized is EonPacket eonPacket)
            {
                var args = new PackageReceivedEventArgs { Packet = eonPacket };
                PackageReceived?.Invoke(this, args);
            } else if (deserialized is ISignalingMessage signalingMessage)
            {
                var args = new MessageReceivedEventArgs { Message = signalingMessage };
                MessageReceived?.Invoke(this, args);
            }
        }
    }
}