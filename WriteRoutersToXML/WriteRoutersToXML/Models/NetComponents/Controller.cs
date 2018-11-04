using System;
using System.Collections.Generic;
using System.Text;
using WriteRoutersToXML.Extensions;
using WriteRoutersToXML.Models.Routing;
using WriteRoutersToXML.Services;

namespace WriteRoutersToXML.Models.NetComponents
{
    public class Controller
    {

        #region Fields

        private ICollection<Router> _routers;

        private int[,] linkMatrix;

        //TODO create structure to save all paths
        private List<Path> currentPaths;

        #endregion

        //Singleton
        private static Controller _instance = null;
        private static readonly object _lock = new object();
        public static Controller Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Controller();
                    }
                    return _instance;
                }
            }
            private set { }
        }

        #region cstor

        //private constructor for singleton
        private Controller()
        {

        }

        #endregion

        #region Public Methods

        public void InitializeController(ICollection<Router> routers)
        {
            _routers = routers;
            linkMatrix = new int[routers.Count, routers.Count];

            foreach (var router in routers)
            {
                foreach (var inter in router.Interfaces)
                {
                    if (inter.IsConnected)
                    {
                        var anotherConnectedInterface = inter.GetAnotherConnectedInterface();
                        if (anotherConnectedInterface != null)
                        {
                            linkMatrix[router.Id, anotherConnectedInterface.Router.Id] = inter.Link.Metric;
                        }
                    }
                }
            }

            LoggerService.Instance.CustomizeOutput(LogType.ControllerLog, LogInitializing);

            LoggerService.Instance.CustomizeOutput(LogType.RouterLog, LogRouterConnections);
        }

        public List<Path> GetAllPaths(int routerFrom, int routerTo)
        {
            RoutingService routingService = new RoutingService(linkMatrix);
            currentPaths = routingService.GetAllPaths(routerFrom, routerTo);

            LoggerService.Instance.CustomizeOutput(LogType.Paths, LogPaths);

            return currentPaths;
        }

        #region Log methods

        public void LogInitializing()
        {
            Console.WriteLine("Controller initialized \nConnection matrix");
            //top header
            StringBuilder topHeader = new StringBuilder("\t");
            for (var i = 0; i < _routers.Count; i++)
            {
                topHeader.Append(i + "\t");
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(topHeader.ToString());
            Console.ForegroundColor = ConsoleColor.White;

            for (var i = 0; i < linkMatrix.GetLength(0); i++)
            {
                for (var j = 0; j < linkMatrix.GetLength(1); j++)
                {
                    if (j == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(i + "\t");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.Write(linkMatrix[i, j] + "\t");
                }
                Console.WriteLine();
            };
        }

        public void LogRouterConnections()
        {
            foreach (Router router in _routers)
            {
                router.LogConnections();
            }
        }

        public void LogPaths()
        {
            StringBuilder result = new StringBuilder();
            foreach (Path path in currentPaths)
            {
                foreach (int nodeId in path.NodesInPath)
                {
                    result.Append(nodeId);
                    if (nodeId != path.NodesInPath[path.NodesInPath.Count - 1])
                    {
                        result.Append("->");
                    } else
                    {
                        result.Append("\tMetric = " + path.Metric);
                    }
                }
                result.Append("\n");
            }
            Console.WriteLine(result.ToString());
        }

        #endregion

        #endregion
    }
}
