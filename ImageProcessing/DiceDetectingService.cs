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

        private int rollingFramesCounter;

        private SquareBoundsCurve squareBounds;

        public DiceDetectingService(CameraService camera)
        {
            this.cameraService = camera;
            this.IsDiceRegionDefined = false;
            this.pips = new List<Point>();
        }

        public bool IsDiceRegionDefined { get; set; }

        public Image<Rgb, byte> Image { get; set; }

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
                    boundary.Radius > Constants.DiceSquareRadiusConstraint)
                    continue;
                this.DefineDiceRegion(boundary);
                DrawingService.PutSquareOnBoard(this.cameraService.ActualFrame, this.squareBounds, true);
                this.DetectRolledNumber();
                return true;
            }
            return false;
        }

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

        private void DetectRolledNumber()
        {
            var resized = this.Image.Mat.Clone();
            CvInvoke.Resize(this.Image.Mat, resized, Size.Empty, Constants.DiceResizingFactor,
                Constants.DiceResizingFactor);

            var binary = FilteringServices.BgrToBinary(resized, Constants.DiceBinaryPoint);
            var canniedBinary = binary.Clone();
            CvInvoke.Canny(binary, canniedBinary, Constants.ThresholdLow, Constants.ThresholdHigh, Constants.Aperture);

            var curves = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canniedBinary, curves, new Mat(), RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            this.rollingFramesCounter++;

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
                DrawingService.PutSquareOnBoard(resized, boundary);
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
            if (this.rollingFramesCounter > Constants.DiceRollingPipsMargin)
            {
                foreach (var pip in this.pips)
                    if (GeometryUtilis.DistanceBetweenPoints(center, pip) < Constants.IgnoredDistanceBetweenPips)
                        return;
                this.pips.Add(center);
            }
        }

        public int DetermineNumberAndResetPipList()
        {
            this.rollingFramesCounter = 0;
            int number = this.pips.Count;
            this.pips = new List<Point>();
            return number;
        }
    }
}