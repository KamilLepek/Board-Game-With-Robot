using System;
using System.Drawing;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace BoardGameWithRobot.ImageProcessing
{
    internal static class BlueSquareTrackingService
    {
        
        public static bool DetectTrackersOnImage(Board world, Mat image)
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
            CameraService.ShowMatrix(edgesImage, "huhue");
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
                    UpdateBlueSquareTrackersInfo(world, approxCurve);
#if DEBUG
                    TagBlueSquare(image, approxCurve);
#endif
                }

            }
            return true;
        }

        private static void UpdateBlueSquareTrackersInfo(Board world, VectorOfPoint approxCurve)
        {
            if(!world.LookForTracker(GeometryUtilis.MassCenter(approxCurve)))
                AddTracker(world, approxCurve);
        }

        private static void AddTracker(Board world, VectorOfPoint approxCurve)
        {
            world.TrackersList.Add(new BlueSquareTracker(approxCurve));
        }

        private static void TagBlueSquare(Mat image, VectorOfPoint approxCurve)
        {

            CvInvoke.PutText(image, "k", GeometryUtilis.MassCenter(approxCurve), FontFace.HersheyComplex, 1.0,
                new Bgr(0, 255, 0).MCvScalar);
        }

        private static bool IsSquareBlue(Mat image, Mat HSV, VectorOfPoint approxCurve)
        {
            Point massCenter = GeometryUtilis.MassCenter(approxCurve);
            int radius = (int)(GeometryUtilis.CalculateSquareRadius(approxCurve, massCenter) * Constants.ColorRadiusDetectingFactor);
            if (radius > Constants.BlueTrackerRadiusMargin)
                return false;
            if (massCenter.Y - radius < 0 || massCenter.X - radius < 0 || massCenter.Y + radius > image.Height ||
                massCenter.X + radius > image.Width)
                return false;
            int color = ApproximateColorInSquare(image, HSV, massCenter, radius);
#if DEBUG
            CvInvoke.PutText(image, "  " + color, GeometryUtilis.MassCenter(approxCurve), FontFace.HersheyComplex, 1.0,
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
            return 2 * color / ((2 * radius + 1) * (2 * radius + 1));
        }
    }
}
