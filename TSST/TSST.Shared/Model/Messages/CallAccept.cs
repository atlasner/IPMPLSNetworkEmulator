using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    // NCC -> CPCC
    [Serializable]
    public class CallAccept_req : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Capacity { get; set; }
        public List<int> Slots { get; set; }
        public Guid Guid { get; set; }

        public override string ToString()
        {
            return $"CallAccept_req from: {From}, to: {To}, capacity: {Capacity}, Slots: {string.Join(", ", Slots)}, guid: {Guid}";
        }
    }

    // CPCC -> NCC
    [Serializable]
    public class CallAccept_rsp : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Capacity { get; set; }
        public List<int> Slots { get; set; }
        public RequestResult Result { get; set; }
        public Guid Guid { get; set; }

        public override string ToString()
        {
            return $"CallAccept_rsp from: {From}, to: {To}, capacity: {Capacity}, Slots: {string.Join(", ", Slots)}, result: {Result}, guid: {Guid}";
        }
    }
}
