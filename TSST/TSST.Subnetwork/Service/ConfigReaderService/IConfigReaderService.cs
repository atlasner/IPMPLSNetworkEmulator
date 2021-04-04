using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSST.Subnetwork.Model;

namespace TSST.Subnetwork.Service.ConfigReaderService
{
    public interface IConfigReaderService
    {
        SubnetworkConfigDto ReadSubnetworkConfig();
    }
}
