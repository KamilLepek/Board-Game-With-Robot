using System;
using System.Drawing;
using Renci.SshNet;

namespace BoardGameWithRobot.Utilities
{
    internal static class RobotControllingService
    {
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

        public static void RunRemoteControlScript(string action, int duration)
        {
            if (!Ssh.IsConnected)
                throw new Exception("Client not connected");
            Ssh.RunCommand($"python {Constants.RemoteControlScript} {action} {duration}");
        }

        public static void GoForward(int miliseconds)
        {
            RunRemoteControlScript("forward", miliseconds);
        }

        public static void GoBackward(int miliseconds)
        {
            RunRemoteControlScript("backward", miliseconds);
        }

        public static void GoLeft(int miliseconds)
        {
            RunRemoteControlScript("left", miliseconds);
        }

        public static void GoRight(int miliseconds)
        {
            RunRemoteControlScript("right", miliseconds);
        }
    }
}
