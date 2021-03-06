﻿using RoutingApp.Controls;
using RoutingApp.Core;
using RoutingApp.Core.Models.NetComponents;
using RoutingApp.Core.Services;
using RoutingApp.Helpers;
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
        public List<Link> links = new List<Link>();
        public List<WorkingRouter> routerViewModels;
        public List<ConnectionViewModel> connections;
        public WorkingController controllerUI;
        public List<ControllerConnectionViewModel> controllerConnections;

        private EditorState editorState = EditorState.RouterMove;
        private ProgramState programState = ProgramState.Edition;
        private List<Button> editorStateButtons = new List<Button>();

        //Link creating
        private WorkingRouter routerFrom;
        private bool linkDrawingStarted;

        public MainWindow()
        {
            RouterSerializeService.defaultFilePath = defaultFilePath;
            Controller.Instance.SetLogger(LoggerService.Instance);

            routers = new List<Router>();

            InitializeComponent();

            InitControllerUI();

            InitTopPanelButtonEventHandlers();

            LoggerService.Instance.OutPutTextBox = ConsoleOutput;
            connections = new List<ConnectionViewModel>();
            routerViewModels = new List<WorkingRouter>();

            controllerConnections = new List<ControllerConnectionViewModel>();
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
            editorState = EditorState.RouterDelete;
            SetMovingEnabled(false);
        }

        private void btnMoveRouter_Click(object sender, RoutedEventArgs e)
        {
            SwitchEditorState(sender as Button);
            editorState = EditorState.RouterMove;
            SetMovingEnabled(true);
        }

        private void btnAddLink_Click(object sender, RoutedEventArgs e)
        {
            SwitchEditorState(sender as Button);
            editorState = EditorState.LinkAdd;
            SetMovingEnabled(false);
        }

        private void btnRemoveLink_Click(object sender, RoutedEventArgs e)
        {
            SwitchEditorState(sender as Button);
            editorState = EditorState.LinkDelete;
            SetMovingEnabled(false);
        }

        private void btnInitTraffic_Click(object sender, RoutedEventArgs e)
        {
            CreateTrafficPopup popUpWindow = new CreateTrafficPopup();
            popUpWindow.Owner = this;
            popUpWindow.OkClicked += (routerFrom, routerTo, numberOfPackets, sizeOfPackets, desiredSpeed, trafficName) =>
            {
                Controller.Instance.InitTraffic(routerFrom, routerTo, numberOfPackets, sizeOfPackets, desiredSpeed, trafficName);
            };

            popUpWindow.ShowDialog();
        }

        private void playSimulation_Click(object sender, RoutedEventArgs e)
        {
            programState = ProgramState.Simulation;
            Controller.Instance.StartSimulation();
        }

        private void pauseSimulation_Click(object sender, RoutedEventArgs e)
        {
            programState = ProgramState.Edition;
            Controller.Instance.PauseSimulation();
        }

        private void nextStepSimulation_Click(object sender, RoutedEventArgs e)
        {
            programState = ProgramState.Edition;
            Controller.Instance.SimulateOneStep();
        }

        //Handle router click events and do different actions depending on state
        public void workingRouter_Click(object sender, RoutedEventArgs e)
        {
            if (programState == ProgramState.Edition)
            {
                WorkingRouter senderRouter = (WorkingRouter)sender;
                switch (editorState)
                {
                    case EditorState.RouterDelete:
                        var listOfConnections = connections.Where(x => x.RouterFrom == senderRouter || x.RouterTo == senderRouter);
                        //delete links from canvas
                        foreach (var connection in listOfConnections)
                        {
                            links.Remove(connection.Link);
                            WorkingArea.Children.Remove(connection.Line);
                        }
                        connections.RemoveAll(x => x.RouterFrom == senderRouter || x.RouterTo == senderRouter);

                        routers.Remove(senderRouter.Router);
                        routerViewModels.Remove(senderRouter);
                        WorkingArea.Children.Remove(senderRouter);

                        var controllerConnection = controllerConnections.FirstOrDefault(x => x.Router == senderRouter);
                        if (controllerConnection != null)
                        {
                            WorkingArea.Children.Remove(controllerConnection.Line);
                            controllerConnections.Remove(controllerConnection);
                        }

                        Controller.Instance.RemoveRouter(senderRouter.Router);
                        break;
                    case EditorState.LinkAdd:
                        if (!linkDrawingStarted)
                        {
                            if (senderRouter.Router.GetFirstFreeInterface() == null)
                            {
                                NoInterfacesMessageBox();
                                return;
                            }

                            linkDrawingStarted = true;
                            routerFrom = senderRouter;
                        }
                        else    //confirm link
                        {
                            //check self clicking
                            if (senderRouter == routerFrom) return;

                            //check free interfaces
                            if (senderRouter.Router.GetFirstFreeInterface() == null)
                            {
                                NoInterfacesMessageBox();
                                return;
                            }

                            //check routers already connected
                            if (senderRouter.Router.GetLinkToRouter(routerFrom.Router) != null)
                            {
                                MessageBox.Show("This routers are already connected");
                                return;
                            }

                            var tempLine = new Line();
                            tempLine.MouseLeftButtonDown += link_Click;
                            tempLine.X1 = routerFrom.Router.PositionX + routerPositionCorrective;
                            tempLine.Y1 = routerFrom.Router.PositionY + routerPositionCorrective;
                            tempLine.X2 = senderRouter.Router.PositionX + routerPositionCorrective;
                            tempLine.Y2 = senderRouter.Router.PositionY + routerPositionCorrective;
                            tempLine.Stroke = new SolidColorBrush(Colors.Black);

                            //Controller.Instance.Con
                            Controller.Instance.CreateConnections(routerFrom.Router, senderRouter.Router);

                            var link = routerFrom.Router.GetLinkToRouter(senderRouter.Router);

                            connections.Add(new ConnectionViewModel(routerFrom, senderRouter, tempLine, link));
                            links.Add(link);

                            WorkingArea.Children.Add(tempLine);

                            linkDrawingStarted = false;
                            routerFrom = null;
                        }
                        break;
                }
            }
        }

        public void link_Click(object sender, RoutedEventArgs e)
        {
            if (programState == ProgramState.Edition)
            {
                Line clickedLine = (Line)sender;
                switch (editorState)
                {
                    case EditorState.LinkDelete:
                        var connection = connections.FirstOrDefault(x => x.Line == clickedLine);

                        var messageBoxResult = MessageBox.Show($"Are you sure, you want to delete link between {connection.RouterFrom.Router.Name} and {connection.RouterTo.Router.Name}"
                            ,"Confirm deletion of link"
                            ,MessageBoxButton.YesNo
                            ,MessageBoxImage.Question);

                        if (messageBoxResult == MessageBoxResult.No) return;


                        //remove link from system
                        Controller.Instance.RemoveLink(connection.Link);

                        //remmove link from ui
                        connections.Remove(connection);
                        WorkingArea.Children.Remove(connection.Line);
                        break;
                }
            }
        }

        public void SwitchEditorState(Button clickedButton)
        {
            foreach (var button in editorStateButtons)
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
            routers.Add(router);
            WorkingRouter routerControl = new WorkingRouter(router);
            routerControl.OnRouterMove += UpdateLinksOnRouterMove;
            routerControl.OnRouterMove += UpdateControllerLinkOnRouterMove;
            routerControl.PreviewMouseDown += workingRouter_Click;
            routerViewModels.Add(routerControl);
            WorkingArea.Children.Add(routerControl);

            controllerConnections.Add(CreateControllerConnectionViewModel(routerControl));
            var a = 5;
        }

        private void SetMovingEnabled(bool isEnabled)
        {
            foreach (var routerView in routerViewModels)
            {
                routerView.IsMovingEnabled = isEnabled;
            }
        }

        #endregion

        public void AddRoutersToCanvas()
        {
            WorkingArea.Children.Clear();

            InitControllerUI();

            connections.Clear();
            controllerConnections.Clear();

            foreach (Router router in routers)
            {
                WorkingRouter routerControl = new WorkingRouter(router);
                routerControl.OnRouterMove += UpdateLinksOnRouterMove;
                routerControl.OnRouterMove += UpdateControllerLinkOnRouterMove;
                routerControl.PreviewMouseDown += workingRouter_Click;
                WorkingArea.Children.Add(routerControl);


                if (router != null)
                {
                    Canvas.SetLeft(routerControl, router.PositionX);
                    Canvas.SetTop(routerControl, router.PositionY);
                }

                routerViewModels.Add(routerControl);

                controllerConnections.Add(CreateControllerConnectionViewModel(routerControl));
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
            line.MouseLeftButtonDown += link_Click;
            line.X1 = router1.PositionX + routerPositionCorrective;
            line.Y1 = router1.PositionY + routerPositionCorrective;
            line.X2 = router2.PositionX + routerPositionCorrective;
            line.Y2 = router2.PositionY + routerPositionCorrective;
            line.Stroke = new SolidColorBrush(Colors.Black);

            return line;
        }

        private ControllerConnectionViewModel CreateControllerConnectionViewModel(WorkingRouter router)
        {
            Line line = new Line();
            line.X1 = router.Router.PositionX + routerPositionCorrective;
            line.Y1 = router.Router.PositionY + routerPositionCorrective;
            line.X2 = controllerUI.PositionX + routerPositionCorrective;
            line.Y2 = controllerUI.PositionY + routerPositionCorrective;
            line.Stroke = new SolidColorBrush(Colors.Blue);
            line.StrokeDashArray = new DoubleCollection(new double[1] { 15 });
            line.StrokeThickness = 0.5;

            WorkingArea.Children.Add(line);
            return new ControllerConnectionViewModel
            {
                Line = line,
                Router = router
            };
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

        public void UpdateControllerLinkOnRouterMove(WorkingRouter routerView)
        {
            var controllerConnectionViewModel = controllerConnections.FirstOrDefault(x => x.Router == routerView);
            if (controllerConnectionViewModel == null) return;

            var line = controllerConnectionViewModel.Line;
            line.X1 = controllerUI.PositionX + routerPositionCorrective;
            line.Y1 = controllerUI.PositionY + routerPositionCorrective;
            line.X2 = routerView.Router.PositionX + routerPositionCorrective;
            line.Y2 = routerView.Router.PositionY + routerPositionCorrective;
        }

        public void UpdateControllerLinksOnControllerMove()
        {
            foreach(var connection in controllerConnections)
            {
                var line = connection.Line;
                line.X1 = controllerUI.PositionX + routerPositionCorrective;
                line.Y1 = controllerUI.PositionY + routerPositionCorrective;
                line.X2 = connection.Router.Router.PositionX + routerPositionCorrective;
                line.Y2 = connection.Router.Router.PositionY + routerPositionCorrective;
            }
        }

        public void InitControllerUI()
        {
            if (controllerUI == null)
            {
                controllerUI = new WorkingController
                {
                    PositionX = 150,
                    PositionY = 10
                };
                controllerUI.OnControllerMove += UpdateControllerLinksOnControllerMove;
            }

            WorkingArea.Children.Add(controllerUI);
            Canvas.SetLeft(controllerUI, controllerUI.PositionX);
            Canvas.SetTop(controllerUI, controllerUI.PositionY);
        }

        #region MessageBoxes

        public void NoInterfacesMessageBox()
        {
            MessageBox.Show("There are not free interfaces, choose another router");
        }

        #endregion
    }
}
