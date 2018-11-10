using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using WriteRoutersToXML.Models.NetComponents;
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

                Controller.Instance.InitializeController(routers.ToList());
            }
            else
            {
                var routers = RouterSerializeService.DeserializeRouters();

                Controller.Instance.InitializeController(routers.ToList());

                HandleUserInput();
            }

        }

        #region User input

        static void HandleUserInput()
        {
            bool isContinue = true;
            while (isContinue)
            {
                Console.WriteLine("1. Get all paths");
                Console.WriteLine("2. Show connections");
                Console.WriteLine("3. Add router");
                Console.WriteLine("0. Finish program");

                int userChoice;
                while(!int.TryParse(Console.ReadLine(), out userChoice))
                {
                    Console.WriteLine("Please enter integer");
                }

                switch (userChoice)
                {
                    case 1:
                        GetPathsUserInput();
                        break;
                    case 2:
                        Controller.Instance.GetAllConnections();
                        break;
                    case 3:
                        AddRouterAndCreateConnection();
                        break;
                    case 0:
                        isContinue = false;
                        break;
                }
            }

        }

        static void GetPathsUserInput()
        {
            Console.WriteLine("Hello, please, enter router from");
            int routerFrom;
            while (!int.TryParse(Console.ReadLine(), out routerFrom))
            {
                Console.WriteLine("please enter integer");
            };

            int routerTo;
            Console.WriteLine("Please enter router to");
            while (!int.TryParse(Console.ReadLine(), out routerTo))
            {
                Console.WriteLine("please enter integer");
            };

            Controller.Instance.GetAllPaths(routerFrom, routerTo);       

        }

        static void AddRouterAndCreateConnection()
        {
            Console.WriteLine("Create router");

            var router = Controller.Instance.AddNewRouter();

            Console.WriteLine("Router created!. Add connections to this router");

            List<int> routersToConnectIds = new List<int>();
            bool isContinue = true;

            while (isContinue)
            {
                Console.WriteLine("Enter router in the system id to connect");

                int routerId;
                while (!int.TryParse(Console.ReadLine(), out routerId))
                {
                    Console.WriteLine("please enter integer");
                };
                routersToConnectIds.Add(routerId);

                Console.WriteLine("Press 1, if you want to add another connection");
                if (Console.ReadLine() != "1") isContinue = false;
            }

            Controller.Instance.CreateConnectionsForRouter(router, routersToConnectIds.ToArray());

            Controller.Instance.GetAllConnections();
        }

        #endregion
    }
}
