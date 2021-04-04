using TSST.NCC.Model;

namespace TSST.NCC.Service.ConfigReaderService
{
    public interface IConfigReaderService
    {
        NccConfigDto ReadNccConfig();
    }
}
