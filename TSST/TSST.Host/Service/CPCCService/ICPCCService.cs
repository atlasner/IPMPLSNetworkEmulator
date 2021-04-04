using System;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;

namespace TSST.Host.Service.CPCCService
{
    public interface ICPCCService
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        void ConnectToNcc(string ip, int port, string nodeName);
        void SendMessage(ISignalingMessage message);
        Guid SendCallRequest(string fromNode, string toNode, int capacity);
        void SendCallTeardownRequest(string fromNode, string toNode, Guid currentGuid);
    }
}