using System;
using System.Windows.Forms;
using BoardGameWithRobot.Controllers;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot
{
    public class Game
    {
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            var controller = new GameController();

            if (controller.InitializeGame())
            {
                controller.PlayGame();
                MessageBox.Show("Game has finished!");
                return;
            }
            MessageBox.Show("Game initialization failed");
            CvInvoke.WaitKey();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnecting Ssh");
            RobotControllingService.Cleanup();
            RobotControllingService.Ssh.Disconnect();
            RobotControllingService.Ssh.Dispose();
        }
    }
}