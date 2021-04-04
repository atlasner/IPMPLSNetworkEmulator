using System.Collections.Generic;

namespace TSST.NCC.Model
{
    public class NccConfigDto
    {
        public string Name { get; set; }
        public bool DomainSlave { get; set; }
        public int ServerPort { get; set; }
        public int ClientPort { get; set; }

        public List<DirectoryEntry> Directory { get; set; }
    }
}
