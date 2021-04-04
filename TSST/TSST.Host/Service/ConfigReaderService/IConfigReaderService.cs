using TSST.Host.Model;

namespace TSST.Host.Service.ConfigReaderService
{
    public interface IConfigReaderService
    {
        HostConfigDto ReadHostConfig();
    }
}