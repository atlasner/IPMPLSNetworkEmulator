using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Shared.Model.Messages
{
    public interface ISignalingMessage
    {
        Guid Guid { get; set; }
    }
}
