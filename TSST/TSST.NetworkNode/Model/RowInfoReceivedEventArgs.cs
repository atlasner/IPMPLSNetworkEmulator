using System;
using TSST.Shared.Model.Rows;

namespace TSST.NetworkNode.Model
{
    public class RowInfoReceivedEventArgs : EventArgs
    {
        public RowInfo RowInfo { get; set; }
    }
}
