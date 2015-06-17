using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using RobotApp;

namespace RobotApp
{
    /// <summary>
    /// **** MainPage class - controller input ****
    ///   Things in the MainPage class handle the App level startup, and App XAML level Directional inputs to the robot.
    ///   XAML sourced input controls, include screen buttons, and keyboard input
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region ----- on-screen click/touch controls -----
        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            TouchDir(CtrlCmds.Forward);
        }
        private void Left_Click(object sender, RoutedEventArgs e)
        {
            TouchDir(CtrlCmds.Left);
        }
        private void Right_Click(object sender, RoutedEventArgs e)
        {
            TouchDir(CtrlCmds.Right);
        }
        private void Backward_Click(object sender, RoutedEventArgs e)
        {
            TouchDir(CtrlCmds.Backward);
        }
        private void ForwardLeft_Click(object sender, RoutedEventArgs e)
        {
            TouchDir(CtrlCmds.ForwardLeft);
        }
        private void ForwardRight_Click(object sender, RoutedEventArgs e)
        {
            TouchDir(CtrlCmds.ForwardRight);
        }
        private void BackwardLeft_Click(object sender, RoutedEventArgs e)
        {
            TouchDir(CtrlCmds.BackLeft);
        }
        private void BackwardRight_Click(object sender, RoutedEventArgs e)
        {
            TouchDir(CtrlCmds.BackRight);
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            TouchDir(CtrlCmds.Stop);
        }
        private void Status_Click(object sender, RoutedEventArgs e)
        {
            // just update the display, without affecting direction of robot.  useful for diagnosting state
            UpdateClickStatus();
        }
        private void SwitchMode_Click(object sender, RoutedEventArgs e)
        {
            SwitchRunningMode();
        }
        private void TouchDir(CtrlCmds dir)
        {
            RobotDriver.FoundLocalControlsWorking = true;
            RobotDriver.SetRobotDirection(dir, (int)CtrlSpeeds.Max);
            UpdateClickStatus();
        }

        /// <summary>
        /// Virtual Key input handlers.  Keys directed here from XAML settings in MainPage.XAML
        /// </summary>
        private void Background_KeyDown_1(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            Debug.WriteLine("KeyDn: \"" + e.Key.ToString() + "\"");
            VKeyToRobotDirection(e.Key);
            UpdateClickStatus();
        }
        private void Background_KeyUp_1(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            VKeyToRobotDirection(Windows.System.VirtualKey.Enter);
            UpdateClickStatus();
        }
        static void VKeyToRobotDirection(Windows.System.VirtualKey vkey)
        {
            switch (vkey)
            {
                case Windows.System.VirtualKey.Down: RobotDriver.SetRobotDirection(CtrlCmds.Backward, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.Up: RobotDriver.SetRobotDirection(CtrlCmds.Forward, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.Left: RobotDriver.SetRobotDirection(CtrlCmds.Left, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.Right: RobotDriver.SetRobotDirection(CtrlCmds.Right, (int)CtrlSpeeds.Max); break;

                case Windows.System.VirtualKey.X: RobotDriver.SetRobotDirection(CtrlCmds.Backward, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.W: RobotDriver.SetRobotDirection(CtrlCmds.Forward, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.A: RobotDriver.SetRobotDirection(CtrlCmds.Left, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.D: RobotDriver.SetRobotDirection(CtrlCmds.Right, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.Z: RobotDriver.SetRobotDirection(CtrlCmds.BackLeft, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.C: RobotDriver.SetRobotDirection(CtrlCmds.BackRight, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.Q: RobotDriver.SetRobotDirection(CtrlCmds.ForwardLeft, (int)CtrlSpeeds.Max); break;
                case Windows.System.VirtualKey.E: RobotDriver.SetRobotDirection(CtrlCmds.ForwardRight, (int)CtrlSpeeds.Max); break;

                case Windows.System.VirtualKey.Enter:
                default: RobotDriver.SetRobotDirection(CtrlCmds.Stop, (int)CtrlSpeeds.Max); break;
            }
            RobotDriver.FoundLocalControlsWorking = true;
        }

        /// <summary>
        /// UpdateClickStatus() - fill in Connection status, and current direction State on screen after each button touch/click
        /// </summary>
        private void UpdateClickStatus()
        {
            this.CurrentState.Text = RobotDriver.lastSetCmd.ToString();
            if (MainPage.isRobot)
            {
                this.Connection.Text = "Robot mode";
            }
            else
            {
                if ((stopwatch.ElapsedMilliseconds - NetworkController.msLastSendTime) > 6000)
                {
                    this.Connection.Text = "NOT SENDING";
                }
                else
                {
                    this.Connection.Text = "OK";
                }
            }
        }
        #endregion
    }
}