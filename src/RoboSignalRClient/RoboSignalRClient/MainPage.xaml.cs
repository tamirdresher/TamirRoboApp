using CodeValue.IoT.SpheroDemo.Control.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RoboSignalRClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SignalRController _signalController;

        public MainPage()
        {
            this.InitializeComponent();
            _signalController = new SignalRController();
            _signalController.StartListenToHub();

        }
    }


    public class SignalRController
    {
        private static String SIGNALR_HUB_URL = "http://10.0.0.4:1324/";
        private static String SIGNALR_HUB = "SpheroHub";
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;

        public void StartListenToHub()
        {
            var motor = new Motor();
            motor.GpioInit();
            _hubConnection = new HubConnection(SIGNALR_HUB_URL);
            _hubProxy = _hubConnection.CreateHubProxy(SIGNALR_HUB);
            _hubConnection.Start()
                .ContinueWith(t =>
                {
                    Debug.WriteLine("SignalR connection done");
                });
            SynchronizationContext context = SynchronizationContext.Current;
            _hubProxy.On<SpheroNotification>("HandleNotification",
                notification =>
                {
                    //gamma - up/down
                    //beta - right/left
                    var gamma = notification.Command.Gamma;
                    var beta = notification.Command.Beta;

                   // CtrlCmds direction = CtrlCmds.Stop;
                    if (gamma == 0)
                    {
                        if (beta == 0)
                        {
                            //direction = CtrlCmds.Stop;
                            motor.StopPulseLeft();
                            motor.StopPulseRight();

                        }
                        // direction = beta > 0 ? CtrlCmds.Right : CtrlCmds.Left;
                    }
                    else if (gamma > 0)//up
                    {
                        if (beta == 0)
                        {
                            motor.PulseLeft();
                            motor.PulseRight();
                        }
                        //direction = beta > 0 ? CtrlCmds.ForwardRight: CtrlCmds.ForwardLeft;
                        if (beta > 0)
                        {
                            motor.PulseRight(); motor.StopPulseLeft();
                        }
                        else
                        {
                            motor.StopPulseRight(); motor.PulseLeft();

                        }
                    }
                    else if (gamma < 0)//down
                    {

                        //if (beta == 0)
                        //{
                        //    direction = CtrlCmds.Backward;
                        //}
                        //direction = beta > 0 ? CtrlCmds.BackRight : CtrlCmds.BackLeft;
                    }

                    // RobotController.SetRobotDirection(direction, (int)CtrlSpeeds.Max);
                });
        }
    }

    class Motor
    {
        private GpioController _gpioController;
        const int LEFT_PWM_PIN = 5;
        const int RIGHT_PWM_PIN = 6;
        const int SENSOR_PIN = 13;
        const int ACT_LED_PIN = 47; // rpi2-its-pin47, rpi-its-pin16
        private GpioPin _leftPwmPin = null;
        private GpioPin _rightPwmPin = null;
        private GpioPin _sensorPin = null;
        private GpioPin _statusLedPin = null;
        public void GpioInit()
        {
            try
            {
                _gpioController = GpioController.GetDefault();
                if (null != _gpioController)
                {
                    _leftPwmPin = _gpioController.OpenPin(LEFT_PWM_PIN);
                    _leftPwmPin.SetDriveMode(GpioPinDriveMode.Output);

                    _rightPwmPin = _gpioController.OpenPin(RIGHT_PWM_PIN);
                    _rightPwmPin.SetDriveMode(GpioPinDriveMode.Output);
                }
            }
            catch
            {
                Debug.WriteLine("Error init gpio");
            }
        }

        public void PulseLeft()
        {
            _leftPwmPin.Write(GpioPinValue.High);
            System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(20));
            _leftPwmPin.Write(GpioPinValue.Low);

        }

        public void PulseRight()
        {
            for (int i = 0; i < 10; i++)
            {
                _rightPwmPin.Write(GpioPinValue.High);
                System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(1.5));
                _rightPwmPin.Write(GpioPinValue.Low);
                System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(20));
                _rightPwmPin.Write(GpioPinValue.High);
                System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(1.5));
                _rightPwmPin.Write(GpioPinValue.Low);
                System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(20));
                _rightPwmPin.Write(GpioPinValue.High);
                System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(1.5));
                _rightPwmPin.Write(GpioPinValue.Low);
                System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(20));

            }





        }

        public void StopPulseLeft()
        {
            _leftPwmPin.Write(GpioPinValue.Low);
        }

        public void StopPulseRight()
        {
            _rightPwmPin.Write(GpioPinValue.Low);
        }
    }
}
