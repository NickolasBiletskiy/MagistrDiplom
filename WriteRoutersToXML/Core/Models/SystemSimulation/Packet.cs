using RoutingApp.Core.Models.NetComponents;

namespace RoutingApp.Core.Models.SystemSimulation
{
    public class Packet
    {

        #region Fields

        public Traffic Traffic { get; private set; }
        public bool IsSending { get; set; }
        public bool IsJustReceived { get; set; }
        public bool IsTransmitted { get; set; }
        public int PacketID { get; set; }

        private double _progress;
        public double Progress
        {
            get
            { return _progress; }
            set
            {
                _progress = value;
                if (_progress > 100) _progress = 100;
            }
        }    //percentage of packet sending
        public int Size { get; set; }
        public int Speed { get; set; }
        public Router CurrentRouter { get; set; }
        public Router SendingToRouter { get; set; }
        public Link Link { get; set; }

        #endregion

        #region cstor

        public Packet(Traffic traffic, int size, Router startRouter, int packedID)
        {
            Traffic = traffic;
            Size = size;
            CurrentRouter = startRouter;
            PacketID = packedID;
        }

        #endregion
    }
}
