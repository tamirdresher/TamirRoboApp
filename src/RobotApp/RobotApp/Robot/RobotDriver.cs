using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;

namespace RobotApp
{
    public enum CtrlCmds { Stop, Forward, Backward, Left, Right, ForwardLeft, ForwardRight, BackLeft, BackRight };
    public enum CtrlSpeeds { Min = 0, Mid = 5000, Max = 10000 }
    
    /// <summary>
    /// **** RobotController Class ****
    /// HID Controller devices - XBox controller
    ///   Data transfer helpers: message parsers, direction to motor value translatores, etc.
    /// </summary>
    public class RobotDriver
    {
        public static bool FoundLocalControlsWorking = false;



        public static long msLastDirectionTime;
        public static CtrlCmds lastSetCmd;
        public static void SetRobotDirection(CtrlCmds cmd, int speed)
        {

            switch (cmd)
            {
                case CtrlCmds.Forward: MotorCtrl.waitTimeLeft = MotorCtrl.PulseMs.ms2; MotorCtrl.waitTimeRight = MotorCtrl.PulseMs.ms1; break;
                case CtrlCmds.Backward: MotorCtrl.waitTimeLeft = MotorCtrl.PulseMs.ms1; MotorCtrl.waitTimeRight = MotorCtrl.PulseMs.ms2; break;
                case CtrlCmds.Left: MotorCtrl.waitTimeLeft = MotorCtrl.PulseMs.ms1; MotorCtrl.waitTimeRight = MotorCtrl.PulseMs.ms1; break;
                case CtrlCmds.Right: MotorCtrl.waitTimeLeft = MotorCtrl.PulseMs.ms2; MotorCtrl.waitTimeRight = MotorCtrl.PulseMs.ms2; break;
                case CtrlCmds.ForwardLeft: MotorCtrl.waitTimeLeft = MotorCtrl.PulseMs.stop; MotorCtrl.waitTimeRight = MotorCtrl.PulseMs.ms1; break;
                case CtrlCmds.ForwardRight: MotorCtrl.waitTimeLeft = MotorCtrl.PulseMs.ms2; MotorCtrl.waitTimeRight = MotorCtrl.PulseMs.stop; break;
                case CtrlCmds.BackLeft: MotorCtrl.waitTimeLeft = MotorCtrl.PulseMs.stop; MotorCtrl.waitTimeRight = MotorCtrl.PulseMs.ms2; break;
                case CtrlCmds.BackRight: MotorCtrl.waitTimeLeft = MotorCtrl.PulseMs.ms1; MotorCtrl.waitTimeRight = MotorCtrl.PulseMs.stop; break;
                default:
                case CtrlCmds.Stop: MotorCtrl.waitTimeLeft = MotorCtrl.PulseMs.stop; MotorCtrl.waitTimeRight = MotorCtrl.PulseMs.stop; break;
            }
            if (speed < (int)CtrlSpeeds.Min) speed = (int)CtrlSpeeds.Min;
            if (speed > (int)CtrlSpeeds.Max) speed = (int)CtrlSpeeds.Max;
            MotorCtrl.speedValue = speed;

            dumpOnDiff(cmd.ToString());

            if (!MainPage.isRobot)
            {
                String sendStr = "[" + (Convert.ToInt32(cmd)).ToString() + "]:" + cmd.ToString();
                NetworkController.SendCommandToRobot(sendStr);
            }
            msLastDirectionTime = MainPage.stopwatch.ElapsedMilliseconds;
            lastSetCmd = cmd;

            Debug.WriteLine("SetRobotDirection - done");

        }

        private static MotorCtrl.PulseMs lastWTL, lastWTR;
        private static int lastSpeed;
        static void dumpOnDiff(String title)
        {
            if ((lastWTR == MotorCtrl.waitTimeRight) && (lastWTL == MotorCtrl.waitTimeLeft) && (lastSpeed == MotorCtrl.speedValue)) return;
            Debug.WriteLine("Motors {0}: Left={1}, Right={2}, Speed={3} TaskId:{4}", title, MotorCtrl.waitTimeLeft, MotorCtrl.waitTimeRight, MotorCtrl.speedValue, System.Threading.Tasks.Task.CurrentId);
            lastWTL = MotorCtrl.waitTimeLeft;
            lastWTR = MotorCtrl.waitTimeRight;
            lastSpeed = MotorCtrl.speedValue;
        }

        public static long msLastMessageInTime;
        static bool lastHidCheck = false;
        public static void ParseCtrlMessage(String str)
        {
            char[] delimiterChars = { '[', ']', ':' };
            string[] words = str.Split(delimiterChars);
            if (words.Length >= 2)
            {
                int id = Convert.ToInt32(words[1]);
                if (id >= 0 && id <= 8)
                {
                    CtrlCmds cmd = (CtrlCmds)id;
                    if (FoundLocalControlsWorking)
                    {
                        if (lastHidCheck != FoundLocalControlsWorking) Debug.WriteLine("LOCAL controls found - skipping messages.");
                    }
                    else
                    {
                        if (lastHidCheck != FoundLocalControlsWorking) Debug.WriteLine("No local controls yet - using messages.");
                        SetRobotDirection(cmd, (int)CtrlSpeeds.Max);
                    }
                    lastHidCheck = FoundLocalControlsWorking;
                }
            }
            msLastMessageInTime = MainPage.stopwatch.ElapsedMilliseconds;
        }



    }
}
