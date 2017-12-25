﻿using System;
using System.Drawing;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace BoardGameWithRobot.ImageProcessing
{
    internal class SimpleImageProcessingServices
    {
        public static VectorOfVectorOfPoint DetectEdgesAsCurvesOnImage(Mat image, Mat hierarchy = null)
        {
            if (hierarchy == null)
                hierarchy = new Mat();
            Mat edgesImage = FilteringServices.CannyImage(image);
            VectorOfVectorOfPoint curves = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(edgesImage, curves, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            return curves;
        }

        public static VectorOfPoint ApproximateCurve(VectorOfPoint curve)
        {
            VectorOfPoint approxCurve = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(
                curve,
                approxCurve,
                CvInvoke.ArcLength(curve, true) * Constants.CurveDetectingFactor,
                true);
            return approxCurve;
        }

        public static bool IsCurveSmallEnoughToBeIgnored(VectorOfPoint curve)
        {
            if (Math.Abs(CvInvoke.ContourArea(curve)) < Constants.CurveSizeIgnoringMargin)
                return true;
            return false;
        }

        /// <summary>
        /// Approximates hue in given square on HSV image
        /// </summary>
        /// <param name="sourceHSV">HSV image</param>
        /// <param name="center">Center on given square</param>
        /// <param name="radius">Radius of the square (manhattan metric radius)</param>
        /// <returns>Value of Hue inside given square</returns>
        public static int ApproximateColorInSquare(Mat sourceHSV, Point center, int radius)
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
            // 2* because EMGU CV keeps hue value in [0,180]
            return 2 * color / ((2 * radius + 1) * (2 * radius + 1));
        }

        /// <summary>
        /// Determines whether hue in given square is between given constraints.
        /// Coinstraints can either determine [bottom,top] or [0,top]u[bottom,360] intervals
        /// </summary>
        /// <param name="bottomConstraint">Constraint</param>
        /// <param name="topConstraint">Constraint</param>
        /// <param name="image">Original image</param>
        /// <param name="HSV">HSV image</param>
        /// <param name="approxCurve">Given square</param>
        /// <returns>True if is between, false otherwise</returns>
        public static bool IsHueInSquareBetweenConstraints(
            int bottomConstraint,
            int topConstraint,
            Mat image,
            Mat HSV,
            VectorOfPoint approxCurve)
        {
            Point massCenter = GeometryUtilis.MassCenter(approxCurve);
            int radius = (int)(GeometryUtilis.CalculateSquareRadius(approxCurve, massCenter) * Constants.ColorRadiusDetectingFactor);
            if (radius > Constants.BlueTrackerRadiusMargin)
                return false;
            if (massCenter.Y - radius < 0 || massCenter.X - radius < 0 || massCenter.Y + radius > HSV.Height ||
                massCenter.X + radius > HSV.Width)
                return false;
            int color = ApproximateColorInSquare(HSV, massCenter, radius);
#if DEBUG
            DrawingService.PutTextOnMassCenterOfCurve(image, approxCurve, color.ToString());
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
    }
}