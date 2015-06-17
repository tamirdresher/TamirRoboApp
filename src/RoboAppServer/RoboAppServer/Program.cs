using CodeValue.IoT.SpheroDemo.Control.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboAppServer
{
    class Program
    {
        private static String SIGNALR_HUB_URL = "http://10.0.0.4:1324/";
        private const String SIGNALR_HUB = "SpheroHub";
        const int FORWARD = 1;
        const int BACKWARD = -1;
        const int NO_DIR = 0;
        const int LEFT = -1;
        const int RIGHT = 1;



        static void Main(string[] args)
        {
            string url = SIGNALR_HUB_URL;
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
                var gamma = FORWARD;
                var beta = NO_DIR;
                while (true)
                {
                    Console.WriteLine("L R F B S");
                    var cmd = Console.ReadLine();
                    var notification = new SpheroNotification();
                    switch (cmd)
                    {
                        case "S":
                            beta = NO_DIR;
                            gamma = NO_DIR;
                            break;
                        case "L":
                            beta = LEFT;
                            break;
                        case "R":
                            beta = RIGHT;
                            break;
                        case "F":
                            gamma = FORWARD;
                            beta = NO_DIR;

                            break;
                        case "B":
                            gamma = BACKWARD;
                            beta = NO_DIR;
                            break;
                    }

                    GlobalHost.ConnectionManager.GetHubContext<SpheroHub>().Clients.All.HandleNotification(new SpheroNotification()
                    {
                        Command = new SpheroCommand()
                        {
                            Beta = beta,
                            Gamma = gamma
                        }
                    });

                }
            }
        }

        [HubName(SIGNALR_HUB)]
        public class SpheroHub : Hub
        {
            public override Task OnConnected()
            {
                return base.OnConnected();
            }
            public void Send(string name, string message)
            {
                Clients.All.addMessage(name, message);
            }
        }

        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseCors(CorsOptions.AllowAll);
                app.MapSignalR();
              
            }
        }
    }
}
