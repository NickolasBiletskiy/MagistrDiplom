using System;
using RoutingApp.Core.Models.NetComponents;

namespace RoutingApp.Core.Models.Routing
{
    public class RoutingTableItem
    {
        public Router DestinationRouter { get; set; }
        public Router ClosestRouter { get; set; }
        public DateTime LastPathUpdateTS { get; set; }
        public bool IsExpired { get
            {
                return (LastPathUpdateTS - DateTime.UtcNow).TotalSeconds > Constants.PATH_UPDATE_TIME;
            } }
    }
}
