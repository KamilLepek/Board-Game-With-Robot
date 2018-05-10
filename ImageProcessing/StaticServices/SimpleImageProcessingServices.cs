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
    internal static class SimpleImageProcessingServices
    {
        public static VectorOfVectorOfPoint DetectEdgesAsCurvesOnImage(Mat image, Mat hierarchy = null)
        {
            if (hierarchy == null)
                hierarchy = new Mat();
            var edgesImage = FilteringServices.CannyImage(image);
            var curves = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(edgesImage, curves, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            return curves;
        }

        public static VectorOfPoint ApproximateCurve(VectorOfPoint curve)
        {
            var approxCurve = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(
                curve,
                approxCurve,
                CvInvoke.ArcLength(curve, true) * Constants.CurveDetectingFactor,
                true);
            return approxCurve;
        }

        public static bool IsCurveSizeBetweenMargins(VectorOfPoint curve,
            int topMargin = Constants.DefaultCurveSizeIgnoringMargin, int bottomMargin = 0)
        {
            double area = Math.Abs(CvInvoke.ContourArea(curve));
            if (area < topMargin && area > bottomMargin)
                return true;
            return false;
        }

        public static bool IsCurveSmallEnoughToBeIgnored(VectorOfPoint curve,
            int topMargin = Constants.DefaultCurveSizeIgnoringMargin)
        {
            return IsCurveSizeBetweenMargins(curve, topMargin);
        }

        /// <summary>
        ///     Approximates hue in given square on HSV image
        /// </summary>
        /// <param name="sourceHSV">HSV image</param>
        /// <param name="center">Center on given square</param>
        /// <param name="radius">Radius of the square (manhattan metric radius)</param>
        /// <returns>Value of Hue inside given square</returns>
        public static int ApproximateColorInSquare(Mat sourceHSV, Point center, int radius)
        {
            var img = sourceHSV.ToImage<Hsv, byte>();
            int color = img.Data[center.Y, center.X, 0];

            for (int i = -radius; i <= radius; i++)
            for (int j = -radius; j <= radius; j++)
                color += img.Data[center.Y + j, center.X + i, 0];
            // 2* because EMGU CV keeps hue value in [0,180]
            return 2 * color / ((2 * radius + 1) * (2 * radius + 1));
        }

        public static bool IsSquare(Mat image, SquareBoundsCurve boundary)
        {
            if (boundary.Radius > Constants.SquareRadiusMargin)
                return false;
            if (boundary.MassCenter.Y - boundary.Radius < 0 || boundary.MassCenter.X - boundary.Radius < 0 ||
                boundary.MassCenter.Y + boundary.Radius > image.Height ||
                boundary.MassCenter.X + boundary.Radius > image.Width)
                return false;
            if (boundary.Curve.Size == 4)
                return true;
            return false;
        }

        /// <summary>
        ///     Determines whether hue in given square is between given constraints.
        ///     Coinstraints can either determine [bottom,top] or [0,top]u[bottom,360] intervals
        /// </summary>
        /// <param name="bottomConstraint">Constraint</param>
        /// <param name="topConstraint">Constraint</param>
        /// <param name="image">Original image</param>
        /// <param name="HSV">HSV image</param>
        /// <param name="boundary">given square</param>
        /// <param name="printColorOnCurve">Determines whether hue value is printed on the curve</param>
        /// <returns>True if is between, false otherwise</returns>
        public static bool IsHueInSquareBetweenConstraints(
            int bottomConstraint,
            int topConstraint,
            Mat image,
            Mat HSV,
            SquareBoundsCurve boundary,
            bool printColorOnCurve = true)
        {
            int searchingRadius = (int) (boundary.Radius * Constants.ColorRadiusDetectingFactor);
            int color = ApproximateColorInSquare(HSV, boundary.MassCenter, searchingRadius);
#if DEBUG
            if(printColorOnCurve)
                DrawingService.PutTextOnImage(image, boundary.MassCenter, color.ToString());
#endif
            if (topConstraint > bottomConstraint)
            {
                if (color < topConstraint && color > bottomConstraint)
                    return true;
            }
            else
            {
                if (color > bottomConstraint || color < topConstraint)
                    return true;
            }
            return false;
        }

        /// <summary>
        ///     Cut Fragment of given image
        /// </summary>
        /// <param name="image">given image</param>
        /// <param name="boundary">Region of interest</param>
        /// <returns>fragment of given image as Image</returns>
        public static Image<Rgb, byte> CutFragmentOfImage(Mat image, SquareBoundsCurve boundary)
        {
            var temp = image.Clone();
            var regionOfInterest = new Rectangle(boundary.MassCenter.X - boundary.Radius,
                boundary.MassCenter.Y - boundary.Radius, 2 * boundary.Radius, 2 * boundary.Radius);
            var img = temp.ToImage<Rgb, byte>();
            img.ROI = regionOfInterest;
            return img;
        }
    }
}