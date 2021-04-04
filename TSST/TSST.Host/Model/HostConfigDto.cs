using System.Collections.Generic;

namespace TSST.Host.Model
{
    public class HostConfigDto
    {
        public string Name { get; set; }
        public string CloudIp { get; set; }
        public int CloudPort { get; set; }
        public string CpccName { get; set; }
        public int CpccPort { get; set; }
        public IDictionary<string, string> NameToIp { get; set; }

        public int OutputPort { get; set; }
    }
}