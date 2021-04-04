using System;

namespace TSST.Shared.Model.Rows
{
    [Serializable]
    public class RowInfo
    {
        public ManagementAction Action { get; set; }
        public IRow Row { get; set; }
    }
}
