using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WriteRoutersToXML.Models.Interfaces;
using WriteRoutersToXML.Models.Routing;
using WriteRoutersToXML.Models.SystemSimulation;

namespace WriteRoutersToXML.Models.NetComponents
{
    [Serializable]
    public class Router : IDeserializable
    {

        #region Fields

        public Guid Id { get; set; }
        public int RouterInSystemId { get; set; }
        public string Name { get; set; }
        public Interface[] Interfaces { get; set; }
        public bool IsActive { get; set; }

        [XmlIgnore]
        public List<Packet> CashedPackets = new List<Packet>();

        [XmlIgnore]
        public List<RoutingTableItem> RoutingTable = new List<RoutingTableItem>();  //destination router, first router in the path

        #endregion

        #region cstor

        public Router()
        {
            Id = Guid.NewGuid();
        }

        public Router(string name) : this(name, Constants.ROUTER_DEFAULT_INTERFACE_COUNT)
        {

        }

        public Router(string name, int numberOfInterfaces) : this()
        {
            Name = name;
            IsActive = true;

            Interfaces = new Interface[numberOfInterfaces];
            for (var i = 0; i < numberOfInterfaces; i++)
            {
                Interfaces[i] = new Interface(this, i);
            }
        }

        #endregion

        #region Public methods

        public void OnDeserializing()
        {
            foreach (Interface inter in Interfaces)
            {
                inter.Router = this;
            }

        }

        public void ConnectTo(Router anotherRouter)
        {
            Interface interFaceFrom = GetFirstFreeInterface();
            if (interFaceFrom == null) throw new Exception($"There no free interfaces in {Name}");
            Interface interfaceTo = anotherRouter.GetFirstFreeInterface();
            if (interfaceTo == null) throw new Exception($"There no free interfaces in {interfaceTo.Router.Name}");

            interFaceFrom.CreateConnection(interfaceTo);
        }

        public void RemoveConnections()
        {
            foreach (Interface inter in Interfaces.Where(x=>x.IsConnected).ToList())
            {
                inter.RemoveConnection();
            }
        }

        public Link GetLinkToRouter(Router anotherRouter)
        {
            foreach (var inter in Interfaces.Where(x => x.IsConnected).ToList())
            {
                if ((inter.Link.Interface1.Router == this && inter.Link.Interface2.Router == anotherRouter) || (inter.Link.Interface1.Router == anotherRouter && inter.Link.Interface2.Router == this))
                {
                    return inter.Link;
                }
            }

            return null;
        }

        public Interface GetFirstFreeInterface()
        {
            return Interfaces.FirstOrDefault(x => !x.IsConnected);
        }

        #region Traffic

        public void SendPackets()
        {
            var packetTrafficGroups = CashedPackets.GroupBy(x => x.Traffic).ToList();
            foreach (var trafficPackets in packetTrafficGroups)
            {
                var packetToSend = trafficPackets.ToList().FirstOrDefault(x => x.IsSending);
                if (packetToSend == null) {
                    packetToSend = trafficPackets.ToList().FirstOrDefault();
                    packetToSend.CurrentRouter = this;
                    packetToSend.IsSending = true;
                    packetToSend.SendingToRouter = GetNextTransitionRouter(packetToSend.Traffic.DestinationRouter);
                }

                
            }
        }

        public Router GetNextTransitionRouter(Router destinationRouter)
        {
            var closestRouterRoutingItem = RoutingTable.FirstOrDefault(x => x.DestinationRouter == destinationRouter);
            if (closestRouterRoutingItem == null || closestRouterRoutingItem.IsExpired)
            {
                if (closestRouterRoutingItem != null) RoutingTable.Remove(closestRouterRoutingItem);
                closestRouterRoutingItem = new RoutingTableItem
                {
                    ClosestRouter = Controller.Instance.GetClothestRouter(this, destinationRouter),
                    DestinationRouter = destinationRouter,
                    LastPathUpdateTS = DateTime.UtcNow
                };
                RoutingTable.Add(closestRouterRoutingItem);
            }

            return closestRouterRoutingItem.ClosestRouter;
        }

        #endregion

        #region Logging

        public string LogConnections()
        {
            StringBuilder result = new StringBuilder($"{Name} routerIsSystemId={RouterInSystemId} id={Id}");
            result.Append("\n");
            Interfaces.Where(x => x.IsConnected).ToList().ForEach((inter) =>
            {
                string anotherRouterName = (inter.Link.Interface1 == inter) ? inter.Link.Interface2.Router.Name : inter.Link.Interface1.Router.Name;
                result.Append($"Connection to {anotherRouterName} using {inter.FullName}, Metric = {inter.Link.Metric} \n");
                //result.Append("Connection to");
            });
            Console.WriteLine(result.ToString());
            return result.ToString();
        }

        #endregion

        #endregion
    }
}
