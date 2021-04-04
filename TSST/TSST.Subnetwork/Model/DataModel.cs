using System.Collections.Generic;
using TSST.Shared.Model.Messages;
using TSST.Shared.Model.Rows;

namespace TSST.Subnetwork.Model
{
    public class DataModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public List<ConnectionRequest_req> ConnectionRequestReq { get; set; } = new List<ConnectionRequest_req>();
        public List<ConnectionRequest_rsp> ConnectionRequestRsp { get; set; } = new List<ConnectionRequest_rsp>();
        public List<SNPLinkConnectionRequest_req> SnpLinkConnectionRequestReq { get; set; } = new List<SNPLinkConnectionRequest_req>();
        public List<SNPLinkConnectionRequest_rsp> SnpLinkConnectionRequestRsp { get; set; } = new List<SNPLinkConnectionRequest_rsp>();
        public List<RowInfo> SentRowInfos { get; set; } = new List<RowInfo>();
        public List<int> Slots { get; set; } = new List<int>();
        public int Capacity { get; set; }
        public RouteTableQuery RouteTableQuery { get; set; }
        public int RequestSentCount { get; set; }
    }
}