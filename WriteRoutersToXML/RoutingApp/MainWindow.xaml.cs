using RoutingApp.Controls;
using RoutingApp.Core.Models.NetComponents;
using RoutingApp.Core.Services;
using RoutingApp.ViewModels;
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

    public enum EditorState
    {
        None,
        RouterDelete,
        RouterMove,
        LinkAdd,
        LinkDelete
    }

    public enum ProgramState
    {
        Simulation,
        Edition
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static double routerPositionCorrective = 32.5;
        static string defaultFilePath = ConfigurationManager.AppSettings["dataDefaultFilePath"];

        public List<Router> routers;
        public List<Link> links;
        public List<WorkingRouter> routerViewModels;
        public List<ConnectionViewModel> connections;

        private EditorState editorState = EditorState.RouterMove;
        private ProgramState programState = ProgramState.Edition;
        private List<Button> editorStateButtons = new List<Button>();

        public MainWindow()
        {
            RouterSerializeService.defaultFilePath = defaultFilePath;

            InitializeComponent();

            InitTopPanelButtonEventHandlers();

            LoggerService.Instance.OutPutTextBox = ConsoleOutput;
            connections = new List<ConnectionViewModel>();
            routerViewModels = new List<WorkingRouter>();
            //AddRoutersToCanvas();
        }

        #region Click Listeners

        private void InitTopPanelButtonEventHandlers()
        {
            editorStateButtons.Add(btnRemoveRouter);
            editorStateButtons.Add(btnMoveRouter);
            editorStateButtons.Add(btnAddLink);
            editorStateButtons.Add(btnRemoveLink);
        }

        private void btnRemoveRouter_Click(object sender, RoutedEventArgs e)
        {
            SwitchEditorState(sender as Button);
        }

        private void btnMoveRouter_Click(object sender, RoutedEventArgs e)
        {
            SwitchEditorState(sender as Button);
        }

        private void btnAddLink_Click(object sender, RoutedEventArgs e)
        {
            SwitchEditorState(sender as Button);
        }

        private void btnRemoveLink_Click(object sender, RoutedEventArgs e)
        {
            SwitchEditorState(sender as Button);
        }

        public void SwitchEditorState(Button clickedButton)
        {
            foreach(var button in editorStateButtons)
            {
                button.Style = (Style)FindResource("modeChangerButton");
            }
            clickedButton.Style = FindResource("modeChangerButtonActive") as Style;
        }

        #endregion

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


        #region File System
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
            CreateViewModels();
        }
        #endregion

        #region Routers
        private void btnAddRouter_Click(object sender, RoutedEventArgs e)
        {
            var router = Controller.Instance.AddNewRouter();
            WorkingRouter routerControl = new WorkingRouter(router);
            WorkingArea.Children.Add(routerControl);
            var a = 5;
        }
        #endregion

        public void AddRoutersToCanvas()
        {
            WorkingArea.Children.Clear();
            foreach (Router router in routers)
            {
                WorkingRouter routerControl = new WorkingRouter(router);
                routerControl.OnRouterMove += UpdateLinksOnRouterMove;
                WorkingArea.Children.Add(routerControl);


                if (router != null)
                {
                    Canvas.SetLeft(routerControl, router.PositionX);
                    Canvas.SetTop(routerControl, router.PositionY);
                }

                routerViewModels.Add(routerControl);
            }
        }

        public void CreateViewModels()
        {
            //init links
            foreach (Link link in links)
            {
                var line = CreateLineForLink(link);
                WorkingArea.Children.Add(line);

                var router1ViewModel = routerViewModels.FirstOrDefault(x => x.Router == link.Interface1.Router);
                var router2ViewModel = routerViewModels.FirstOrDefault(x => x.Router == link.Interface2.Router);
                var viewModel = new ConnectionViewModel(router1ViewModel, router2ViewModel, line, link);

                connections.Add(viewModel);
            }
        }

        private Line CreateLineForLink(Link link)
        {
            Router router1 = link.Interface1.Router;
            Router router2 = link.Interface2.Router;

            Line line = new Line();
            line.X1 = router1.PositionX + routerPositionCorrective;
            line.Y1 = router1.PositionY + routerPositionCorrective;
            line.X2 = router2.PositionX + routerPositionCorrective;
            line.Y2 = router2.PositionY + routerPositionCorrective;
            line.Stroke = new SolidColorBrush(Colors.Black);

            return line;
        }

        public void UpdateLinksOnRouterMove(WorkingRouter routerView)
        {
            //update links to router
            var connectionFrom = connections.Where(x => x.RouterFrom == routerView).ToList();
            foreach (var connection in connectionFrom)
            {
                var line = connection.Line;
                line.X1 = connection.RouterFrom.Router.PositionX + routerPositionCorrective;
                line.Y1 = connection.RouterFrom.Router.PositionY + routerPositionCorrective;
            }

            var connectionTo = connections.Where(x => x.RouterTo == routerView).ToList();
            foreach (var connection in connectionTo)
            {
                var line = connection.Line;
                line.X2 = connection.RouterTo.Router.PositionX + routerPositionCorrective;
                line.Y2 = connection.RouterTo.Router.PositionY + routerPositionCorrective;
            }
        }

    }
}
