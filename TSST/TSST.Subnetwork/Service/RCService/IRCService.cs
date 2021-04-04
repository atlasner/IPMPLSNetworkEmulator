using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSST.Subnetwork.Model;

namespace TSST.Subnetwork.Service.RCService
{
    public interface IRCService
    {
        RouteTableQuery GetRouteTableQuery(string fromNode, string toNode, int distance, int capacity);
        RouteTableQuery GetRouteTableQuery(string fromNode, string toNode, List<int> slots);
        Edge SwitchEdgeStatus(string fromNode, string toNode, bool status);
    }
}
