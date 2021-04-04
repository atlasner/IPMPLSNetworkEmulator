using System.Collections.Generic;

namespace TSST.NetworkNode.Model
{
    public class NetworkNodeConfigDto
    {
        public string Name { get; set; }

        public string CloudIp { get; set; }
        public int CloudPort { get; set; }

        public string ManagementIp { get; set; }
        public int ManagementPort { get; set; }

        public int CCRCPort { get; set; }
        public List<Neighbor> Neighbors { get; set; }
        public int DomainCCRCPort { get; set; }
    }
}
