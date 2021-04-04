using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSST.Subnetwork.Model;

namespace TSST.Subnetwork.Service.ConfigReaderService
{
    public class ConfigReaderService : IConfigReaderService
    {
        private readonly string _filePath;

        public ConfigReaderService(string filePath)
        {
            _filePath = filePath;
        }
        public SubnetworkConfigDto ReadSubnetworkConfig()
        {
            var config = new SubnetworkConfigDto();

            var lines = File.ReadAllLines(_filePath);

            config.Edges = new List<Edge>();
            config.Nodes = new List<Node>();

            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                if (line.StartsWith("NAME"))
                {
                    config.Name = parts[1];
                }
                else if (line.StartsWith("ISDOMAIN"))
                {
                    config.IsDomain = parts[1].Equals("1");
                }
                else if (line.StartsWith("NCCPORT"))
                {
                    config.NCCPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("SERVERPORT"))
                {
                    config.ServerPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("CLIENTPORT"))
                {
                    config.ClientPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("CABLECLOUDPORT"))
                {
                    config.CableCloudPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("CLIENT"))
                {
                    config.Clients.Add(new Client
                    {
                        Name = parts[1],
                        Port = int.Parse(parts[2])
                    });
                }
                else if (line.StartsWith("EDGE"))
                {
                    config.Edges.Add(new Edge
                    {
                        Id = int.Parse(parts[1]),
                        Node1 = new Node { Id = config.Nodes.FirstOrDefault(n => n.Name == parts[2]).Id, Name = parts[2] },
                        Node2 = new Node { Id = config.Nodes.FirstOrDefault(n => n.Name == parts[3]).Id, Name = parts[3] },
                        Length = int.Parse(parts[4]),
                        State = parts[5] == "1"
                    });
                }
                else if (line.StartsWith("NODE"))
                {
                    config.Nodes.Add(new Node
                    {
                        Id = uint.Parse(parts[1]),
                        Name = parts[2]
                    });
                }
            }

            return config;
        }
    }
}
