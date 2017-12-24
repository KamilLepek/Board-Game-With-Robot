using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.Structure;

namespace BoardGameWithRobot.ImageProcessing
{
    /// <summary>
    /// Main image processing class
    /// </summary>
    internal class Detector
    {
        private readonly CameraService cameraService;

        public Detector(CameraService camera)
        {
            this.cameraService = camera;
        }

        /// <summary>
        /// Checks if game can start
        /// </summary>
        /// <returns>returns true if starting the game is possible</returns>
        public bool InitializeBoard(Board world)
        {
            if (!this.DetectTrackersOnInit(world)) 
            {
                Console.WriteLine("Board initialization failed.");
                return false;
            }
            return true;
        }

        public bool InitializeRobot()
        {
            return true;
        }

        /// <summary>
        /// Detected blue square on the image to determine if board is available(board has blue square tracker)
        /// </summary>
        /// <returns>true if tracker detected</returns>
        public bool DetectTrackersOnInit(Board world)
        {
            for (int i = 0; i < Constants.DetectorFrameSearchRadius; i++)
            {
                this.cameraService.GetCameraFrame();
                this.DetectTrackerInSquare(world, true);
            }
            if (world.TrackersList.Count == Constants.NumberOfTrackers)
                return true;
            Console.WriteLine("{0} trackers detected instead of {1}", world.TrackersList.Count, Constants.NumberOfTrackers);
            return false;
        }

        public bool DetectTrackerInSquare(Board world, bool wholeMap = false)
        {
            if (wholeMap)
            {
                BlueSquareTrackingService.DetectTrackersOnImage(world, this.cameraService.ActualFrame);
                return true;
            }
            else
            {
                if (world.TrackersList.Count(item => item.State == Enums.TrackerDetectionState.Active) >=
                    Constants.MinimumNumberOfActiveTrackers)
                {
                    int i = 0;
                    foreach (var tracker in world.TrackersList)
                    {
                        tracker.SearchForTracker();
#if DEBUG
                        CameraService.ShowMatrix(tracker.Image.Mat, i++.ToString() );
#endif
                    }
                    return true;
                }
                else
                {
                    Console.WriteLine("Tracker lost. Possibility of board/camera movement!");
                    return false;
                }

            }
        }
    }
}
