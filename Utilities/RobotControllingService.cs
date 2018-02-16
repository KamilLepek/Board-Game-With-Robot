using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Renci.SshNet;

namespace BoardGameWithRobot.Utilities
{
    internal static class RobotControllingService
    {
        public static Enums.RobotControllingState State = Enums.RobotControllingState.Wait;

        public static bool RobotTurn = false; //TODO: pretty redundant

        public static double Angle;

        public static SshClient Ssh { get; private set; }

        public static Point TrackStartPoint { get; set; }

        public static Point TrackEndPoint { get; set; }

        public static Point Vector => new Point(TrackEndPoint.X - TrackStartPoint.X, TrackEndPoint.Y - TrackStartPoint.Y);

        public static void InitializeSshConnectionParams()
        {
            Ssh = new SshClient(
                Constants.RobotAdress,
                Constants.RobotPort,
                Constants.RobotLogin,
                Constants.Password);
        }

        private static SshCommand RunRemoteControlScript(string action, int duration)
        {
            if (!Ssh.IsConnected)
                throw new Exception("Client not connected");
            return Ssh.CreateCommand($"python {Constants.RemoteControlScript} {action} {duration}");
        }

        public static SshCommand GoForward(int miliseconds)
        {
            return RunRemoteControlScript("forward", miliseconds);
        }

        public static SshCommand GoBackward(int miliseconds)
        {
            return RunRemoteControlScript("backward", miliseconds);
        }

        public static SshCommand GoLeft(int miliseconds)
        {
            return RunRemoteControlScript("left", miliseconds);
        }

        public static SshCommand GoRight(int miliseconds)
        {
            return RunRemoteControlScript("right", miliseconds);
        }

        public static void Cleanup()
        {
            RunRemoteControlScript("cleanup", 0).Execute();
        }

        public static void TryToPushForward()
        {
            Stopwatch st = new Stopwatch();
            while (State != Enums.RobotControllingState.Finished)
            {
                switch (State)
                {
                    case Enums.RobotControllingState.Forward:
                        st.Restart();
                        GoForward(90).Execute();
                        Thread.Sleep(10);
                        GoForward(90).Execute();
                        State = Enums.RobotControllingState.Backward;
                        break;
                    case Enums.RobotControllingState.Backward:
                        GoBackward(100).Execute();
                        State = Enums.RobotControllingState.Correct;
                        break;
                    case Enums.RobotControllingState.Correct:
                        if (Angle < -1.25)
                            GoRight(8 * (int)Math.Abs(Angle)).Execute();
                        else if (Angle > 1.25)
                            GoLeft(8 * (int)Math.Abs(Angle)).Execute();
                        State = Enums.RobotControllingState.Wait;
                        break;
                    case Enums.RobotControllingState.Wait:
                        if (st.Elapsed.Seconds > 5 && RobotTurn) // when we are too close to the pawn for too long
                        {
                            GoForward(130).Execute();
                            GoBackward(100).Execute();
                            st.Restart();
                        }
                        else if (!RobotTurn)
                            st.Restart();
                        Thread.Sleep(1000);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                Console.WriteLine($"Angle: {Angle}");
            }
        }
    }
}
