using System;
using System.Linq;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot.ImageProcessing
{
    /// <summary>
    ///     Class for detecting blue square trackers
    /// </summary>
    internal class BlueSquareTrackingService
    {
        private readonly Board board;

        private readonly CameraService cameraService;

        public BlueSquareTrackingService(CameraService cam, Board b)
        {
            this.cameraService = cam;
            this.board = b;
        }

        public void DetectBlueSquareTrackersOnInit()
        {
            var curves = SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.cameraService.ActualFrame);
            for (int i = 0; i < curves.Size; i++)
            {
                var boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));

                // ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        boundary.Curve,
                        Constants.TrackerCurveSizeIgnoringMargin,
                        Constants.DefaultCurveSizeIgnoringMargin) ||
                    !CvInvoke.IsContourConvex(boundary.Curve))
                    continue;

                // Check if it is blue square
                if (SimpleImageProcessingServices.IsSquare(this.cameraService.ActualFrame, boundary) &&
                    this.IsSquareBlue(this.cameraService.ActualFrame,
                        FilteringServices.BGRToHSV(this.cameraService.ActualFrame), boundary))
                    this.AddTrackerIfNecessary(boundary);
            }
        }

        private void AddTrackerIfNecessary(SquareBoundsCurve boundary)
        {
            if (!this.board.LookForTracker(boundary.MassCenter))
                this.board.TrackersList.Add(new BlueSquareTracker(boundary, this));
        }

        public bool IsSquareBlue(Mat image, Mat HSV, SquareBoundsCurve boundary)
        {
            return SimpleImageProcessingServices.IsHueInSquareBetweenConstraints(
                Constants.BlueHueBottomConstraint,
                Constants.BlueHueTopConstraint,
                image,
                HSV,
                boundary);
        }

        /// <summary>
        ///     Handles detecting blue square trackers on single frame within their image
        /// </summary>
        /// <returns>True if enough trackers have been detected</returns>
        public bool DetectTrackersInTheirSquares()
        {
            if (this.board.TrackersList.Count(item => item.State == Enums.TrackerDetectionState.Active) >=
                Constants.MinimumNumberOfActiveTrackers)
            {
                foreach (var tracker in this.board.TrackersList)
                    tracker.SearchForTracker();
                return true;
            }
            Console.WriteLine("Tracker lost. Possibility of board/camera movement!");
            return false;
        }
    }
}