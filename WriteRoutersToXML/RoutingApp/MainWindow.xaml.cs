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
using System.Windows.Shapes;

namespace RoutingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string defaultFilePath = ConfigurationManager.AppSettings["dataDefaultFilePath"];

        public List<Router> routers;
        public List<Link> links;

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

                Controller.Instance.InitializeController(routers.ToList());

                this.routers = routers.ToList();
                this.links = Controller.Instance.GetAllLinks();


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

        public void AddLinksToCanvas()
        {

            //init links
            foreach (Link link in links)
            {
                var line = CreateLineForLink(link);
                WorkingArea.Children.Add(line);
            }
        }

        private void btnAddRouter_Click(object sender, RoutedEventArgs e)
        {
            var router = Controller.Instance.AddNewRouter();
            WorkingRouter routerControl = new WorkingRouter(router);
            WorkingArea.Children.Add(routerControl);
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
            Controller.Instance.InitializeController(routers);
            links = Controller.Instance.GetAllLinks();

            AddRoutersToCanvas();
            AddLinksToCanvas();
        }

        private Line CreateLineForLink(Link link)
        {
            Router router1 = link.Interface1.Router;
            Router router2 = link.Interface2.Router;

            Line line = new Line();
            line.X1 = router1.PositionX + 32.5;
            line.Y1 = router1.PositionY + 32.5;
            line.X2 = router2.PositionX + 32.5;
            line.Y2 = router2.PositionY + 32.5;
            line.Stroke = new SolidColorBrush(Colors.Black);

            return line;
        }
    }
}
