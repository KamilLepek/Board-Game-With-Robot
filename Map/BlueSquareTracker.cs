using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using BoardGameWithRobot.ImageProcessing;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Map
{
    internal class BlueSquareTracker : BoardObject
    {
        /// <summary>
        /// Determines center of square that we search for this blue square tracker
        /// </summary>
        public Point Center { get; set; }

        /// <summary>
        /// Half of the diagonal of the square
        /// </summary>
        public int Radius { get; set; }

        public int FramesSinceDetected { get; set; }

        public Image<Rgb, byte> Image { get; set; }

        public Enums.TrackerDetectionState State { get; set; }

        public BlueSquareTracker(VectorOfPoint curve)
        {
            this.Center = GeometryUtilis.MassCenter(curve);
            this.Radius = GeometryUtilis.CalculateSquareRadius(curve, this.Center);
            this.FramesSinceDetected = 0;
            this.State = Enums.TrackerDetectionState.Active;
        }

        public void PrintTracker(Mat image, bool rectangle = true)
        {
            if (rectangle)
            {
                int radius = (int) (this.Radius / Math.Sqrt(2));
                CvInvoke.Rectangle(
                    image,
                    new Rectangle(this.Center.X - radius, this.Center.Y - radius, 2 * radius + 1, 2 * radius + 1),
                    new Bgr(0, 255, 0).MCvScalar,
                    1,
                    LineType.EightConnected,
                    0);
            }
        }

        public void SetImage(Mat image)
        {
            this.Image?.Dispose();
            Mat temp = image.Clone();
            var regionOfInterest = new Rectangle(this.Center.X - this.Radius,
                this.Center.Y - this.Radius, 2 * this.Radius, 2 * this.Radius);
            Image<Rgb, Byte> img = temp.ToImage<Rgb, Byte>();
            img.ROI = regionOfInterest;
            this.Image = img;
        }

        public void SearchForTracker()
        {
            // Create HSV image from BGR source image
            Mat sourceHSV = new Mat();
            CvInvoke.CvtColor(this.Image.Mat, sourceHSV, ColorConversion.Bgr2Hsv);

            // Detect edges on image
            Mat edgesImage = FilteringServices.CannyImage(this.Image.Mat);
            Mat hierarchy = new Mat();
            VectorOfVectorOfPoint curves = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(edgesImage, curves, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            // Detect tracker inside edges (blue square for now)
            VectorOfPoint approxCurve = new VectorOfPoint();
            for (int i = 0; i < curves.Size; i++)
            {
                CvInvoke.ApproxPolyDP(curves[i], approxCurve, CvInvoke.ArcLength(curves[i], true) * Constants.CurveDetectingFactor, true);
                // ignore if curve is relatively small or not convex
                if (Math.Abs(CvInvoke.ContourArea(curves[i])) < Constants.CurveSizeIgnoringMargin ||
                    !CvInvoke.IsContourConvex(approxCurve))
                    continue;
                if (approxCurve.Size == 4 && BlueSquareTrackingService.IsSquareBlue(this.Image.Mat, sourceHSV, approxCurve))
                {
                    BlueSquareTrackingService.TagBlueSquare(this.Image.Mat, approxCurve);
                    this.UpdateParamsOnDetection();
                }
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
