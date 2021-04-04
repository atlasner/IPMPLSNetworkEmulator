using System;
using SimpleTCP;
using TSST.NetworkNode.Model;
using TSST.Shared.Model.Rows;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.NetworkNode.Service.ManagementAgentService
{
    public sealed class ManagementAgentService : IManagementAgentService
    {
        private readonly ILogService _logService;
        private readonly IObjectSerializerService _objectSerializerService;
        private SimpleTcpClient _client;

        public event EventHandler<RowInfoReceivedEventArgs> RowInfoReceived;

        public ManagementAgentService(ILogService logService, IObjectSerializerService objectSerializerService)
        {
            _logService = logService;
            _objectSerializerService = objectSerializerService;
        }

        public void StartClient(string ip, int port, string nodeName)
        {
            _client = new SimpleTcpClient();
            _client.Connect(ip, port);
            _client.Write($"INIT {nodeName}");

            _logService.LogInfo("Connected do Management");

            _client.DataReceived += OnRowInfoReceived;
        }

        private void OnRowInfoReceived(object sender, Message message)
        {
            var args = new RowInfoReceivedEventArgs { RowInfo = (RowInfo)_objectSerializerService.Deserialize(message.Data) };
            RowInfoReceived?.Invoke(this, args);
        }
    }
}
