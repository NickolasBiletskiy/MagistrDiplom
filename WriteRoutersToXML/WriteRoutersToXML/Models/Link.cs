using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using WriteRoutersToXML.Helpers;

namespace WriteRoutersToXML.Models
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
            Name = int1.FullName + NameSplitters.INTERFACES_SPLITTER + int2.FullName;
        }

        #endregion
    }
}
