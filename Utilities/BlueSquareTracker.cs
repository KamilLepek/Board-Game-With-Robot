using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Utilities
{
    internal static class BlueSquareTracker
    {
        /// <summary>
        /// Determines top left corner of square that we search for this blue square tracker
        /// </summary>
        public static Point Position { get; set; }

        /// <summary>
        /// Length of square side that we search in in pixels
        /// </summary>
        public static int Size { get; set; }

        public static bool DetectTrackerOnImage(Mat image)
        {
            // Create HSV image from BGR source image
            Mat sourceHSV = new Mat();
            CvInvoke.CvtColor(image, sourceHSV, ColorConversion.Bgr2Hsv);

            // Detect edges on image
            Mat edgesImage = FilteringServices.CannyImage(image);
            Mat hierarchy = new Mat();
            VectorOfVectorOfPoint curves = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(edgesImage, curves, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
#if DEBUG
            CameraService.PrintMatrix(edgesImage, "huhue");
#endif
            // Detect tracker inside edges (blue square for now)
            VectorOfPoint approxCurve = new VectorOfPoint();
            for (int i = 0; i < curves.Size; i++)
            {
                CvInvoke.ApproxPolyDP(curves[i], approxCurve, CvInvoke.ArcLength(curves[i], true) * Constants.CurveDetectingFactor, true);
                // ignore if curve is relatively small or not convex
                if (Math.Abs(CvInvoke.ContourArea(curves[i])) < Constants.CurveSizeIgnoringMargin ||
                    !CvInvoke.IsContourConvex(approxCurve))
                    continue;
                if (approxCurve.Size == 4 && IsSquareBlue(image, sourceHSV, approxCurve))
                {
#if DEBUG
                    TagBlueSquare(image, approxCurve);
#endif
                    return true;
                }

            }
            return false;
        }

        private static void TagBlueSquare(Mat image, VectorOfPoint approxCurve)
        {
            CvInvoke.PutText(image, "k", new Point(MassCenter(approxCurve).X, MassCenter(approxCurve).Y), FontFace.HersheyComplex, 1.0,
                new Bgr(0, 255, 0).MCvScalar);
        }

        private static Point MassCenter(VectorOfPoint approxCurve)
        {
            MCvMoments moment = CvInvoke.Moments(approxCurve, false);
            return new Point((int)(moment.M10 / moment.M00), (int)(moment.M01 / moment.M00));
        }

        private static bool IsSquareBlue(Mat image, Mat HSV, VectorOfPoint approxCurve)
        {
            Point massCenter = MassCenter(approxCurve);
            int radius = (int)((approxCurve[0].X - (int)massCenter.X) * Constants.ColorRadiusDetectingFactor);
            int color = ApproximateColorInSquare(image, HSV, massCenter, radius);
#if DEBUG
            CvInvoke.PutText(image, "  " + color.ToString(), new Point(MassCenter(approxCurve).X, MassCenter(approxCurve).Y), FontFace.HersheyComplex, 1.0,
                new Bgr(0, 255, 0).MCvScalar);
#endif
            if (color < Constants.BlueHueTopConstraint && color > Constants.BlueHueBottomConstraint)
                return true;
            return false;
        }

        private static int ApproximateColorInSquare(Mat image, Mat sourceHSV, Point center, int radius)
        {
            Image<Hsv, Byte> img = sourceHSV.ToImage<Hsv, Byte>();
            int color = img.Data[center.Y, center.X, 0];

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    color += img.Data[center.Y + j, center.X + i, 0];
                }
            }
#if DEBUG
            CvInvoke.Rectangle(
                image,
                new Rectangle(center.X - radius, center.Y - radius, 2 * radius + 1, 2 * radius + 1),
                new Bgr(0, 255, 0).MCvScalar,
                1,
                LineType.EightConnected,
                0);
#endif
            return 2 * color / ((2 * radius + 1) * (2 * radius + 1));
        }
    }
}
