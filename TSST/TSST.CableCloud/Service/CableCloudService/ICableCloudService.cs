
using System.Net.Sockets;
using AsyncNet.Tcp.Remote;
using TSST.CableCloud.Model;
using TSST.Shared.Model;

namespace TSST.CableCloud.Service.CableCloudService
{
    public interface ICableCloudService 
    {
        CableCloudConfigDto CableCloudConfig { get; }
        void StartListening();
        void SendPacket(IRemoteTcpPeer message, EonPacket package);
        void SwitchCableStatus(ForwardingInfoDto forwardingInfoDto);
    }
}