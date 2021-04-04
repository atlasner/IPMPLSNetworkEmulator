using System;
using System.Collections.Generic;
using System.Linq;
using TSST.NCC.Model;
using TSST.Shared.Model.Messages;

namespace TSST.Subnetwork.Service.DataHolder
{
    public static class DataHolder
    {
        public static List<DataModel> Data { get; set; } = new List<DataModel>();

        public static bool CheckIfRequestsMatchResponses(Guid guid)
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

            foreach (var req in dataModel.CallCoordinationReq)
            {
                if (dataModel.CallCoordinationRsp.Find(r => r.Guid == guid) != null)
                {
                    counter++;
                }
            }

            return dataModel.CallCoordinationReq.Count == counter;
        }

        //public static void AddMessage(ISignalingMessage message, string from, string to)
        //{
        //    var dataModel = GetDataModel(from, to);

        //    if (dataModel == null)
        //    {
        //        dataModel = new DataModel
        //        {
        //            From = from,
        //            To = to,
        //            CallCoordinationReq = new List<CallCoordination_req>(),
        //            CallCoordinationRsp = new List<CallCoordination_rsp>()
        //        };

        //        Data.Add(dataModel);
        //    }

        //    switch (message)
        //    {
        //        case SNPLinkConnectionRequest_rsp rsp:
        //            Data.Single(x => x.From == from && x.To == to).SnpLinkConnectionRequestRsp.Add(rsp);
        //            break;
        //        case SNPLinkConnectionRequest_req req:
        //            Data.Single(x => x.From == from && x.To == to).SnpLinkConnectionRequestReq.Add(req);
        //            break;
        //    }
        //}

        public static DataModel GetDataModel(string from, string to)
        {
            return Data.SingleOrDefault(x => x.From == from && x.To == to);
        }

        public static DataModel GetDataModel(Guid guid)
        {
            return (from dm in Data from req in dm.ConnectionRequestReq where req.Guid == guid select dm).FirstOrDefault();
        }

        public static List<DataModel> GetDataModel(string client)
        {
            var list = new List<DataModel>();

            list = Data.Where(d => d.From == client || d.To == client).ToList();

            return list;
        }
    }

}
