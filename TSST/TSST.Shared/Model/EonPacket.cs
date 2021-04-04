using System;
using System.Collections.Generic;

namespace TSST.Shared.Model
{
    [Serializable]
    public class EonPacket
    {
        public int Ttl { get; set; }
        public List<int> OccupiedSlots { get; set; }
        public int OccupiedCapacity { get; set; }

        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public int Port { get; set; }

        public string Content { get; set; }
    }
}
