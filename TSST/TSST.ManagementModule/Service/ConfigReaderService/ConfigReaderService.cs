using System.Collections.ObjectModel;
using System.IO;
using TSST.ManagementModule.Model;
using TSST.Shared.Model.Rows;

namespace TSST.ManagementModule.Service.ConfigReaderService
{
    public class ConfigReaderService : IConfigReaderService
    {
        private readonly string _filePath;

        public ConfigReaderService(string filePath)
        {
            _filePath = filePath;
        }

        public ManagementModuleConfigDto ReadFromFile()
        {
            var config = new ManagementModuleConfigDto();

            var lines = File.ReadAllLines(_filePath);

            var eonRows = new ObservableCollection<EonRow>();


            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                if (line.StartsWith("MANAGEMENTIP"))
                {
                    config.Ip = parts[1];
                }
                else if (line.StartsWith("MANAGEMENTPORT"))
                {
                    config.Port = int.Parse(parts[1]);
                }
                else if (line.StartsWith("EONROW"))
                {
                    var row = new EonRow {Node = parts[1], IncomingPort = int.Parse(parts[2]), FirstSlotIndex = int.Parse(parts[3]), LastSlotIndex = int.Parse(parts[4]), OutPort = int.Parse(parts[5])};
                    eonRows.Add(row);
                }
            }

            config.EonRows = eonRows;

            return config;
        }
    }
}
