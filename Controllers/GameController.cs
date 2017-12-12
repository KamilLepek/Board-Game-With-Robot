using System;
using System.Diagnostics;
using BoardGameWithRobot.Utilities;

namespace BoardGameWithRobot.Controllers
{
    internal class GameController
    {
        private readonly CameraService cameraService;

        private readonly Detector detector;

        private readonly Stopwatch timer = new Stopwatch();

        public GameController()
        {
            this.cameraService = new CameraService();
            this.detector = new Detector(this.cameraService);
        }

        /// <summary>
        /// Initializes everything at the beginning
        /// </summary>
        /// <returns>true if every step succeeded</returns>
        public bool InitializeGame()
        {
            if (!this.cameraService.InitializeCamera())
                return false;
            if (!this.detector.InitializeBoard())
                return false;
            return true;
        }

        /// <summary>
        /// Main loop of the game
        /// </summary>
        public void PlayGame()
        {
            while (true)
            {
                this.timer.Restart();
                //check if tracker is there where it should be
                if (!this.detector.DetectTrackerInSquare())
                {
                    // if tracker wasnt spotted in the right place, init everything again
                    if (!this.detector.InitializeBoard())
                    {   
                        Console.WriteLine("Tracker has been lost! Make sure camera points on the game board properly!");
                        return;
                    }
                }

                this.cameraService.GetCameraFrame();
                double elapsed = this.timer.ElapsedMilliseconds;
                Console.WriteLine(elapsed + " ms between frames. " + (int)(1000/elapsed) + " fps.");
                this.cameraService.PrintFrame();
            }
        }
    }
}
