using System;
using System.Xml.Serialization;
using RoutingApp.Core.Helpers;

namespace RoutingApp.Core.Models.NetComponents
{
    [Serializable]
    public class Link
    {

        #region Fields

        public bool IsActive { get; set; }
        public string Name { get; set; }
        [XmlIgnore]
        public Interface Interface1 { get; set; }
        [XmlIgnore]
        public Interface Interface2 { get; set; }
        [XmlIgnore]
        public Action<int> MetricChanged;

        //Metric fields
        //Currently metric is only length   

        [XmlIgnore]
        private int _metric;
        public int Metric
        {
            get
            {
                return _metric;
            }
            set
            {
                _metric = value;
                MetricChanged?.Invoke(Metric);
            }
        }

        #endregion

        #region cstor

        public Link()
        {

        }

        public Link(Interface int1, Interface int2)
        {
            Interface1 = int1;
            Interface2 = int2;
            IsActive = true;
            Metric = Constants.LINK_MAX_BANDWIDH;
            Name = int1.FullName + NameSplitters.INTERFACES_SPLITTER + int2.FullName;
        }

        #endregion
    }
}
