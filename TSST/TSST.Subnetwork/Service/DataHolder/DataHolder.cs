using System;
using System.Collections.Generic;
using System.Linq;
using TSST.Shared.Model.Messages;
using TSST.Subnetwork.Model;

namespace TSST.Subnetwork.Service.DataHolder
{
    public static class DataHolder
    {
        public static List<DataModel> Data { get; set; } = new List<DataModel>();

        public static bool CheckIfSNPLinkConnectionMatchResponses(Guid guid)
        {
            //var dataModel = Data.SingleOrDefault(dm =>
            //    dm.SnpLinkConnectionRequestReq.SingleOrDefault(f => f.From == from && f.To == to) != null);

            var dataModel = GetDataModel(guid);

            if (dataModel == null)
            {
                return false;
            }

            //var counter = (from req in dataModel.SnpLinkConnectionRequestReq where req.From == @from && req.To == to from rsp in dataModel.SnpLinkConnectionRequestRsp where req.Guid == rsp.Guid select req).Count();

            var counter = 0;

            foreach (var req in dataModel.SnpLinkConnectionRequestReq)
            {
                if (dataModel.SnpLinkConnectionRequestRsp.Find(r => r.Guid == guid) != null)
                {
                    counter++;
                }
            }

            return dataModel.SnpLinkConnectionRequestReq.Count == counter;
        }

        public static bool CheckIfConnectionRequestMatchResponses(Guid guid)
        {
            //var dataModel = Data.SingleOrDefault(dm =>
            //    dm.SnpLinkConnectionRequestReq.SingleOrDefault(f => f.From == from && f.To == to) != null);

            var dataModel = GetDataModel(guid);

            if (dataModel == null)
            {
                return false;
            }

            //var counter = (from req in dataModel.SnpLinkConnectionRequestReq where req.From == @from && req.To == to from rsp in dataModel.SnpLinkConnectionRequestRsp where req.Guid == rsp.Guid select req).Count();

            var counter = 0;

            foreach (var req in dataModel.ConnectionRequestReq)
            {
                if (dataModel.ConnectionRequestRsp.Find(r => r.Guid == guid) != null)
                {
                    counter++;
                }
            }

            return dataModel.SnpLinkConnectionRequestReq.Count == counter;
        }

        public static void AddMessage(ISignalingMessage message, string from, string to)
        {
            var dataModel = GetDataModel(from, to);

            if (dataModel == null)
            {
                dataModel = new DataModel
                {
                    From = from,
                    To = to,
                    SnpLinkConnectionRequestReq = new List<SNPLinkConnectionRequest_req>(),
                    SnpLinkConnectionRequestRsp = new List<SNPLinkConnectionRequest_rsp>()
                };

                Data.Add(dataModel);
            }

            switch (message)
            {
                case SNPLinkConnectionRequest_rsp rsp:
                    Data.Single(x => x.From == from && x.To == to).SnpLinkConnectionRequestRsp.Add(rsp);
                    break;
                case SNPLinkConnectionRequest_req req:
                    Data.Single(x => x.From == from && x.To == to).SnpLinkConnectionRequestReq.Add(req);
                    break;
            }
        }

        public static DataModel GetDataModel(string from, string to)
        {
            return Data.SingleOrDefault(x => x.From == from && x.To == to);
        }

        public static DataModel GetDataModel(Guid guid)
        {
            var datamodel =
                (from dm in Data from req in dm.SnpLinkConnectionRequestReq where req.Guid == guid select dm)
                .FirstOrDefault();

            if (datamodel == null)
            {
                return (from dm in Data from req in dm.ConnectionRequestReq where req.Guid == guid select dm)
                    .FirstOrDefault();
            }

            return datamodel;
        }

        public static List<DataModel> GetDataModel(Edge edge)
        {
            return Data.FindAll(d => d.RouteTableQuery.Edges.Contains(edge));
        }
    }

}
