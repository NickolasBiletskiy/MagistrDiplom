using RoutingApp.Controls;
using RoutingApp.Core.Models.NetComponents;
using System.Windows.Shapes;

namespace RoutingApp.ViewModels
{
    public class ConnectionViewModel
    {
        public WorkingRouter RouterFrom { get; set; }
        public Line Line { get; set; }
        public Link Link { get; set; }
        public WorkingRouter RouterTo { get; set; }

        public ConnectionViewModel(WorkingRouter routerFrom, WorkingRouter routerTo, Line line, Link link)
        {
            RouterFrom = routerFrom;
            RouterTo = routerTo;
            Line = line;
            Link = link;
        }
    }
}
