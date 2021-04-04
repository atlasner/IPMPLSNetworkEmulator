using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using TSST.Shared.Model.Messages;

namespace TSST.Shared.Model
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public ISignalingMessage Message { get; set; }
    }
}
