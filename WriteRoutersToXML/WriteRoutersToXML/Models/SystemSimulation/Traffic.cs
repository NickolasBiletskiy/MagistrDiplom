using System.Collections.Generic;
using System.Linq;
using WriteRoutersToXML.Models.NetComponents;

namespace WriteRoutersToXML.Models.SystemSimulation
{
    public class Traffic
    {
        public Router InitiatorRouter { get; set; }
        public Router DestinationRouter { get; set; }
        public List<Packet> Packets { get; set; }
        public int DesiredBandWidth { get; set; }
        public List<Packet> TransmittedPackets
        {
            get
            {
                return Packets.Where(x => x.IsTransmitted).ToList();
            }
        }

        #region cstor

        public Traffic(Router routerFrom, Router routerTo, int numberOfPackets, int sizeOfPackets, int desiredSpeed)
        {
            InitiatorRouter = routerFrom;
            DestinationRouter = routerTo;
            DesiredBandWidth = desiredSpeed;
            Packets = Enumerable.Repeat(new Packet
            (
                this,
                sizeOfPackets,
                InitiatorRouter                
            ), numberOfPackets).ToList();
        }

        #endregion
    }
}
