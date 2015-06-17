﻿using System;
using System.IO;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.Threading;
using Windows.Devices.Gpio;
using CodeValue.IoT.SpheroDemo.Control.Models;
using Microsoft.AspNet.SignalR.Client;
using System.Threading.Tasks;

namespace RobotApp
{
    public sealed partial class MainPage : Page
    {


        private static String defaultHostName = "";
        public static String serverHostName = defaultHostName; // read from config file
        public static bool isRobot = true; // determined by existence of hostName

        public static Stopwatch stopwatch;
        private SignalRController _signalRController;
        private Task _signalRTask;



        /// <summary>
        /// MainPage initialize all asynchronous functions
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            stopwatch = new Stopwatch();
            stopwatch.Start();

            GetModeAndStartup();
        }

        /// <summary>
        /// Show the current running mode
        /// </summary>
        public void ShowStartupStatus()
        {
            this.CurrentState.Text = "Robot-Kit Sample";
            this.Connection.Text = (isRobot ? ("Robot to " + serverHostName) : "Controller");
        }

        /// <summary>
        /// Switch and store the current running mode in local config file
        /// </summary>
        public async void SwitchRunningMode()
        {
            try
            {
                if (serverHostName.Length > 0) serverHostName = "";
                else serverHostName = defaultHostName;

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile configFile = await storageFolder.CreateFileAsync("config.txt", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(configFile, serverHostName);

                isRobot = serverHostName.Length > 0;
                ShowStartupStatus();
                NetworkController.NetworkInit(serverHostName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SetRunningMode() - " + ex.Message);
            }
        }

        /// <summary>
        /// Read the current running mode (controller host name) from local config file.
        /// Initialize accordingly
        /// </summary>
        public async void GetModeAndStartup()
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile configFile = await storageFolder.GetFileAsync("config.txt");
                String fileContent = await FileIO.ReadTextAsync(configFile);

                serverHostName = fileContent;
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("GetRunningMode() - configuration does not exist yet.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetRunningMode() - " + ex.Message);
            }

            GpioController gpioController = null;
            isRobot = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.Gpio.GpioController");

            if (isRobot)
            {
                try
                {
                    gpioController = GpioController.GetDefault();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            isRobot = gpioController != null;//(serverHostName.Length > 0);
            ShowStartupStatus();

            XBoxController.XboxJoystickInit();
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NetworkController.NetworkInit(serverHostName);
                _signalRController = new SignalRController();
                _signalRController.StartListenToHub();
            }
            if (isRobot)
            {
                MotorCtrl.MotorsInit();
                RobotDriver.SetRobotDirection(CtrlCmds.Stop, (int)CtrlSpeeds.Max);
            }

        }


    }
}
