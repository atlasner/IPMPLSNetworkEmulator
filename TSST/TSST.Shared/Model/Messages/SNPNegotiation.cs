using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    // LRM A -> LRM Z
    [Serializable]
    public class SNPNegotiation_req : ISignalingMessage
    {
        public string From { get; set; }
        public int FromPort { get; set; }
        public int ToPort { get; set; }
        public string To { get; set; }
        public List<int> Slots { get; set; }
        public int Port { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public AllocationAction Action { get; set; }
        public bool RequestFromDomain { get; set; } = false;
        public bool Rerouting { get; set; } = false;
        public bool Releasing { get; set; } = false;

        public override string ToString()
        {
            return
                $"SNPNegotiation_req from: {From + ":" + FromPort}, to: {To + ":" + ToPort}, Slots: {string.Join(", ", Slots)}, guid: {Guid}";
        }
    }

    // LRM Z -> LRM A
    [Serializable]
    public class SNPNegotiation_rsp : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public List<int> Slots { get; set; }

        public RequestResult Result { get; set; }
        public int Port { get; set; }
        public Guid Guid { get; set; }
        public int ToPort { get; set; }
        public int FromPort { get; set; }

        public bool RequestFromDomain { get; set; }
        public bool Rerouting { get; set; } = false;
        public bool Releasing { get; set; } = false;


        public override string ToString()
        {
            return
                $"SNPNegotiation_rsp from: {From + ":" + FromPort}, to: {To + ":" + ToPort}, Slots: {string.Join(", ", Slots)}, guid: {Guid}";
        }
    }
}
