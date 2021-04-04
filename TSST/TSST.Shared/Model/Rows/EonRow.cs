using System;

namespace TSST.Shared.Model.Rows
{
    [Serializable]
    public class EonRow : IRow
    {
        public string Node { get; set; }
        public int IncomingPort { get; set; }
        public int OutPort { get; set; }

        public int FirstSlotIndex { get; set; }
        public int LastSlotIndex { get; set; }

        public override string ToString()
        {
            return
                $"EON Row: Node: {Node}, IncomingPort: {IncomingPort}, FirstSlotIndex: {FirstSlotIndex}, LastSlotIndex: {LastSlotIndex}, OutPort: {OutPort}";
        }
    }
}
