using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using TSST.ManagementModule.Service.ManagementService;
using TSST.Shared.Model.Rows;
using TSST.Shared.Service.LogService;

namespace TSST.ManagementModule.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILogService _logService;
        private readonly IManagementService _managementService;

        public string WindowTitle => "Management Module";

        public IRow SelectedRow { get; set; }

        private IRow _deletedRow;


        public EonRow NewEonRow { get; set; } = new EonRow();

        public int SelectedTable { get; set; }
        public ObservableCollection<string> Logs => _logService.Logs;
        public ObservableCollection<EonRow> EonRows => _managementService.ManagementConfig.EonRows;



        private static readonly object Lock = new object();


        private RelayCommand<IRow> _deleteRowCommand;

        public RelayCommand<IRow> DeleteRowCommand 
        {
            get {
                return _deleteRowCommand ?? (_deleteRowCommand = new RelayCommand<IRow>(_ =>
                {
                    _deletedRow = SelectedRow;
                    _managementService.DeleteTableRow(ref _deletedRow);
                }));
            }
        }

        private RelayCommand<int> _addRowCommand;

        public RelayCommand<int> AddRowCommand
        {
            get
            {
                return _addRowCommand ?? (_addRowCommand = new RelayCommand<int>(_ =>
                           {
                               _managementService.AddTableRow(CreateRow(SelectedTable));
                           }));
            }
        }

        public MainViewModel(ILogService logService, IManagementService managementService)
        {
            _logService = logService;
            _managementService = managementService;

            Initialize();
        }

        private void Initialize()
        {
            BindingOperations.EnableCollectionSynchronization(Logs, Lock);
            BindingOperations.EnableCollectionSynchronization(EonRows, Lock);

            Task.Run(() =>  _managementService.StartListeningAsync());
        }


        private IRow CreateRow(int selectedTable)
        {
            switch (selectedTable)
            {
                case 0:
                    var eonRow = new EonRow
                    {
                        Node = NewEonRow.Node,
                        IncomingPort = NewEonRow.IncomingPort,
                        FirstSlotIndex = NewEonRow.FirstSlotIndex,
                        LastSlotIndex = NewEonRow.LastSlotIndex,
                        OutPort = NewEonRow.OutPort
                    };
                    return eonRow;

                default:
                    return null;
            }

        }
    }
}