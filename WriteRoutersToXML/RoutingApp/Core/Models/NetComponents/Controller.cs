using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingApp.Core.Extensions;
using RoutingApp.Core.Models.Routing;
using RoutingApp.Core.Models.SystemSimulation;
using RoutingApp.Core.Services;


namespace RoutingApp.Core.Models.NetComponents
{
    public class Controller
    {

        #region Fields

        private List<Router> _routers;

        private List<Traffic> _activeTraffic;

        private SystemSimulator _systemSimulator;

        //dictionary for saving allpahts between points. Key -> Tuple(nodeFrom, nodeTo), Value - List<Path>
        private Dictionary<Tuple<Router, Router>, List<Path>> _allPaths = new Dictionary<Tuple<Router, Router>, List<Path>>();

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
            _activeTraffic = new List<Traffic>();
        }

        #endregion

        #region Public Methods

        public void InitializeController(List<Router> routers)
        {
            _routers = routers;

            LoggerService.Instance.CustomizeOutput(LogType.ControllerLog, LogInitializing);

            LoggerService.Instance.CustomizeOutput(LogType.RouterLog, LogRouterConnections);
        }                         

        public Router GetClothestRouter(Router routerFrom, Router routerTo)
        {
            Tuple<Router, Router> searchTuple = new Tuple<Router, Router>(routerFrom, routerTo);
            List<Path> paths;

            Path bestPath = null;
            if (_allPaths.TryGetValue(searchTuple, out paths))
            {
                bestPath = paths.OrderByDescending(x => x.Metric).FirstOrDefault();
            }
            else
            {
                bestPath = GetAllPaths(routerFrom.RouterInSystemId, routerTo.RouterInSystemId).OrderByDescending(x => x.Metric).FirstOrDefault();
            }

            if (bestPath.RoutersInPath.Count > 1)
            {
                return bestPath.RoutersInPath[1];
            }


            return bestPath.RoutersInPath[0];
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
            router.RouterInSystemId = lastRouterIndex;
            _routers.Add(router);

            UpdateRouterInSystemIds();

            return router;
        }

        public void CreateConnectionsForRouter(Router router, params int[] routerInSystemsIds)
        {
            List<Router> routers = _routers.Where(x => routerInSystemsIds.Contains(x.RouterInSystemId)).ToList();
            foreach (Router currentRouter in routers)
            {
                router.ConnectTo(currentRouter);
            }

            UpdatePathsFromRouters(routers, router);
        }

        public void RemoveRouter(int routerInSystemID)
        {
            Console.WriteLine($"Initiated removing router {routerInSystemID}");
            Router router = _routers.FirstOrDefault(x => x.RouterInSystemId == routerInSystemID);

            router.RemoveConnections();
            _routers.Remove(router);

            RemoveAllPathsContainingRouter(router);


            UpdateRouterInSystemIds();
        }

        #region Routing

        public List<Path> GetAllPaths(int routerFrom, int routerTo)
        {
            Router routerStart = _routers.FirstOrDefault(x => x.RouterInSystemId == routerFrom);
            Router routerEnd = _routers.FirstOrDefault(x => x.RouterInSystemId == routerTo);

            var tuple = new Tuple<Router, Router>(routerStart, routerEnd);
            List<Path> currentPaths;
            if (_allPaths.TryGetValue(tuple, out currentPaths))
            {
                LoggerService.Instance.CustomizeOutput(LogType.Paths, () =>
                {
                    LogPaths(routerFrom, routerTo, currentPaths);
                });
                return currentPaths;
            }

            var paths = new List<Path>();
            var linkMatrix = GetLinkMatrix();

            Path startPath = new Path(_routers.FirstOrDefault(x => x.RouterInSystemId == routerFrom));

            CreateNewPath(startPath, routerFrom, routerTo, paths, linkMatrix);

            _allPaths.Add(tuple, paths);

            //fill alternative paths
            FillReversePaths(tuple, paths);

            LoggerService.Instance.CustomizeOutput(LogType.Paths, () =>
            {
                LogPaths(routerFrom, routerTo, _allPaths[tuple]);
            });

            return _allPaths[tuple];
        }

        public void FillReversePaths(Tuple<Router, Router> routerPair, List<Path> paths)
        {
            var reverseTuple = new Tuple<Router, Router>(routerPair.Item2, routerPair.Item1);
            List<Path> reversePaths;

            //if such paths are not yet calculated
            if (!_allPaths.TryGetValue(reverseTuple, out reversePaths))
            {
                reversePaths = new List<Path>();
                foreach (Path path in paths)
                {
                    reversePaths.Add(path.GetReversePath());
                }
                _allPaths.Add(reverseTuple, reversePaths);
            }
        }

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
                        if (!IsPathContainsInList(paths, currentPath))
                        {
                            paths.Add(currentPath);
                        }
                    }
                }
            }
        }

        /* Method for updating paths after adding new node. 
        /  Get all suppathes and continue it to new router
        */
        private void UpdatePathsFromRouters(List<Router> routersWithConnectionsToNewRouter, Router newRouter)
        {
            List<Path> paths = _allPaths.Values.SelectMany(x => x).Where(x => x.DoesPathContainsRouters(routersWithConnectionsToNewRouter)).ToList();

            var linkMatrix = GetLinkMatrix();

            foreach (Path path in paths)
            {
                Router routerStart = path.StartNode;
                Router routerEnd = path.LastNode;

                var tuple = new Tuple<Router, Router>(routerStart, routerEnd);
                List<Path> currentPaths; //we will update this with new paths
                if (!_allPaths.TryGetValue(tuple, out currentPaths))
                {
                    currentPaths = new List<Path>();
                }

                foreach (Router connectedWithNewRouter in routersWithConnectionsToNewRouter)
                {
                    if (connectedWithNewRouter == routerEnd) continue;  //connection to end node

                    Path startPath = path.GetSubPathTo(connectedWithNewRouter);

                    if (startPath == null) continue;    //there is no subpass to router

                    startPath.AddNodeToPath(newRouter);

                    CreateNewPath(startPath, newRouter.RouterInSystemId, path.LastNode.RouterInSystemId, currentPaths, linkMatrix);
                }

                if (_allPaths.ContainsKey(tuple))
                {
                    _allPaths[tuple] = currentPaths;
                }
                else
                {
                    _allPaths.Add(tuple, currentPaths);
                }
            }
        }

        private bool IsPathContainsInList(List<Path> paths, Path pathToCheck)
        {
            foreach (Path path in paths)
            {
                bool isPathExists = false;
                if (path.RoutersInPath.Count != pathToCheck.RoutersInPath.Count) continue;
                for (var i = 0; i < path.RoutersInPath.Count; i++)
                {
                    if (path.RoutersInPath[i] != pathToCheck.RoutersInPath[i])
                    {
                        isPathExists = false;
                        break;
                    }
                    isPathExists = true;
                }

                if (isPathExists) return true;

            }
            return false;
        }

        private void RemoveAllPathsContainingRouter(Router router)
        {
            Console.WriteLine($"Removing all paths containing {router.RouterInSystemId}");

            for (var i = 0; i < _allPaths.Keys.Count; i++)
            {
                var key = _allPaths.Keys.ToList()[i];
                var pathsForKey = _allPaths[key];

                var pathsWhichContainsRouter = pathsForKey.Where(x => x.DoesPathContainsRouter(router)).ToList();
                if (pathsWhichContainsRouter.Count != 0)
                {
                    _allPaths[key] = pathsForKey.Except(pathsWhichContainsRouter).ToList();
                }
            }

            foreach (var pathKeyValuePair in _allPaths)
            {
                var pathsWhichContainsRouter = pathKeyValuePair.Value.Where(x => x.DoesPathContainsRouter(router)).ToList();
                if (pathsWhichContainsRouter.Count != 0)
                {
                    _allPaths[pathKeyValuePair.Key] = pathKeyValuePair.Value.Except(pathsWhichContainsRouter).ToList();
                }
            }
        }

        private void UpdateRouterInSystemIds()
        {
            for (var routerId = 0; routerId < _routers.Count; routerId++)
            {
                _routers[routerId].RouterInSystemId = routerId;
            }
        }

        #endregion

        #region Traffic

        public void InitTraffic(int routerFromid, int routerToid, int numberOfPackets, int sizeOfPackets, int desiredSpeed, string trafficName)
        {
            Router routerFrom = _routers.FirstOrDefault(x => x.RouterInSystemId == routerFromid);
            Router routerTo = _routers.FirstOrDefault(x => x.RouterInSystemId == routerToid);

            if (routerFrom == null || routerTo == null) return;

            Traffic traffic = new Traffic(routerFrom, routerTo, numberOfPackets, sizeOfPackets, desiredSpeed, trafficName);

            _activeTraffic.Add(traffic);

            routerFrom.CashedPackets.AddRange(traffic.Packets);
        }

        public void InitSimulation()
        {
            _systemSimulator = new SystemSimulator(_routers);
        }

        public void StartSimulation()
        {
            _systemSimulator.IsPaused = false;
        }

        public void PauseSimulation()
        {
            _systemSimulator.IsPaused = true;
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
