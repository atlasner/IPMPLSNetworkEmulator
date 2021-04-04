using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    // CC -> LRM
    [Serializable]
    public class SNPLinkConnectionRequest_req : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public List<int> Slots { get; set; }
        public AllocationAction Action { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public int FromPort { get; set; }
        public int ToPort { get; set; }

        public bool RequestFromDomain { get; set; } = false;
        public bool Rerouting { get; set; } = false;
        public bool Releasing { get; set; } = false;

        public override string ToString()
        {
            return
                $"SNPLinkConnectionRequest_req from: {From+":"+FromPort}, to: {To+":"+ToPort}, Slots: {string.Join(", ", Slots)}, action: {Action}, guid: {Guid}";
        }
    }

    // LRM -> CC
    [Serializable]
    public class SNPLinkConnectionRequest_rsp : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public List<int> Slots { get; set; }
        public RequestResult Result { get; set; }
        public Guid Guid { get; set; }
        public int FromPort { get; set; }
        public int ToPort { get; set; }

        public bool RequestFromDomain { get; set; }
        public bool Rerouting { get; set; } = false;
        public bool Releasing { get; set; } = false;

        public override string ToString()
        {
            return
                $"SNPLinkConnectionRequest_rsp from: {From + ":" + FromPort}, to: {To + ":" + ToPort}, Slots: {string.Join(", ", Slots)}, result: {Result}, guid: {Guid}";
        }
    }
}
