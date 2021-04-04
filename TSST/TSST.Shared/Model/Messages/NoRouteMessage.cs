using System;

namespace TSST.Shared.Model.Messages
{
    [Serializable]
    public class NoRouteMessage : ISignalingMessage
    {
        public Guid Guid { get; set; }
    }
}
