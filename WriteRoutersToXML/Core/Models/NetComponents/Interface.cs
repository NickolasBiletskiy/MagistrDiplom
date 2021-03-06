﻿using System;
using System.Xml.Serialization;
using RoutingApp.Core.Extensions;
using RoutingApp.Core.Helpers;
using RoutingApp.Core.Models.Interfaces;

namespace RoutingApp.Core.Models.NetComponents
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

        [XmlIgnore]
        public bool IsBusy { get; set; }

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
            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public void OnDeserializing()
        {

        }

        public void CreateConnection (Interface interfaceToConnect)
        {            
            Link = new Link(this, interfaceToConnect);
            IsConnected = true;

            Console.WriteLine($"Initiated connection between {FullName}  and {interfaceToConnect.FullName}");

            interfaceToConnect.SubmitConnection(Link);
        }

        public void RemoveConnection()
        {
            var connectedInterace = this.GetAnotherConnectedInterface();
            connectedInterace.IsConnected = IsConnected = false;
            connectedInterace.Link = Link = null;
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
