using System;
using System.Collections.Generic;
using WriteRoutersToXML.Models.NetComponents;

namespace WriteRoutersToXML.Models.Routing
{
    public class Path
    {
        public Guid PathId;
        public List<Router> RoutersInPath;

        public int Metric { get; set; }

        public Path()
        {
            PathId = Guid.NewGuid();
            RoutersInPath = new List<Router>();
        }

        public Path (Router startRouter) : this()
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
