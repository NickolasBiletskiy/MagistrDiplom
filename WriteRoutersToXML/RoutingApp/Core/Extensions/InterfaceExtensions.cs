using RoutingApp.Core.Models.NetComponents;

namespace RoutingApp.Core.Extensions
{
    public static class InterfaceExtensions
    {
        //extension method for receiving another linked interface
        public static Interface GetAnotherConnectedInterface(this Interface myInterface)
        {
            if (myInterface.IsConnected && myInterface.Link != null)
            {
                return (myInterface.Link.Interface1 == myInterface) ? myInterface.Link.Interface2 : myInterface.Link.Interface1;
            }
            return null;
        }
    }
}
