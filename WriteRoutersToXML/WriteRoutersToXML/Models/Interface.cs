using System;
using System.Xml.Serialization;
using WriteRoutersToXML.Helpers;
using WriteRoutersToXML.Models.Interfaces;

namespace WriteRoutersToXML.Models
{
    [Serializable]
    public class Interface : IDeserializable
    {

        #region Fields

        [XmlIgnore]
        public Router Router { get; set; }

        public string Name { get; set; }
        public string FullName { get; set; }

        public bool IsConnected { get; set; }

        public Link Link { get; set; }

        #endregion

        #region cstor

        public Interface()
        {

        }

        public Interface (Router router, int interfaceNumber)
        {
            Router = router;
            Name = $"Int{interfaceNumber}";
            FullName = router.Name + NameSplitters.ROUTERS_SPLITTER + Name;
        }

        #endregion

        #region Public Methods

        public void OnDeserializing()
        {
            //if (Link != null)
            //{
            //    var interfacesToConnectNames = Link.Name.Split(NameSplitters.INTERFACES_SPLITTER);
            //    if (interfacesToConnectNames[0] == FullName)
            //    {
            //        Link.Interface1 = this;
            //    }
            //    else if (interfacesToConnectNames[1] == FullName)
            //    {
            //        Link.Interface2 = this;
            //    }
            //    else
            //    {
            //        throw new Exception($"Exception on interface deserializing. Couldn parse Link {Link.Name}");
            //    }
            //}
        }

        public void CreateConnection (Interface interfaceToConnect)
        {
            Link = new Link(this, interfaceToConnect);
            IsConnected = true;

            Console.WriteLine($"Initiated connection between {FullName}  and {interfaceToConnect.FullName}");

            interfaceToConnect.SubmitConnection(Link);
        }

        public void SubmitConnection(Link link)
        {
            //Check conenction
            if (link.Interface2 != this) throw new Exception($"Wrong link attached to interface. Interface {FullName}, link.int1 = {link.Interface1.FullName}, link.int2 = {link.Interface2.FullName}");
            if (IsConnected == true || Link != null) throw new Exception($"Link is already attached to {FullName}");

            Link = link;
            IsConnected = true;

            Console.WriteLine($"Successfully connected {Link.Interface1.FullName}  and {Link.Interface2.FullName}");
        }



        #endregion

    }
}
