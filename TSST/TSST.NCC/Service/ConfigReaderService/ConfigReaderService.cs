using System.Collections.Generic;
using System.IO;
using TSST.NCC.Model;

namespace TSST.NCC.Service.ConfigReaderService
{
    public class ConfigReaderService : IConfigReaderService
    {
        private readonly string _filePath;
        public ConfigReaderService(string filepath)
        {
            _filePath = filepath;
        }

        public NccConfigDto ReadNccConfig()
        {
            var config = new NccConfigDto();

            var lines = File.ReadAllLines(_filePath);

            config.Directory = new List<DirectoryEntry>();

            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                if (line.StartsWith("NAME"))
                {
                    config.Name = parts[1];
                }
                else if (line.StartsWith("DOMAINSLAVE"))
                {
                    config.DomainSlave = parts[1].Equals("1");
                }
                else if (line.StartsWith("SERVERPORT"))
                {
                    config.ServerPort = int.Parse(parts[1]);
                }
                else if(line.StartsWith("CLIENTPORT"))
                {
                    config.ClientPort = int.Parse(parts[1]);
                }
                else if (line.StartsWith("DIRECTORY"))
                {
                    config.Directory.Add(new DirectoryEntry
                    {
                        Name = parts[1],
                        ToNode = parts[2],
                        EstimatedDistance = int.Parse(parts[3]),
                        LocalClient = parts[4] == "1"
                    });
                }
            }

            return config;
        }
    }
}
