﻿using Core.Services;
using RoutingApp.Core.Models.Interfaces;
using RoutingApp.Core.Models.Routing;
using RoutingApp.Core.Models.SystemSimulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace RoutingApp.Core.Models.NetComponents
{
    [Serializable]
    public class Router : IDeserializable
    {

        #region Fields

        public Guid Id { get; set; }

        [XmlIgnore]
        private int _routerInSystemId;
        public int RouterInSystemId
        {
            get
            {
                return _routerInSystemId;
            }
            set
            {
                _routerInSystemId = value;
                RouterIndexChanged?.Invoke(Name);
            }
        }

        [XmlIgnore]
        public Action<string> RouterIndexChanged;

        [XmlIgnore]
        private string _name;
        public string Name { get {
                return "router" + RouterInSystemId;
            } set { _name = value; } }
        public Interface[] Interfaces { get; set; }
        public bool IsActive { get; set; }

        //Position on canvas
        public double PositionX { get; set; }
        public double PositionY { get; set; }

        [XmlIgnore]
        public List<Packet> CashedPackets = new List<Packet>();

        [XmlIgnore]
        public List<Packet> ReceivedPackets = new List<Packet>();

        [XmlIgnore]
        public List<RoutingTableItem> RoutingTable = new List<RoutingTableItem>();  //destination router, first router in the path

        [XmlIgnore]
        public ILogger LoggerService;

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
            foreach (Interface inter in Interfaces.Where(x => x.IsConnected).ToList())
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
                var packetToSend = trafficPackets.ToList().FirstOrDefault(x => x.IsSending && !x.IsTransmitted);
                if (packetToSend == null)
                {
                    packetToSend = trafficPackets.ToList().FirstOrDefault();
                    packetToSend.CurrentRouter = this;
                    packetToSend.IsSending = true;
                    packetToSend.SendingToRouter = GetNextTransitionRouter(packetToSend.Traffic.DestinationRouter);
                    packetToSend.Link = GetLinkToRouter(packetToSend.SendingToRouter);
                    packetToSend.Speed = Math.Min(packetToSend.Link.Metric, packetToSend.Traffic.DesiredBandWidth);

                    packetToSend.Link.Metric -= packetToSend.Speed;
                }
                else
                {
                    //check destination router still exists
                    if (!packetToSend.SendingToRouter.IsActive)
                    {
                        LoggerService.Log("Previous router disconencted. Changed direction");
                        packetToSend.SendingToRouter = GetNextTransitionRouter(packetToSend.Traffic.DestinationRouter);
                        packetToSend.Link = GetLinkToRouter(packetToSend.SendingToRouter);
                        packetToSend.Speed = Math.Min(packetToSend.Link.Metric, packetToSend.Traffic.DesiredBandWidth);
                        packetToSend.IsSending = true;
                        packetToSend.Progress = 0;
                    }
                }
                if (packetToSend.IsJustReceived == true)
                {
                    packetToSend.IsJustReceived = false;
                    continue;
                }

                //send here
                packetToSend.Progress += 100 * (packetToSend.Speed * (double)Constants.UPDATE_TIME / 1000) / packetToSend.Size; //convert to seconds
                LoggerService.Log($"{packetToSend.Traffic.Name}: packet ID={packetToSend.PacketID + 1}/{packetToSend.Traffic.Packets.Count}  from {packetToSend.CurrentRouter.RouterInSystemId} to {packetToSend.Traffic.DestinationRouter.RouterInSystemId} through {packetToSend.SendingToRouter.RouterInSystemId}. Progression = {packetToSend.Progress}");
                // check packet is sent
                if (packetToSend.Progress >= 100)
                {
                    CashedPackets.Remove(packetToSend);

                    packetToSend.IsSending = false;
                    packetToSend.IsJustReceived = true;
                    packetToSend.Progress = 0;

                    packetToSend.Link.Metric += packetToSend.Speed;
                    packetToSend.Link = null;
                    packetToSend.Speed = 0;

                    //packet came to final router
                    if (packetToSend.SendingToRouter == packetToSend.Traffic.DestinationRouter)
                    {
                        LoggerService.Log($"Packet ID={packetToSend.PacketID} for traffic {packetToSend.Traffic.Name} received destination router.");
                        packetToSend.IsTransmitted = true;
                        packetToSend.Traffic.TransmittedPackets.Add(packetToSend);
                        packetToSend.SendingToRouter.ReceivedPackets.Add(packetToSend);
                    }
                    else
                    {
                        packetToSend.SendingToRouter.CashedPackets.Add(packetToSend);
                    }
                }
            }
        }

        public Router GetNextTransitionRouter(Router destinationRouter)
        {
            var closestRouterRoutingItem = RoutingTable.FirstOrDefault(x => x.DestinationRouter == destinationRouter);

            if (closestRouterRoutingItem == null || closestRouterRoutingItem.IsExpired || !closestRouterRoutingItem.ClosestRouter.IsActive)
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

            });
            return result.ToString();
        }

        #endregion

        #endregion
    }
}
