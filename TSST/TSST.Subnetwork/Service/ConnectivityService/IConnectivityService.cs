using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSST.Shared.Model;
using TSST.Shared.Model.Messages;
using TSST.Shared.Model.Rows;

namespace TSST.Subnetwork.Service.ConnectivityService
{
    public interface IConnectivityService
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        string SubdomainName { get; set; }
        void ConnectToNCCorDomain(string ip, int port, string nodeName);
        void StartListening(string ip, int port);

        void SendMessage(ISignalingMessage message, string nodeName);
        void SendMessageToNCCorDomain(ISignalingMessage message);
        void SendRowInfo(string nodeName, RowInfo rowInfo);
    }
}
