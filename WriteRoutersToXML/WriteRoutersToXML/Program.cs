using System;
using System.Configuration;
using System.IO;
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
                    routers[i] = new Router("router" + i, i, 5);
                }

                routers[0].ConnectTo(routers[2]);
                routers[0].ConnectTo(routers[3]);
                routers[1].ConnectTo(routers[2]);
                routers[1].ConnectTo(routers[4]);
                routers[2].ConnectTo(routers[4]);
                routers[3].ConnectTo(routers[4]);

                RouterSerializeService.SerializeRouters(routers);
                
                Controller.Instance.InitializeController(routers);
            }
            else
            {
                var routers = RouterSerializeService.DeserializeRouters();

                Controller.Instance.InitializeController(routers);

                var paths = Controller.Instance.GetAllPaths(0, 4);

                HandleUserInput();

                Console.ReadKey();
            }

        }

        static void HandleUserInput()
        {
            bool isContinue = true;
            while (isContinue)
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

                Console.WriteLine("do you want to continue? y/n");

                var userChoice = Console.ReadKey();
                if (userChoice.KeyChar != 'y')
                {
                    isContinue = false;
                }
            }

        }
    }
}
