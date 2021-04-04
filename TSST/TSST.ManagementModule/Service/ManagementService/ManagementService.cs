using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SimpleTCP;
using TSST.ManagementModule.Model;
using TSST.ManagementModule.Service.ConfigReaderService;
using TSST.Shared.Model.Rows;
using TSST.Shared.Service.LogService;
using TSST.Shared.Service.ObjectSerializerService;

namespace TSST.ManagementModule.Service.ManagementService
{
    public class ManagementService : IManagementService
    {
        private readonly ILogService _logService;
        private readonly IObjectSerializerService _objectSerializerService;

        public ManagementModuleConfigDto ManagementConfig { get; }

        private readonly ConcurrentDictionary<string, Socket> _socketOfNode = new ConcurrentDictionary<string, Socket>();
        private readonly ConcurrentDictionary<Socket, string> _nodeOfSocket = new ConcurrentDictionary<Socket, string>();


        public ManagementService(IConfigReaderService configReaderService, ILogService logService, IObjectSerializerService objectSerializerService)
        {
            _logService = logService;
            _objectSerializerService = objectSerializerService;

            try
            {
                ManagementConfig = configReaderService.ReadFromFile();
            }
            catch (Exception e)
            {
                _logService.LogError("WRONG CONFIG: " + e.Message);
            }
        }

        public void AddTableRow(IRow row)
        {
            if (row == null)
                return;

            var rowInfo = new RowInfo();

            switch (row)
            {
                case EonRow eonRow:
                    rowInfo.Action = ManagementAction.AddEonRow;
                    rowInfo.Row = eonRow;
                    var node = eonRow.Node;

                    SendRowInfo(node, rowInfo);
                    ManagementConfig.EonRows.Add(eonRow);
                    break;
            }
        }

        public void StartListeningAsync()
        {
            var server = new SimpleTcpServer();
            server.Start(ManagementConfig.Port, IPAddress.Parse(ManagementConfig.Ip).AddressFamily);
            server.DataReceived += DataReceived;
        }

        private void DataReceived(object sender, Message message)
        {

            var parts = message.MessageString.Split(' ');
            AddToTranslationDictionary(message, parts);
            _logService.LogInfo("Connected with node " + parts[1]);

            SendAllRowsToNode(parts[1]);
        }

        public void DeleteTableRow(ref IRow row)
        {
            var rowInfo = new RowInfo();

            switch (row)
            {
                case EonRow eonRow:
                    rowInfo.Action = ManagementAction.DeleteEonRow;
                    rowInfo.Row = eonRow;
                    var node = eonRow.Node;

                    SendRowInfo(node, rowInfo);

                    ManagementConfig.EonRows.Remove(eonRow);
                    break;
            }
        }

        private void SendAllRowsToNode(string nodeName)
        {
            _logService.LogInfo("Sending rows to node");
            foreach (var row in ManagementConfig.EonRows.Where(r => r.Node == nodeName))
            {
                var rowInfo = new RowInfo {Action = ManagementAction.AddEonRow, Row = row};
                SendRowInfo(nodeName, rowInfo);
                Thread.Sleep(50);
            }
        }

        private void AddToTranslationDictionary(Message handler, string[] parts)
        {
            while (true)
            {
                var success = _nodeOfSocket.TryAdd(handler.TcpClient.Client, parts[1]);
                if (success)
                {
                    break;
                }
                Thread.Sleep(100);
            }

            while (true)
            {
                var success = _socketOfNode.TryAdd(parts[1], handler.TcpClient.Client);
                if (success)
                {
                    break;
                }
                Thread.Sleep(100);
            }
        }

        private void SendRowInfo(string nodeName, RowInfo rowInfo)
        {
            if (nodeName == null)
                return;

            if (!_socketOfNode.ContainsKey(nodeName))
            {
                _logService.LogWarning($"{nodeName} is not connected");
                return;
            }

            _logService.LogInfo($"Sending {rowInfo.Action} to {nodeName}");

            var handler = _socketOfNode[nodeName];

            var byteData = _objectSerializerService.Serialize(rowInfo);

            handler.Send(byteData);

        }
    }
}
