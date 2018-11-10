using System;
using System.Linq;
using System.Text;
using WriteRoutersToXML.Models.Interfaces;

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

        public void GenerateTraffc(int routerTo)
        {
            var path = Controller.Instance.GetAllPaths(RouterInSystemId, routerTo);
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
