using System;
using BoardGameWithRobot.ImageProcessing;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Map
{
    internal class BlueSquareTracker : BoardObject
    {
        public int FramesSinceDetected { get; set; }

        private readonly BlueSquareTrackingService blueSquareTrackingService;

        /// <summary>
        /// Part of the camera input on which we search for the tracker to determine whether camera has moved
        /// </summary>
        private Image<Rgb, byte> image;

        public Enums.TrackerDetectionState State { get; set; }

        public BlueSquareTracker(SquareBoundsCurve boundary, BlueSquareTrackingService blueSquareTracking) : base(boundary)
        {
            this.blueSquareTrackingService = blueSquareTracking;
            this.FramesSinceDetected = 0;
            this.State = Enums.TrackerDetectionState.Active;
        }

        /// <summary>
        /// Prints square on tracker on the image
        /// </summary>
        public void PrintTracker(Mat img)
        {
            DrawingService.PutSquareOnImage(img, this.Boundary, true, 1/Math.Sqrt(2));
        }

        public void SetImage(Mat img)
        {
            this.image?.Dispose();
            this.image = SimpleImageProcessingServices.CutFragmentOfImage(img, this.Boundary);
        }

        /// <summary>
        /// Searchers For Tracker on the part of camera input defined at init
        /// </summary>
        public void SearchForTracker()
        {
            VectorOfVectorOfPoint curves = SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.image.Mat);

            for (int i = 0; i < curves.Size; i++)
            {
                SquareBoundsCurve boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));
                // ignore if curve is relatively small or not convex
                if (SimpleImageProcessingServices.IsCurveSmallEnoughToBeIgnored(boundary.Curve) ||
                    !CvInvoke.IsContourConvex(boundary.Curve))
                    continue;
                if (boundary.Curve.Size == 4 
                    && this.blueSquareTrackingService.IsSquareBlue(this.image.Mat, FilteringServices.BGRToHSV(this.image.Mat), boundary))
                    this.UpdateParamsOnDetection();
            }
        }

        public void UpdateParamsOnDetection()
        {
            this.FramesSinceDetected = 0;
            if (this.State == Enums.TrackerDetectionState.Inactive)
                this.State = Enums.TrackerDetectionState.Active;
        }
    }
}
