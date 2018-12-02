using RoutingApp.Controls;
using RoutingApp.Core;
using RoutingApp.Core.Models.NetComponents;
using RoutingApp.Helpers;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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
            Link.MetricChanged += OnMetricChanged;
        }

        private void OnMetricChanged(int availableBandwidth)
        {
            //update in main thread
             
            Line.Dispatcher.Invoke(new Action(() =>
            {
                var filledPercentage = 1 - (double)availableBandwidth / (Constants.LINK_MAX_BANDWIDH);

                var hueValue = HSLColorHelper.GreenHue + (HSLColorHelper.RedHue - HSLColorHelper.GreenHue) * filledPercentage;

                var color = new SolidColorBrush(HSLColorHelper.RGBFromHSL(hueValue, 100, 50));
                Line.Stroke = color;
            }), DispatcherPriority.ContextIdle);
        }        
    }
}
