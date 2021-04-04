using TSST.NetworkNode.Model;

namespace TSST.NetworkNode.Service.ConfigReaderService
{
    public interface IConfigReaderService
    {
        NetworkNodeConfigDto ReadHostConfig();
    }
}
