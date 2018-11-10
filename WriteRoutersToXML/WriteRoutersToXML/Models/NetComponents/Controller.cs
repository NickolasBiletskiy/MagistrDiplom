using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WriteRoutersToXML.Extensions;
using WriteRoutersToXML.Models.Routing;
using WriteRoutersToXML.Services;

namespace WriteRoutersToXML.Models.NetComponents
{
    public class Controller
    {

        #region Fields

        private List<Router> _routers;

        //private int[,] linkMatrix;

        //dictionary for saving allpahts between points. Key -> Tuple(nodeFrom, nodeTo), Value - List<Path>
        private Dictionary<Tuple<int, int>, List<Path>> _allPaths = new Dictionary<Tuple<int, int>, List<Path>>();

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

        #endregion

        #region cstor

        //private constructor for singleton
        private Controller()
        {

        }

        #endregion

        #region Public Methods

        public void InitializeController(List<Router> routers)
        {
            _routers = routers;

            LoggerService.Instance.CustomizeOutput(LogType.ControllerLog, LogInitializing);

            LoggerService.Instance.CustomizeOutput(LogType.RouterLog, LogRouterConnections);
        }

        public List<Path> GetAllPaths(int routerFrom, int routerTo)
        {
            var tuple = new Tuple<int, int>(routerFrom, routerTo);
            List<Path> currentPaths;
            if (_allPaths.TryGetValue(tuple, out currentPaths))
            {
                return currentPaths;
            }

            var paths = new List<Path>();
            var linkMatrix = GetLinkMatrix();

            Path startPath = new Path(_routers.FirstOrDefault(x => x.RouterInSystemId == routerFrom));

            CreateNewPath(startPath, routerFrom, routerTo, paths, linkMatrix);

            _allPaths.Add(tuple, paths);

            LoggerService.Instance.CustomizeOutput(LogType.Paths, () =>
            {
                LogPaths(routerFrom, routerTo, _allPaths[tuple]);
            });

            return _allPaths[tuple];
        }

        public void GetAllConnections()
        {
            LoggerService.Instance.CustomizeOutput(LogType.ControllerLog, LogInitializing);

            LoggerService.Instance.CustomizeOutput(LogType.RouterLog, LogRouterConnections);
        }

        public Router AddNewRouter()
        {
            int lastRouterIndex = _routers.OrderByDescending(x => x.RouterInSystemId).Select(x => x.RouterInSystemId).FirstOrDefault();
            Router router = new Router("router" + ++lastRouterIndex);
            _routers.Add(router);

            return router;
        }

        public void CreateConnectionsForRouter(Router router, params int[] routerInSystemsIds)
        {
            List<Router> routers = _routers.Where(x => routerInSystemsIds.Contains(x.RouterInSystemId)).ToList();
            foreach (Router currentRouter in routers)
            {
                router.ConnectTo(currentRouter);
            }

            //TODO update paths here
        }

        #region Routing

        private void CreateNewPath(Path path, int currentNode, int lastNode, List<Path> paths, int[,] linkMatrix)
        {
            bool isFirstInterface = true;
            Path currentPath = path;
            var pathSavePoint = path.Clone();
            for (var j = 0; j < linkMatrix.GetLength(1); j++)
            {
                if (linkMatrix[currentNode, j] != 0)
                {
                    Router currentRouter = _routers.FirstOrDefault(x => x.RouterInSystemId == j);
                    if (pathSavePoint.RoutersInPath.Contains(currentRouter))
                    {
                        continue;
                    }

                    if (isFirstInterface)
                    {
                        isFirstInterface = false;
                    }
                    else
                    {
                        currentPath = pathSavePoint.Clone();
                    }

                    currentPath.AddNodeToPath(currentRouter);

                    if (j != lastNode)
                    {
                        CreateNewPath(currentPath, j, lastNode, paths, linkMatrix);
                    }
                    else
                    {
                        //Path is full
                        paths.Add(currentPath);
                    }
                }
            }
        }

        #endregion

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

            var linkMatrix = GetLinkMatrix();
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

        public void LogPaths(int routerFrom, int routerTo, List<Path> paths)
        {
            StringBuilder result = new StringBuilder();
            foreach (Path path in paths)
            {
                foreach (Router router in path.RoutersInPath)
                {
                    result.Append(router.RouterInSystemId);
                    if (router != path.RoutersInPath[path.RoutersInPath.Count - 1])
                    {
                        result.Append("->");
                    }
                    else
                    {
                        result.Append($"\tMetric = {path.Metric} {Constants.BANDWIDTH_UNITS} PathId = {path.PathId}");
                    }
                }
                result.Append("\n");
            }
            Console.WriteLine(result.ToString());
        }

        #endregion

        #endregion

        #region Private Methods

        private int[,] GetLinkMatrix()
        {
            int[,] linkMatrix = new int[_routers.Count, _routers.Count];

            int routerInSystemId = 0;

            foreach (var router in _routers)
            {
                router.RouterInSystemId = routerInSystemId++;
            }

            foreach (var router in _routers)
            {
                foreach (var inter in router.Interfaces)
                {
                    if (inter.IsConnected)
                    {
                        var anotherConnectedInterface = inter.GetAnotherConnectedInterface();
                        if (anotherConnectedInterface != null)
                        {
                            linkMatrix[router.RouterInSystemId, anotherConnectedInterface.Router.RouterInSystemId] = 1;
                        }
                    }
                }
            }

            return linkMatrix;
        }

        #endregion
    }
}
