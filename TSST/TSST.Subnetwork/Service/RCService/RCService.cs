using System;
using System.Collections.Generic;
using System.Linq;
using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;
using TSST.Shared.Service.LogService;
using TSST.Subnetwork.Model;
using TSST.Subnetwork.Service.ConfigReaderService;
using Edge = TSST.Subnetwork.Model.Edge;

namespace TSST.Subnetwork.Service.RCService
{
    public class RCService : IRCService
    {
        private readonly ILogService _logService;
        private SubnetworkConfigDto _subnetworkConfigDto;
        private const int GuardBandwidth = 5;
        private const int MaxPossibleSlots = 64;
        public RCService(IConfigReaderService configReaderService, ILogService logService)
        {
            _subnetworkConfigDto = configReaderService.ReadSubnetworkConfig();
            _logService = logService;
        }

        public RouteTableQuery GetRouteTableQuery(string fromNode, string toNode, int distance, int capacity)
        {
            var graph = new Graph<uint, string>();

            foreach (var node in _subnetworkConfigDto.Nodes)
            {
                graph.AddNode(node.Id);
            }

            foreach (var edge in _subnetworkConfigDto.Edges.Where(edge => edge.State))
            {
                graph.Connect(edge.Node1.Id, edge.Node2.Id, edge.Length, null);
                graph.Connect(edge.Node2.Id, edge.Node1.Id, edge.Length, null);
            }

            var occupiedSlots = new List<int>();
            var possibleSlots = Enumerable.Range(0, MaxPossibleSlots - 1).ToList();


            foreach (var dm in DataHolder.DataHolder.Data)
            {
                occupiedSlots.AddRange(dm.Slots);
            }

            var bandwidth = capacity * 2 / GetModulationValue(distance) + GuardBandwidth;

            var numberOfSlots = (int) Math.Ceiling(bandwidth / 12.5);

            possibleSlots.RemoveAll(slot => occupiedSlots.Contains(slot));

            var freeSlots = GetFreeSlots(numberOfSlots, possibleSlots);

            var result = graph.Dijkstra(_subnetworkConfigDto.Nodes.Find(n => n.Name == fromNode).Id,
                _subnetworkConfigDto.Nodes.Find(n => n.Name == toNode).Id);


            var pathNodesId = result.GetPath();

            var pathNodes = pathNodesId.Select(node => _subnetworkConfigDto.Nodes.Find(n => n.Id == node).Name).ToList();

            return new RouteTableQuery {Nodes = pathNodes, Slots = freeSlots, Edges = GetEdgesBetweenNodes(pathNodes) };
        }

        private List<int> GetFreeSlots(int numberOfSlots, ICollection<int> possibleSlots)
        {
            List<int> freeSlots = null;
            while(true)
            {
                freeSlots = new List<int>();
                var firstSlot = possibleSlots.First();
                for (var j = firstSlot; j < firstSlot + numberOfSlots; j++)
                {
                    freeSlots.Add(j);
                }

                if (possibleSlots.Intersect(freeSlots).Count() == numberOfSlots)
                {
                    break;
                }
                possibleSlots.Remove(firstSlot);

                if (possibleSlots.Count == 0)
                {
                    return null;
                }
            }

            return freeSlots;
        }

        private int GetModulationValue(int distance)
        {

            if (distance <= 100)
            {
                return 6;
            }

            if (distance <= 200)
            {
                return 5;
            }

            if (distance <= 300)
            {
                return 4;
            }

            if (distance <= 400)
            {
                return 3;
            }

            return distance <= 500 ? 2 : 1;
        }

        public RouteTableQuery GetRouteTableQuery(string fromNode, string toNode, List<int> slots)
        {
            var graph = new Graph<uint, string>();

            foreach (var node in _subnetworkConfigDto.Nodes)
            {
                graph.AddNode(node.Id);
            }

            foreach (var edge in _subnetworkConfigDto.Edges.Where(e => e.State))
            {
                graph.Connect(edge.Node1.Id, edge.Node2.Id, edge.Length, null);
                graph.Connect(edge.Node2.Id, edge.Node1.Id, edge.Length, null);
            }

            var result = graph.Dijkstra(_subnetworkConfigDto.Nodes.Find(n => n.Name == fromNode).Id,
                _subnetworkConfigDto.Nodes.Find(n => n.Name == toNode).Id);

            var pathNodesId = result.GetPath();

            var pathNodes = pathNodesId.Select(node => _subnetworkConfigDto.Nodes.Find(n => n.Id == node).Name).ToList();

            return !pathNodes.Any() ? null : new RouteTableQuery { Nodes = pathNodes, Slots = slots, Edges = GetEdgesBetweenNodes(pathNodes)};
        }

        public Edge SwitchEdgeStatus(string fromNode, string toNode, bool status)
        {
            var edge = _subnetworkConfigDto.Edges.SingleOrDefault(e => e.Node1.Name == fromNode && e.Node2.Name == toNode || e.Node2.Name == fromNode && e.Node1.Name == toNode);
            if(edge != null)
                edge.State = status;
            return edge;
        }

        public List<Edge> GetEdgesBetweenNodes(List<string> nodes)
        {
            var edges = new List<Edge>();

            for (var i = 0; i < nodes.Count - 1; i++)
            {
                var edge = _subnetworkConfigDto.Edges.SingleOrDefault(e =>
                    (e.Node1.Name == nodes[i] && e.Node2.Name == nodes[i + 1]) ||
                    (e.Node2.Name == nodes[i] && e.Node1.Name == nodes[i + 1]));

                if(edge != null)
                    edges.Add(edge);
            }

            return edges;
        }
    }
}
