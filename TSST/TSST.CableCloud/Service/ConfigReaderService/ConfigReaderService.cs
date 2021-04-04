using System.Collections.Generic;
using System.IO;
using System.Net;
using TSST.CableCloud.Model;

namespace TSST.CableCloud.Service.ConfigReaderService
{
    public class ConfigReaderService : IConfigReaderService
    {
        private readonly string _filePath;

        public ConfigReaderService(string filepath)
        {
            _filePath = filepath;
        }

        public CableCloudConfigDto ReadFromFile()
        {
            var config = new CableCloudConfigDto();
            var forwardingTable = new List<ForwardingInfoDto>();

            var lines = File.ReadAllLines(_filePath);

            foreach (var line in lines)
            {
                var parts = line.Split(' ');

                if (line.StartsWith("IPADDRESS"))
                {
                    config.Ip = IPAddress.Parse(parts[1]);
                }
                else if (line.StartsWith("PORT"))
                {
                    config.Port = int.Parse(parts[1]);
                }
                else if (line.StartsWith("CABLE"))
                {
                    forwardingTable.Add(new ForwardingInfoDto(forwardingTable.Count, parts[1], int.Parse(parts[2]), parts[3], int.Parse(parts[4]), parts[5].Equals("1")));
                }
            }

            config.ForwardingTable = forwardingTable;

            return config;
        }
    }
}