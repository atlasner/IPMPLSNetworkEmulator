using System.Collections.ObjectModel;
using TSST.Shared.Model.Rows;

namespace TSST.ManagementModule.Model
{
    public class ManagementModuleConfigDto
    {
        public string Ip { get; set; }
        public int Port { get; set; }

        public ObservableCollection<EonRow> EonRows { get; set; }
    }
}
