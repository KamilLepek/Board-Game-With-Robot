using System;
using System.Collections.Generic;
using System.Drawing;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace BoardGameWithRobot.ImageProcessing
{
    internal class DiceDetectingService
    {
        private readonly CameraService cameraService;

        private List<Point> pips;

        private SquareBoundsCurve squareBounds;

        public DiceDetectingService(CameraService camera)
        {
            this.cameraService = camera;
            this.IsDiceRegionDefined = false;
            this.pips = new List<Point>();
        }

        public bool IsDiceRegionDefined { get; set; }

        public Image<Rgb, byte> Image { get; set; }

        /// <summary>
        ///     Detects dice
        /// </summary>
        /// <returns> Returns true if dice have been detected on actual frame </returns>
        public bool DetectDice()
        {
            var curves =
                SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.cameraService.ActualFrame);
            for (int i = 0; i < curves.Size; i++)
            {
                var boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));

                // ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        boundary.Curve, Constants.DiceContourSizeTopConstraint,
                        Constants.DiceContourSizeBottomConstraint) ||
                    !CvInvoke.IsContourConvex(boundary.Curve) ||
                    boundary.Radius > Constants.DiceSquareRadiusConstraint ||
                    GeometryUtilis.DistanceBetweenPoints(boundary.MassCenter,
                        new Point(this.cameraService.ActualFrame.Width / 2, this.cameraService.ActualFrame.Height / 2)) > Constants.DistanceFromCenterThatDiceIsSearched) 
                    continue;
                this.DefineDiceRegion(boundary);
                DrawingService.PutSquareOnImage(this.cameraService.ActualFrame, this.squareBounds, true);
#if DEBUG
                DrawingService.PutTextOnImage(this.cameraService.ActualFrame, boundary.MassCenter, Math.Abs(CvInvoke.ContourArea(boundary.Curve)).ToString());
#endif
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Updates image on which we will search for pips
        /// </summary>
        /// <param name="boundary"> Curve that has been detected as dice </param>
        private void DefineDiceRegion(SquareBoundsCurve boundary)
        {
            if (!this.IsDiceRegionDefined ||
                GeometryUtilis.DistanceBetweenPoints(this.squareBounds.MassCenter, boundary.MassCenter) >
                Constants.DistanceFromLastPositionIgnoringMargin)
            {
                this.squareBounds = boundary;
                this.IsDiceRegionDefined = true;
            }
            this.Image?.Dispose();
            this.Image =
                SimpleImageProcessingServices.CutFragmentOfImage(
                    this.cameraService.ActualFrame,
                    this.squareBounds);
        }

        /// <summary>
        ///     Detects pips on defined image. Adds them to pips list if necessary (if they aren't already on the list)
        /// </summary>
        public void DetectRolledNumber()
        {
            this.DefineDiceRegion(this.squareBounds);
            var resized = this.Image.Mat.Clone();
            CvInvoke.Resize(this.Image.Mat, resized, Size.Empty, Constants.DiceResizingFactor,
                Constants.DiceResizingFactor);

            var binary = FilteringServices.BgrToBinary(resized, Constants.DiceBinaryPoint);
            var canniedBinary = binary.Clone();
            CvInvoke.Canny(binary, canniedBinary, Constants.ThresholdLow, Constants.ThresholdHigh, Constants.Aperture);

            var curves = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canniedBinary, curves, new Mat(), RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < curves.Size; i++)
            {
                var boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));

                //ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        boundary.Curve, Constants.DicePipTopSizeConstraint, Constants.DicePipBottomSizeConstraint) ||
                    !CvInvoke.IsContourConvex(boundary.Curve) ||
                    boundary.Radius > Constants.DiceSquareRadiusConstraint)
                    continue;
                this.AddToPipListIfNecessary(boundary.MassCenter);
#if DEBUG
                DrawingService.PutSquareOnImage(resized, boundary);
                DrawingService.PutTextOnImage(resized, boundary.MassCenter, Math.Abs(CvInvoke.ContourArea(boundary.Curve)).ToString());
#endif
            }

#if DEBUG
            //this.cameraService.ShowMatrix(binary, "binary");
            //this.cameraService.ShowMatrix(canniedBinary, "Cannied binary");
            this.cameraService.ShowMatrix(resized, "Pips on dice");
#endif
        }

        private void AddToPipListIfNecessary(Point center)
        {
            foreach (var pip in this.pips)
                if (GeometryUtilis.DistanceBetweenPoints(center, pip) < Constants.IgnoredDistanceBetweenPips)
                    return;
            this.pips.Add(center);
        }

        public int DetermineNumberAndResetPipList()
        {
            int number = this.pips.Count;
            this.pips = new List<Point>();
            return number;
        }
    }
}