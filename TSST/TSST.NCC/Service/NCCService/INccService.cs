using TSST.Shared.Model.Messages;

namespace TSST.NCC.Service.NCCService
{
    public interface INccService
    {
        void ConnectToNcc(string ip, int port, string nodeName);
        void StartListening(string ip, int port);

        void SendMessage(ISignalingMessage message, string nodeName);
    }
}