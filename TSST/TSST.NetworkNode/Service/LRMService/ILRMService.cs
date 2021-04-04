using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSST.NetworkNode.Model;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;

namespace TSST.NetworkNode.Service.LRMService
{
    public interface ILRMService
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<RowInfoReceivedEventArgs> RowInfoReceived;

        void StartClient(string ip, int port, string nodeName);
        void SendMessageToCCRC(ISignalingMessage message);

        void StartClientDomain(string ip, int port, string nodeName);

        void SendMessageToDomainCCRC(ISignalingMessage message);
    }
}
