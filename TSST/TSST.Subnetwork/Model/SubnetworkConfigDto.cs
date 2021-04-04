using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TSST.Subnetwork.Model
{
    public class SubnetworkConfigDto
    {
        public string Name { get; set; }
        public int ServerPort { get; set; }
        public int ClientPort { get; set; }
        public int NCCPort { get; set; }
        public int CableCloudPort { get; set; }
        public bool IsDomain { get; set; }
        public List<Client> Clients { get; set; } = new List<Client>();
        public List<Edge> Edges { get; set; }
        public List<Node> Nodes { get; set; }
    }
}
