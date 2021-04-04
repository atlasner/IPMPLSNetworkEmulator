using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    [Serializable]
    public class CallTeardown_req : ISignalingMessage
    {
        public string From { get; set; }
        public string To { get; set; }

        public override string ToString()
        {
            return $"CallTeardown_req from: {From}, to: {To}, guid: {Guid}";
        }

        public Guid Guid { get; set; }
    }
}
