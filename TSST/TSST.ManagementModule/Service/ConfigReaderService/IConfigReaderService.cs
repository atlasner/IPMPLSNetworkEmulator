using TSST.ManagementModule.Model;

namespace TSST.ManagementModule.Service.ConfigReaderService
{
    public interface IConfigReaderService
    {
        ManagementModuleConfigDto ReadFromFile();
    }
}
