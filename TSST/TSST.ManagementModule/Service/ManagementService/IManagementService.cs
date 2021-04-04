using TSST.ManagementModule.Model;
using TSST.Shared.Model.Rows;

namespace TSST.ManagementModule.Service.ManagementService
{
    public interface IManagementService
    {
        ManagementModuleConfigDto ManagementConfig { get; }
        void StartListeningAsync();
        void DeleteTableRow(ref IRow row);
        void AddTableRow(IRow row);
    }
}
