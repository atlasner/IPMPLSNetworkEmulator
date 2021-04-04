using System.Collections.Generic;
using TSST.Shared.Model;
using TSST.Shared.Model.Rows;

namespace TSST.NetworkNode.Service.RoutingService
{
    interface IRoutingService
    {
        void ProcessPackage(EonPacket packet);
        void ApplyRowInfo(RowInfo rowInfo);
        bool CheckFreeSlots(List<int> slots);
    }
}
