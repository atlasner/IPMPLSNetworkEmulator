using System.Collections.Generic;
using System.Net;

namespace TSST.CableCloud.Model
{
    public class CableCloudConfigDto
    {
        public IPAddress Ip { get; set; }
        public int Port { get; set; }
        public IEnumerable<ForwardingInfoDto> ForwardingTable { get; set; }
    }

}