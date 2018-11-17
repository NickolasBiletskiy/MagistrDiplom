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
        public Router StartNode
        {
            get
            {
                return RoutersInPath[0];
            }
        }
        public Router LastNode
        {
            get
            {
                return RoutersInPath[RoutersInPath.Count - 1];
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

        public Path GetSubPathTo(Router router)
        {
            Path subPath = new Path();

            var seekingRouterId = RoutersInPath.IndexOf(router);

            if (seekingRouterId == -1) return null;

            subPath.RoutersInPath = RoutersInPath.GetRange(0, seekingRouterId + 1);

            return subPath;
        }

        public Path GetReversePath()
        {
            var reversePath = new Path(RoutersInPath[RoutersInPath.Count - 1]);
            for (var i = RoutersInPath.Count - 2; i >= 0; i--)
            {
                reversePath.AddNodeToPath(RoutersInPath[i]);
            }
            return reversePath;
        }

        public bool DoesPathContainsRouters(List<Router> routers)
        {
            foreach (var router in RoutersInPath)
            {
                if (routers.Contains(router)) return true;
            };
            return false;
        }

        public bool DoesPathContainsRouter(Router router)
        {
            return RoutersInPath.Contains(router);
        }

        #endregion
    }
}
