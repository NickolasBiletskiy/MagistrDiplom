using WriteRoutersToXML.Models.NetComponents;

namespace WriteRoutersToXML.Models.SystemSimulation
{
    public class Packet
    {

        #region Fields

        public Traffic Traffic { get; private set; }
        public bool IsSending { get; set; }
        public bool IsTransmitted { get; set; }
        public int Progression { get; set; }    //percentage of packet sending
        public int Size { get; set; }
        public double Speed { get; set; }
        public Router CurrentRouter { get; set; }
        public Router SendingToRouter { get; set; }
        public Link Link { get; set; }

        #endregion

        #region cstor

        public Packet(Traffic traffic, int size, Router startRouter)
        {
            Traffic = traffic;
            Size = size;
            CurrentRouter = startRouter;
        }

        #endregion
    }
}
