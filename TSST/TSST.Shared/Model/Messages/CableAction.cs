using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    [Serializable]
    public class CableAction : ISignalingMessage
    {
        public Guid Guid { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool Status { get; set; }
    }
}
