using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    // CPCC -> NCC
    [Serializable]
    public class CallRequest_req : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Capacity { get; set; }

        public Guid Guid { get; set; } = Guid.NewGuid();

        public override string ToString()
        {
            return $"CallRequest_req from: {From}, to: {To}, capacity: {Capacity}, guid: {Guid}";
        }
    }

    [Serializable]
    public class CallRequest_rsp : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Capacity { get; set; }
        public RequestResult Result { get; set; }

        public Guid Guid { get; set; } = Guid.NewGuid();
        public List<int> Slots { get; set; } = new List<int>();

        public override string ToString()
        {
            return $"CallRequest_rsp from: {From}, to: {To}, result: {Result}, capacity: {Capacity}, guid: {Guid}";
        }
    }
}
