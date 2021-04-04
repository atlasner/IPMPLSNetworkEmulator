using System.Collections.Generic;
using System.IO;
using TSST.NetworkNode.Model;

namespace TSST.NetworkNode.Service.ConfigReaderService
{
    class ConfigReaderService : IConfigReaderService
    {
        private readonly string _filePath;

        public ConfigReaderService(string filePath)
        {
            _filePath = filePath;
        }

        public NetworkNodeConfigDto ReadHostConfig()
        {
            var config = new NetworkNodeConfigDto();

            var lines = File.ReadAllLines(_filePath);

            config.Neighbors = new List<Neighbor>();

            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                if (line.StartsWith("NAME"))
                {
                    config.Name = parts[1];
                }
                else if (line.StartsWith("CLOUDIP"))
                {
                    config.CloudIp = parts[1];
                }
                else if (line.StartsWith("CLOUDPORT"))
                {
                    config.CloudPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("MANAGEMENTIP"))
                {
                    config.ManagementIp = parts[1];
                }
                else if (line.StartsWith("MANAGEMENTPORT"))
                {
                    config.ManagementPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("CCRCPORT")) 
                {
                    config.CCRCPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("DOMAINCCRCPORT"))
                {
                    config.DomainCCRCPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("NEIGHBOR"))
                {
                    config.Neighbors.Add(new Neighbor
                    {
                        Name = parts[1],
                        Port = int.Parse(parts[2])
                    });
                }
            }

            return config;
        }
    }
}
