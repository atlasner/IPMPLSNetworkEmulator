using System.Collections.Generic;
using System.IO;
using TSST.Host.Model;

namespace TSST.Host.Service.ConfigReaderService
{
    public class ConfigReaderService : IConfigReaderService
    {
        private readonly string _filePath;

        public ConfigReaderService(string filePath)
        {
            _filePath = filePath;
        }

        public HostConfigDto ReadHostConfig()
        {
            var config = new HostConfigDto();

            var lines = File.ReadAllLines(_filePath);

            var nameToIp = new Dictionary<string, string>();

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
                else if(line.StartsWith("CPCCNAME"))
                {
                    config.CpccName = parts[1];
                }
                else if(line.StartsWith("CPCCPORT"))
                {
                    config.CpccPort = int.Parse(parts[1]);
                }
                else if(line.StartsWith("OUTPUTPORT")) 
                {
                    config.OutputPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("NODE"))
                {
                    nameToIp.Add(parts[1], parts[2]);
                }
            }

            config.NameToIp = nameToIp;

            return config;
        }
    }
}