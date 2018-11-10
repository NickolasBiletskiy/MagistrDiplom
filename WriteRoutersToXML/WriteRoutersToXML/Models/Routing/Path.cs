using System;
using System.Collections.Generic;
using WriteRoutersToXML.Models.NetComponents;

namespace WriteRoutersToXML.Models.Routing
{
    public class Path
    {
        public Guid PathId;
        public List<Router> RoutersInPath;

        //Metric = average speed / hops count
        public double Metric
        {
            get
            {
                double metric = 0;
                for (var i = 1; i < RoutersInPath.Count; i++)
                {
                    metric += RoutersInPath[i - 1].GetLinkToRouter(RoutersInPath[i]).Metric;
                }
                return metric / Math.Pow(RoutersInPath.Count - 1, 2);
            }
        }

        public Path()
        {
            PathId = Guid.NewGuid();
            RoutersInPath = new List<Router>();
        }

        public Path(Router startRouter) : this()
        {
            RoutersInPath.Add(startRouter);
        }

        #region Public methods

        public void AddNodeToPath(Router router)
        {
            if (!RoutersInPath.Contains(router))
            {
                RoutersInPath.Add(router);
            }
        }

        public Path Clone()
        {
            Path clonedPath = new Path();

            clonedPath.RoutersInPath = new List<Router>(RoutersInPath);

            return clonedPath;
        }

        #endregion
    }
}
