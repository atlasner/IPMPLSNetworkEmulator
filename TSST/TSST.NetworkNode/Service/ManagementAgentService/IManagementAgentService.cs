using System;
using TSST.NetworkNode.Model;

namespace TSST.NetworkNode.Service.ManagementAgentService
{
    public interface IManagementAgentService
    {
        event EventHandler<RowInfoReceivedEventArgs> RowInfoReceived;
        void StartClient(string ip, int port, string nodeName);
    }
}
