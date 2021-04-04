using System;
using System.Threading;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;

namespace TSST.Shared.Service.CableCloudConnectionService
{
    public interface ICableCloudConnectionService
    {
        event EventHandler<PackageReceivedEventArgs> PackageReceived;
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        void Send(EonPacket package);
        void SendSNPNegotiation(ISignalingMessage message);
        void StartClient(string ip, int port, string nodeName);
        void SendPeriodically(EonPacket package, string time, CancellationToken cancellationToken);
    }
}