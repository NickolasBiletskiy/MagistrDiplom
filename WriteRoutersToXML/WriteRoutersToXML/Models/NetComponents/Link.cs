﻿using System;
using System.Xml.Serialization;
using WriteRoutersToXML.Helpers;

namespace WriteRoutersToXML.Models.NetComponents
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

        //Metric fields
        //Currently metric is only length
        [XmlIgnore]
        public int Metric
        {
            get
            {
                return AvailableBandWidth;
            }
            private set { }
        }

        public int AvailableBandWidth { get; set; }

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
            AvailableBandWidth = Constants.LINK_MAX_BANDWIDH;
            Name = int1.FullName + NameSplitters.INTERFACES_SPLITTER + int2.FullName;
        }

        #endregion
    }
}
