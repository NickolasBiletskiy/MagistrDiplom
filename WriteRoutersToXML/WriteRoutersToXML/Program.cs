using System;
using System.Configuration;
using System.IO;
using WriteRoutersToXML.Models;
using WriteRoutersToXML.Services;

namespace WriteRoutersToXML
{
    class Program
    {
        static string defaultFilePath = ConfigurationManager.AppSettings["dataDefaultFilePath"];
        static void Main(string[] args)
        {
            if (!File.Exists(defaultFilePath))
            {

                var routers = new Router[5];

                for (var i = 0; i < 5; i++)
                {
                    routers[i] = new Router("router" + i, 5);
                }

                routers[0].ConnectTo(routers[2]);
                routers[0].ConnectTo(routers[3]);
                routers[1].ConnectTo(routers[2]);
                routers[1].ConnectTo(routers[4]);
                routers[2].ConnectTo(routers[4]);
                routers[3].ConnectTo(routers[4]);

                RouterSerializeService.SerializeRouters(routers);

                foreach (Router router in routers)
                {
                    router.WriteConnectionsToConsole();
                }
            }
            else
            {
                var routers = RouterSerializeService.DeserializeRouters();
                foreach(Router router in routers)
                {
                    router.WriteConnectionsToConsole();
                }
                Console.ReadKey();
            }

        }
    }
}
