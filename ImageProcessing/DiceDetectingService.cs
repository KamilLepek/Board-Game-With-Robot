using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        private Point massCenter;

        private int radius;

        public bool IsDiceRegionDefined { get; set; }

        public Image<Rgb, byte> Image { get; set; }

        private List<Point> pips;

        private int rollingFramesCounter = 0;

        public DiceDetectingService(CameraService camera)
        {
            this.cameraService = camera;
            this.IsDiceRegionDefined = false;
            this.pips = new List<Point>();
        }

        public bool DetectDice()
        {
            VectorOfVectorOfPoint curves =
                SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.cameraService.ActualFrame);
            for (int i = 0; i < curves.Size; i++)
            {
                VectorOfPoint approxCurve = SimpleImageProcessingServices.ApproximateCurve(curves[i]);

                Point massCntr = GeometryUtilis.MassCenter(approxCurve);
                int r = GeometryUtilis.CalculateSquareRadius(approxCurve, massCntr);

                // ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                    approxCurve, Constants.DiceContourSizeTopConstraint, Constants.DiceContourSizeBottomConstraint) ||
                    !CvInvoke.IsContourConvex(approxCurve) ||
                    r > Constants.DiceSquareRadiusConstraint)
                    continue;
                this.DefineDiceRegion(massCntr, r);
#if DEBUG
                DrawingService.PutSquareOnBoard(this.cameraService.ActualFrame, this.massCenter, this.radius);
#endif
                this.DetectRolledNumber();
                return true;
            }
            return false;
        }

        private void DefineDiceRegion(Point center, int r)
        {
            if (!this.IsDiceRegionDefined || GeometryUtilis.DistanceBetweenPoints(this.massCenter, center) > 5)
            {
                this.massCenter = center;
                this.radius = r;
                this.IsDiceRegionDefined = true;
            }
            this.Image?.Dispose();
            this.Image =
                SimpleImageProcessingServices.CutFragmentOfImage(
                    this.cameraService.ActualFrame, 
                    this.massCenter, 
                    this.radius);
        }

        private void DetectRolledNumber()
        {
            Mat resized = this.Image.Mat.Clone();
            CvInvoke.Resize(this.Image.Mat, resized, Size.Empty, Constants.DiceResizingFactor, Constants.DiceResizingFactor);

            Mat binary = FilteringServices.BgrToBinary(resized, Constants.DiceBinaryPoint);
            Mat canniedBinary = binary.Clone();
            CvInvoke.Canny(binary, canniedBinary, Constants.ThresholdLow, Constants.ThresholdHigh, Constants.Aperture);
            
            VectorOfVectorOfPoint curves = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canniedBinary, curves, new Mat(), RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            this.rollingFramesCounter++;

            for (int i = 0; i < curves.Size; i++)
            {
                VectorOfPoint approxCurve = SimpleImageProcessingServices.ApproximateCurve(curves[i]);

                Point massCntr = GeometryUtilis.MassCenter(approxCurve);
                int r = GeometryUtilis.CalculateSquareRadius(approxCurve, massCntr);

                //ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        approxCurve, Constants.DicePipTopSizeConstraint, Constants.DicePipBottomSizeConstraint) ||
                    !CvInvoke.IsContourConvex(approxCurve) ||
                    r > Constants.DiceSquareRadiusConstraint)
                    continue;
                this.AddToPipListIfNecessary(massCntr);
#if DEBUG
                DrawingService.PutSquareOnBoard(resized, massCntr, r);
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
                {
                    if (GeometryUtilis.DistanceBetweenPoints(center, pip) < Constants.IgnoredDistanceBetweenPips)
                        return;
                }
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
