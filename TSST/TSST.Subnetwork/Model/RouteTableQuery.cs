using System.Collections.Generic;

namespace TSST.Subnetwork.Model
{
    public class RouteTableQuery
    {
        public List<string> Nodes { get; set; }
        public List<int> Slots { get; set; }
        public List<Edge> Edges { get; set; }
    }
}
