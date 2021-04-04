using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST.NCC.Model
{
    public class DirectoryEntry
    {
        public string Name { get; set; }
        public string ToNode { get; set; }
        public int EstimatedDistance { get; set; }
        public bool LocalClient { get; set; }
    }
}
