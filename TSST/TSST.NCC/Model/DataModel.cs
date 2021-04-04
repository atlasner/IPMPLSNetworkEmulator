using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSST.Shared.Model.Messages;

namespace TSST.NCC.Model
{
    public class DataModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public List<int> Slots { get; set; } = new List<int>();
        public int Capacity { get; set; }

        public List<CallRequest_req> CallRequestReq = new List<CallRequest_req>();
        public List<CallRequest_rsp> CallRequestRsp = new List<CallRequest_rsp>();
        public List<ConnectionRequest_req> ConnectionRequestReq = new List<ConnectionRequest_req>();
        public List<ConnectionRequest_rsp> ConnectionRequestRsp = new List<ConnectionRequest_rsp>();
        public List<CallCoordination_req> CallCoordinationReq = new List<CallCoordination_req>();
        public List<CallCoordination_rsp> CallCoordinationRsp = new List<CallCoordination_rsp>();
        public List<CallAccept_req> CallAcceptReq = new List<CallAccept_req>();
        public List<CallAccept_rsp> CallAcceptRsp = new List<CallAccept_rsp>();
    }
}
