using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    // NCC -> DomainCC lub DomainCC -> SubnetworkCC
    [Serializable]
    public class ConnectionRequest_req : ISignalingMessage
    {
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }
        public int Capacity { get; set; }
        public List<int> Slots { get; set; } = new List<int>();
        public AllocationAction Action { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();

        public int EstimatedDistance { get; set; } = 100;

        public override string ToString()
        {
            return
                $"ConnectionRequest_req from: {AddressFrom}, to: {AddressTo}, capacity: {Capacity}, Slots: {string.Join(", ", Slots)}, action: {Action}, guid: {Guid}";
        }
    }

    [Serializable]
    public class ConnectionRequest_rsp : ISignalingMessage
    {
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }
        public int Capacity { get; set; }
        public List<int> Slots { get; set; }
        public RequestResult Result { get; set; }
        public Guid Guid { get; set; }

        public override string ToString()
        {
            return
                $"ConnectionRequest_rsp from: {AddressFrom}, to: {AddressTo}, capacity: {Capacity}, Slots: {string.Join(", ", Slots)}, result: {Result}, guid: {Guid}";
        }
    }
}
