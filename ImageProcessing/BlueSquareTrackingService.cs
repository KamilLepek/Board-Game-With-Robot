using System;
using System.Linq;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.Util;

namespace BoardGameWithRobot.ImageProcessing
{
    /// <summary>
    /// Class for detecting blue square trackers
    /// </summary>
    internal class BlueSquareTrackingService
    {

        private readonly CameraService cameraService;

        private readonly Board board;

        public BlueSquareTrackingService(CameraService cam, Board b)
        {
            this.cameraService = cam;
            this.board = b;
        }

        public void DetectBlueSquareTrackersOnInit()
        {
            VectorOfVectorOfPoint curves = SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.cameraService.ActualFrame);
            for (int i = 0; i < curves.Size; i++)
            {
                VectorOfPoint approxCurve = SimpleImageProcessingServices.ApproximateCurve(curves[i]);

                // ignore if curve is relatively small or not convex
                if (SimpleImageProcessingServices.IsCurveSmallEnoughToBeIgnored(approxCurve) ||
                    !CvInvoke.IsContourConvex(approxCurve))
                    continue;

                // Check if it is blue square
                if (approxCurve.Size == 4 &&
                    this.IsSquareBlue(this.cameraService.ActualFrame, FilteringServices.BGRToHSV(this.cameraService.ActualFrame), approxCurve))
                {
                    this.AddTrackerIfNecessary(approxCurve);
                }
            }
        }

        private void AddTrackerIfNecessary(VectorOfPoint approxCurve)
        {
            if(!this.board.LookForTracker(GeometryUtilis.MassCenter(approxCurve)))
                this.board.TrackersList.Add(new BlueSquareTracker(approxCurve, this));
        }

        public bool IsSquareBlue(Mat image, Mat HSV, VectorOfPoint approxCurve)
        {        
            return SimpleImageProcessingServices.IsHueInSquareBetweenConstraints(
                Constants.BlueHueBottomConstraint,
                Constants.BlueHueTopConstraint,
                image,
                HSV,
                approxCurve);
        }

        /// <summary>
        /// Handles detecting blue square trackers on single frame within their image
        /// </summary>
        /// <returns>True if enough trackers have been detected</returns>
        public bool DetectTrackerInSquare()
        {
            if (this.board.TrackersList.Count(item => item.State == Enums.TrackerDetectionState.Active) >=
                Constants.MinimumNumberOfActiveTrackers)
            {
                int i = 0;
                foreach (var tracker in this.board.TrackersList)
                {
                    tracker.SearchForTracker();
#if DEBUG
                    this.cameraService.ShowMatrix(tracker.Image.Mat, i++.ToString());
#endif
                }
                return true;
            }
            Console.WriteLine("Tracker lost. Possibility of board/camera movement!");
            return false;
        }
    }
}
