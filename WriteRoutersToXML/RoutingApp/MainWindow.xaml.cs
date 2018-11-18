using RoutingApp.Controls;
using RoutingApp.Core.Models.NetComponents;
using RoutingApp.Core.Services;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RoutingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string defaultFilePath = ConfigurationManager.AppSettings["dataDefaultFilePath"];

        public List<Router> routers;

        public MainWindow()
        {
            InitializeComponent();
            LoggerService.Instance.OutPutTextBox = ConsoleOutput;

                       
            //AddRoutersToCanvas();
        }

        public void DebugRun()
        {
            if (!File.Exists(defaultFilePath))
            {

                var routers = new Router[5];

                for (var i = 0; i < 5; i++)
                {
                    routers[i] = new Router("router" + i, 5);
                }

                routers[0].ConnectTo(routers[2]);
                routers[0].ConnectTo(routers[3]);
                routers[1].ConnectTo(routers[2]);
                routers[1].ConnectTo(routers[4]);
                routers[2].ConnectTo(routers[4]);
                routers[3].ConnectTo(routers[4]);

                RouterSerializeService.SerializeRouters(routers);

                Controller.Instance.InitializeController(routers.ToList());
            }
            else
            {
                var routers = RouterSerializeService.DeserializeRouters();

                this.routers = routers.ToList();

                Controller.Instance.InitializeController(routers.ToList());

                Controller.Instance.InitTraffic(0, 4, 10, 700, 70, "Traffic1");
                Controller.Instance.InitTraffic(2, 3, 6, 300, 30, "Traffic2");

                Controller.Instance.InitSimulation();
                //Controller.Instance.StartSimulation();

                var a = 5;
            }
        }

        public void AddRoutersToCanvas()
        {
            WorkingArea.Children.Clear();
            foreach (Router router in routers)
            {
                WorkingRouter routerControl = new WorkingRouter(router);
                WorkingArea.Children.Add(routerControl);


                if (router != null)
                {
                    Canvas.SetLeft(routerControl, router.PositionX);
                    Canvas.SetTop(routerControl, router.PositionY);
                    //var transform = routerControl.RenderTransform as TranslateTransform;
                    //transform.X = router.PositionX;
                    //transform.Y = router.PositionY;
                }
            }
        }

        private void btnAddRouter_Click(object sender, RoutedEventArgs e)
        {
            var a = 5;
        }

        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            RouterSerializeService.SerializeRouters(routers.ToArray());
        }

        private void btnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(defaultFilePath)) return;
            routers = RouterSerializeService.DeserializeRouters().ToList();
            AddRoutersToCanvas();
        }
    }
}
