using System;
using System.Collections.Generic;
using WriteRoutersToXML.Models.Routing;

namespace WriteRoutersToXML.Services
{
    public class RoutingService
    {
        #region Fields

        private int[,] connectionMatrix { get; set; }
        private List<Path> paths;

        #endregion

        #region cstor

        public RoutingService(int[,] connectionMatrix)
        {
            this.connectionMatrix = connectionMatrix;
        }

        #endregion

        #region Public methods

        public List<Path> GetAllPaths(int routerFrom, int routerTo)
        {
            paths = new List<Path>();
            Path startPath = new Path(routerFrom);
            //paths.Add(startPath);
            CreateNewPath(startPath, routerFrom, routerTo);
            return paths;
        }

        #endregion

        #region

        private void CreateNewPath(Path path, int currentNode, int lastNode)
        {
            //bool isFirstInterface = true;
            for (var j = 0; j < connectionMatrix.GetLength(1); j++)
            {
                if (connectionMatrix[currentNode, j] != 0)
                {
                    if (path.NodesInPath.Contains(j))
                    {
                        continue;
                    }
                    Path newPath = path.Clone();
                    newPath.AddNodeToPath(j, connectionMatrix[currentNode, j]);

                    //if (isFirstInterface) isFirstInterface = false;

                    if (j != lastNode)
                    {
                        CreateNewPath(newPath, j, lastNode);
                    } else
                    {
                        //Path is full
                        paths.Add(newPath);
                    }                
                }
            }
        }

        #endregion
    }
}
