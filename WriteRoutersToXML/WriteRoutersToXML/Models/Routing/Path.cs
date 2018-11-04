using System;
using System.Collections.Generic;

namespace WriteRoutersToXML.Models.Routing
{
    public class Path
    {
        public List<int> NodesInPath;

        public int Metric { get; set; }

        public Path()
        {

        }

        public Path (int startNode)
        {
            NodesInPath = new List<int>();
            NodesInPath.Add(startNode);
        }
        
        #region Public methods

        public void AddNodeToPath(int nodeId, int metric)
        {
            if (!NodesInPath.Contains(nodeId))
            {
                NodesInPath.Add(nodeId);
                Metric += metric;
            }
        }

        public Path Clone()
        {
            Path clonedPath = new Path();
            clonedPath.Metric = Metric;
            clonedPath.NodesInPath = new List<int>(NodesInPath);

            return clonedPath;
        }
        
        #endregion
    }
}
