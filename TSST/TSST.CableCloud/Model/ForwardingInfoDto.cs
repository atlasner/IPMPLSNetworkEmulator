using System.ComponentModel;

namespace TSST.CableCloud.Model
{
    public sealed class ForwardingInfoDto : INotifyPropertyChanged
    {
        public int Id { get; }
        public string Node1 { get; set; }
        public string Node2 { get; set; }
        public int Port1 { get; set; }
        public int Port2 { get; set; }

        private bool _status;
        public bool Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public ForwardingInfoDto(int id, string nodeName1, int port1, string nodeName2, int port2, bool status)
        {
            Id = id;
            Node1 = nodeName1;
            Node2 = nodeName2;
            Port1 = port1;
            Port2 = port2;
            Status = status;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}