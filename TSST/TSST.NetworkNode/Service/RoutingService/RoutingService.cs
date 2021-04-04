using System.Collections.Generic;
using System.Linq;
using TSST.Shared.Model;
using TSST.Shared.Model.Rows;
using TSST.Shared.Service.CableCloudConnectionService;
using TSST.Shared.Service.LogService;

namespace TSST.NetworkNode.Service.RoutingService
{
    class RoutingService : IRoutingService
    {
        private readonly ILogService _logService;
        private readonly ICableCloudConnectionService _cableCloudConnectionService;

        private readonly List<EonRow> _eonRows = new List<EonRow>();
        private readonly List<bool> _slots = new List<bool>();
        private readonly int _slotCount = 64;

        public void ProcessPackage(EonPacket packet)
        {
            _logService.LogInfo("Processing packet");

            var row = GetMatchingRow(packet);

            packet.Port = row.OutPort;

            _cableCloudConnectionService.Send(packet);

            _logService.LogInfo("Packet processed");
        }

        public void ApplyRowInfo(RowInfo rowInfo)
        {
            _logService.LogInfo($"Getting RowInfo: {rowInfo.Action}");

            switch (rowInfo.Action)
            {
                case ManagementAction.AddEonRow:
                    if (rowInfo.Row is EonRow eonRow)
                    {
                        _eonRows.Add(eonRow);
                        for (var i = eonRow.FirstSlotIndex; i <= eonRow.LastSlotIndex; i++)
                            _slots[i] = true;
                        _logService.LogInfo("Adding " + eonRow);
                    }

                    break;
                case ManagementAction.DeleteEonRow:
                    if (rowInfo.Row is EonRow eonRowD)
                    {
                        var row = _eonRows.FirstOrDefault(r => r.Node == eonRowD.Node && r.FirstSlotIndex == eonRowD.FirstSlotIndex && r.LastSlotIndex == eonRowD.LastSlotIndex && r.OutPort == eonRowD.OutPort);
                        _eonRows.Remove(row);

                        for (var i = eonRowD.FirstSlotIndex; i <= eonRowD.LastSlotIndex; i++)
                            _slots[i] = false;
                        _logService.LogInfo("Removing " + eonRowD);
                    }
                    break;
            }
        }

        private EonRow GetMatchingRow(EonPacket packet)
        {
            return _eonRows.FirstOrDefault(r =>
                r.FirstSlotIndex == packet.OccupiedSlots.First() && r.LastSlotIndex == packet.OccupiedSlots.Last()/* &&
                r.IncomingPort == packet.Port*/);
        }

        public bool CheckFreeSlots(List<int> slots)
        {
            //return slots.All(slot => !_slots[slot]);
            return true;
        }

        public RoutingService(ILogService logService, ICableCloudConnectionService cableCloudConnectionService)
        {
            _logService = logService;
            _cableCloudConnectionService = cableCloudConnectionService;

            for(var i = 0; i < _slotCount; i++)
            {
                _slots.Add(false);
            }
        }
    }
}
