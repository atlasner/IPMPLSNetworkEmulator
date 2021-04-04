using TSST.CableCloud.Model;

namespace TSST.CableCloud.Service.ConfigReaderService
{
    public interface IConfigReaderService
    {
        CableCloudConfigDto ReadFromFile();
    }
}