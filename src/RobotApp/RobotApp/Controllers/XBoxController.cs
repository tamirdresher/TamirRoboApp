using System;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;

namespace RobotApp
{
    public class XBoxController
    {
        private static XboxHidController controller;
        private static int lastControllerCount = 0;
        public static async void XboxJoystickInit()
        {
            string deviceSelector = HidDevice.GetDeviceSelector(0x01, 0x05);
            DeviceInformationCollection deviceInformationCollection = await DeviceInformation.FindAllAsync(deviceSelector);

            if (deviceInformationCollection.Count == 0)
            {
                Debug.WriteLine("No Xbox360 controller found!");
            }
            lastControllerCount = deviceInformationCollection.Count;

            foreach (DeviceInformation d in deviceInformationCollection)
            {
                Debug.WriteLine("Device ID: " + d.Id);

                HidDevice hidDevice = await HidDevice.FromIdAsync(d.Id, Windows.Storage.FileAccessMode.Read);

                if (hidDevice == null)
                {
                    try
                    {
                        var deviceAccessStatus = DeviceAccessInformation.CreateFromId(d.Id).CurrentStatus;

                        if (!deviceAccessStatus.Equals(DeviceAccessStatus.Allowed))
                        {
                            Debug.WriteLine("DeviceAccess: " + deviceAccessStatus.ToString());
                            RobotDriver.FoundLocalControlsWorking = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Xbox init - " + e.Message);
                    }

                    Debug.WriteLine("Failed to connect to the controller!");
                }

                controller = new XboxHidController(hidDevice);
                controller.DirectionChanged += Controller_DirectionChanged;
            }
        }



        public static async void XboxJoystickCheck()
        {
            string deviceSelector = HidDevice.GetDeviceSelector(0x01, 0x05);
            DeviceInformationCollection deviceInformationCollection = await DeviceInformation.FindAllAsync(deviceSelector);
            if (deviceInformationCollection.Count != lastControllerCount)
            {
                lastControllerCount = deviceInformationCollection.Count;
                XboxJoystickInit();
            }
        }

        private static void Controller_DirectionChanged(ControllerVector sender)
        {
            RobotDriver.FoundLocalControlsWorking = true;
            Debug.WriteLine("XBoxController - Direction: " + sender.Direction + ", Magnitude: " + sender.Magnitude);
            XBoxToRobotDirection((sender.Magnitude < 2500) ? ControllerDirection.None : sender.Direction, sender.Magnitude);

            MotorCtrl.speedValue = sender.Magnitude;
        }

        static void XBoxToRobotDirection(ControllerDirection dir, int magnitude)
        {
            switch (dir)
            {
                case ControllerDirection.Down:      RobotDriver.SetRobotDirection(CtrlCmds.Backward, magnitude); break;
                case ControllerDirection.Up:        RobotDriver.SetRobotDirection(CtrlCmds.Forward, magnitude); break;
                case ControllerDirection.Left:      RobotDriver.SetRobotDirection(CtrlCmds.Left, magnitude); break;
                case ControllerDirection.Right:     RobotDriver.SetRobotDirection(CtrlCmds.Right, magnitude); break;
                case ControllerDirection.DownLeft:  RobotDriver.SetRobotDirection(CtrlCmds.BackLeft, magnitude); break;
                case ControllerDirection.DownRight: RobotDriver.SetRobotDirection(CtrlCmds.BackRight, magnitude); break;
                case ControllerDirection.UpLeft:    RobotDriver.SetRobotDirection(CtrlCmds.ForwardLeft, magnitude); break;
                case ControllerDirection.UpRight:   RobotDriver.SetRobotDirection(CtrlCmds.ForwardRight, magnitude); break;
                default:                            RobotDriver.SetRobotDirection(CtrlCmds.Stop, (int)CtrlSpeeds.Max); break;
            }
        }
    }
}