using System.Configuration;
using RoutingApp.Core.Models.NetComponents;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using RoutingApp.Core.Helpers;

namespace RoutingApp.Core.Services
{
    static class RouterSerializeService
    {
        static string defaultFilePath = ConfigurationManager.AppSettings["dataDefaultFilePath"];

        public static void SerializeRouters (Router[] routers)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(Router[]));
            using (FileStream fs = new FileStream(defaultFilePath, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, routers);
            }
        }

        public static Router[] DeserializeRouters()
        {
            //list to recreate connections
            List<Link> allLinks = new List<Link>();

            XmlSerializer formatter = new XmlSerializer(typeof(Router[]));
            Router[] routers;
            using (FileStream fs = new FileStream(defaultFilePath, FileMode.OpenOrCreate))
            {
                routers = (Router[])formatter.Deserialize(fs);
                foreach (Router router in routers)
                {
                    router.OnDeserializing();
                    foreach(var inter in router.Interfaces)
                    {
                        if (inter.IsConnected && inter.Link != null)
                        {
                            allLinks.Add(inter.Link);
                        }
                    }
                }
            }


            //recreate links here
            allLinks = allLinks.GroupBy(x=>x.Name).Select(x=>x.First()).ToList();
            allLinks.ForEach((link) =>
            {
                var interfaceNames = link.Name.Split(NameSplitters.INTERFACES_SPLITTER);
                if (interfaceNames.Length == 2)
                {
                    var router1Name = interfaceNames[0].Split(NameSplitters.ROUTERS_SPLITTER)[0];
                    var router2Name = interfaceNames[1].Split(NameSplitters.ROUTERS_SPLITTER)[0];

                    var int1 = routers.FirstOrDefault(x => x.Name == router1Name).Interfaces.FirstOrDefault(x => x.FullName == interfaceNames[0]);
                    var int2 = routers.FirstOrDefault(x => x.Name == router2Name).Interfaces.FirstOrDefault(x => x.FullName == interfaceNames[1]);

                    link.Interface1 = int1;
                    link.Interface2 = int2;

                    int1.Link = link;
                    int2.Link = link;
                }
            });

            return routers;
        }
    }
}
