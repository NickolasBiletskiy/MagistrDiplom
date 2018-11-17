using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using RoutingApp.Core.Models.NetComponents;
using RoutingApp.Core.Services;

namespace RoutingApp.Core
{
    public class SystemSimulator
    {

        #region Fields

        private bool _isPaused;
        public bool IsPaused { get { return _isPaused; } set {
                _isPaused = value;
                if(!_isPaused)
                {
                    Update();
                }
            } }
        public List<Router> Routers { get; set; }

        #endregion

        #region cstor

        public SystemSimulator(List<Router> routers)
        {
            Routers = routers;
            IsPaused = true;
            //Thread thread = new Thread(new ThreadStart(Update));
            //thread.Start();
            Update();
        }

        #endregion

        public void Update()
        {
            Stopwatch stopwatch;
            while (!IsPaused)
            {
                stopwatch = Stopwatch.StartNew();

                //update code here
                foreach(Router router in Routers)
                {
                    if (!router.CashedPackets.Any()) continue;  //if router has no packets to proceed

                    router.SendPackets();
                    //router.CashedPackets.FirstOrDefault();
                }
                //update code here
                stopwatch.Stop();
                var elapsedMiliseconds = stopwatch.ElapsedMilliseconds;

                LoggerService.Instance.LogToTextBox($"SystemSimulation: elapsedTime = {elapsedMiliseconds}");

                Thread.Sleep((Constants.UPDATE_TIME > elapsedMiliseconds) ? Constants.UPDATE_TIME - (int)elapsedMiliseconds : 0);
            }
        }

    }
}
