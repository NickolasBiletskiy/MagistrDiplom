﻿using Core.Services;
using RoutingApp.Core.Models.NetComponents;
using RoutingApp.Core.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RoutingApp.Core
{
    public class SystemSimulator
    {

        #region Fields
        
        public ILogger LoggerService { get; set; }

        private bool _isPaused;
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
                if (!_isPaused)
                {
                    Update();
                }
            }
        }

        private bool _simulateStep;
        public bool SimulateStep
        {
            get { return _simulateStep; }
            set
            {
                _simulateStep = value;
                if (_simulateStep)
                {
                    Update();
                }
            }
        }

        public List<Router> Routers { get; set; }
        private int _stepCount = 0;

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
            while (!IsPaused || SimulateStep)
            {
                _stepCount++;
                stopwatch = Stopwatch.StartNew();

                if (SimulateStep)
                {
                    LoggerService.Log($"************One-step simulation, step #{_stepCount} ************");
                    _simulateStep = false;
                }
                else
                {
                    LoggerService.Log($"************System simulation, step #{_stepCount} ************");
                }

                bool isSimulationNeeded = false;
                //update code here
                foreach (Router router in Routers)
                {
                    if (!router.CashedPackets.Any()) continue;  //if router has no packets to proceed

                    router.SendPackets();
                    isSimulationNeeded = true;
                    //router.CashedPackets.FirstOrDefault();
                }

                stopwatch.Stop();
                var elapsedMiliseconds = stopwatch.ElapsedMilliseconds;

                LoggerService.Log($"SystemSimulation: elapsedTime = {elapsedMiliseconds}");

                if (!isSimulationNeeded)
                {
                    LoggerService.Log($"No traffic left, simulation stopped");
                    IsPaused = true;
                }

                Thread.Sleep((Constants.UPDATE_TIME > elapsedMiliseconds) ? Constants.UPDATE_TIME - (int)elapsedMiliseconds : 0);
            }
        }

    }
}
