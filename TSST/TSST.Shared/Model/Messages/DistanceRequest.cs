using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    [Serializable]
    public class DistanceRequest_req : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public Guid Guid { get; set; }
    }
    [Serializable]
    public class DistanceRequest_rsp : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Distance { get; set; }
        public Guid Guid { get; set; }
    }
}
