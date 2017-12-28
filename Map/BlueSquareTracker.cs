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
        public Image<Rgb, byte> Image { get; set; }

        public Enums.TrackerDetectionState State { get; set; }

        public BlueSquareTracker(VectorOfPoint curve, BlueSquareTrackingService blueSquareTracking) : base(curve)
        {
            this.blueSquareTrackingService = blueSquareTracking;
            this.FramesSinceDetected = 0;
            this.State = Enums.TrackerDetectionState.Active;
        }

        /// <summary>
        /// Prints square on tracker on the image
        /// </summary>
        public void PrintTracker(Mat image)
        {
            int manhattanRadius = (int)(this.Radius / Math.Sqrt(2));
            DrawingService.PutSquareOnBoard(image, this.Center, manhattanRadius, true);
        }

        public void SetImage(Mat image)
        {
            this.Image?.Dispose();
            this.Image = SimpleImageProcessingServices.CutFragmentOfImage(image, this.Center, this.Radius);
        }

        /// <summary>
        /// Searchers For Tracker on the part of camera input defined at init
        /// </summary>
        public void SearchForTracker()
        {
            VectorOfVectorOfPoint curves = SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.Image.Mat);

            for (int i = 0; i < curves.Size; i++)
            {
                VectorOfPoint approxCurve = SimpleImageProcessingServices.ApproximateCurve(curves[i]);
                // ignore if curve is relatively small or not convex
                if (SimpleImageProcessingServices.IsCurveSmallEnoughToBeIgnored(approxCurve) ||
                    !CvInvoke.IsContourConvex(approxCurve))
                    continue;
                if (approxCurve.Size == 4 
                    && this.blueSquareTrackingService.IsSquareBlue(this.Image.Mat, FilteringServices.BGRToHSV(this.Image.Mat), approxCurve))
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
