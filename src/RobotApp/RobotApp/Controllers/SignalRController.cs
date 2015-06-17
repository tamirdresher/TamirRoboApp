using System;
using System.Threading;
using CodeValue.IoT.SpheroDemo.Control.Models;
using Microsoft.AspNet.SignalR.Client;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.Foundation;

namespace RobotApp
{
    public class SignalRController
    {
        private static String SIGNALR_HUB_URL = "http://cvconiotcloudcontrol.azurewebsites.net";
        private static String SIGNALR_HUB = "SpheroHub";
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private IAsyncAction _job;

        public void StartListenToHub()
        {
            var syncCtx=SynchronizationContext.Current;
            bool isReady = true;
            CtrlCmds direction = CtrlCmds.Stop;

            _hubConnection = new HubConnection(SIGNALR_HUB_URL);
            _hubProxy = _hubConnection.CreateHubProxy(SIGNALR_HUB);
            _hubConnection.Start()
                .ContinueWith(t=>{
                    Debug.WriteLine("SignalR connection done {0}",t.Status);
                    if (t.IsFaulted)
                    {
                        Debug.WriteLine("SignalR Error: {0}", t.Exception);

                    }
                });
            SynchronizationContext context = SynchronizationContext.Current;
            _hubProxy.On<SpheroNotification>("HandleNotification",
                notification =>
                {                   
                    //workaround of throttling
                    if (!isReady)
                        return;
                    isReady = false;

                    //gamma - up/down
                    //beta - right/left
                    var gamma = notification.Command.Gamma; ;
                    var beta = notification.Command.Beta;

                    gamma = Math.Sign(gamma) * Math.Min(Math.Abs(gamma), 80);
                    beta = Math.Sign(beta) * Math.Min(Math.Abs(beta), 80);

                    if(Math.Abs(gamma)<10)
                    {
                        gamma = 0;
                    }
                    if (Math.Abs(beta) < 10)
                    {
                        beta = 0;
                    }

                    if (gamma == 0)
                    {
                        direction = beta > 0 ? CtrlCmds.Right : CtrlCmds.Left;

                        if (beta == 0)
                        {
                            direction = CtrlCmds.Stop;
                        }
                    }
                    else if (gamma < 0)//up
                    {
                        direction = beta > 0 ? CtrlCmds.ForwardRight : CtrlCmds.ForwardLeft;

                        if (beta == 0)
                        {
                            direction = CtrlCmds.Forward;
                        }
                       
                    }
                    else if (gamma > 0)//down
                    {
                        direction = beta > 0 ? CtrlCmds.BackRight : CtrlCmds.BackLeft;

                        if (beta == 0)
                        {
                            direction = CtrlCmds.Backward;
                        }
                    }
                    Debug.WriteLine("SignalR: {0} beta:{1} gamma:{2}", direction, beta,gamma);


                    var strength = Math.Max(80,Math.Sqrt(Math.Pow(gamma, 2) + Math.Pow(beta, 2))) / 80;
                    if (direction == CtrlCmds.Stop) strength = 1;

                    RobotDriver.SetRobotDirection(direction,(int)( strength*(int)CtrlSpeeds.Max));
                    isReady = true;
                });

            //_job=ThreadPool.RunAsync((_) =>
            //{
            //    while (direction == CtrlCmds.Stop) { }
            //    RobotDriver.SetRobotDirection(direction, (int)CtrlSpeeds.Min);
            //});
        }
    }
}