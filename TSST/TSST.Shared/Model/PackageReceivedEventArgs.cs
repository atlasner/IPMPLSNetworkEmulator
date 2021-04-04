using System;

namespace TSST.Shared.Model
{
    public class PackageReceivedEventArgs : EventArgs
    {
        public EonPacket Packet { get; set; }
    }
}
