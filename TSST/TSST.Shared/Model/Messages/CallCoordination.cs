using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    // NCC -> NCC
    [Serializable]
    public class CallCoordination_req : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public string StartingNode { get; set; }
        public int Capacity { get; set; }
        public List<int> Slots { get; set; }
        public AllocationAction Action { get; set; }
        public Guid Guid { get; set; }
        public override string ToString()
        {
            return
                $"CallCoordination_req from: {From}, to: {To}, startingNode: {StartingNode}, Action: {Action}, Slots: {string.Join(", ", Slots)}, guid: {Guid}";
        }
    }

    [Serializable]
    public class CallCoordination_rsp : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public string StartingNode { get; set; }
        public List<int> Slots { get; set; }
        public Guid Guid { get; set; }
        public RequestResult Result { get; set; }
        public int Capacity { get; set; }

        public override string ToString()
        {
            return
                $"CallCoordination_rsp from: {From}, to: {To}, startingNode: {StartingNode}, Action: {Result}, Slots: {string.Join(", ", Slots)}, result: {Result}, guid: {Guid}";
        }
    }
}
