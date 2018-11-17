using RoutingApp.Core.Models.NetComponents;
using RoutingApp.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using RoutingApp.Core;

namespace RoutingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string defaultFilePath = ConfigurationManager.AppSettings["dataDefaultFilePath"];

        public MainWindow()
        {
            InitializeComponent();
            LoggerService.Instance.OutPutTextBox = ConsoleOutput;


            DebugRun();
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

                Controller.Instance.InitTraffic(0, 4, 10, 700, 70, "Traffic1");
                Controller.Instance.InitTraffic(2, 3, 6, 300, 30, "Traffic1");

                Controller.Instance.InitSimulation();
                Task.Run(() => Controller.Instance.StartSimulation());
                
                var a = 5;
            }
        }
    }
}
