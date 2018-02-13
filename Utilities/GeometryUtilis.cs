using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Utilities
{
    internal static class GeometryUtilis
    {
        public static Point MassCenter(VectorOfPoint approxCurve)
        {
            var moment = CvInvoke.Moments(approxCurve);
            return new Point((int) (moment.M10 / moment.M00), (int) (moment.M01 / moment.M00));
        }

        /// <summary>
        /// Diffrence of 2 vectors (a-b)
        /// </summary>
        public static Point DifferenceVector(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static double NormOfVector(Point x)
        {
            return Math.Sqrt(x.X * x.X + x.Y * x.Y);
        }

        public static double DistanceBetweenPoints(Point a, Point b)
        {
            return NormOfVector(DifferenceVector(a, b));
        }

        public static int CalculateSquareRadius(VectorOfPoint approxCurve, Point massCenter)
        {
            return (int) DistanceBetweenPoints(approxCurve[0], massCenter);
        }

        public static Point PointBetweenPoints(Point a, Point b)
        {
            return new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }

        public static double AngleBetweenVectors(Point a, Point b)
        {
            double sin = a.X * b.Y - b.X * a.Y;
            double cos = a.X * b.X + a.Y * b.Y;
            return Math.Atan2(sin, cos) * (180 / Math.PI);
        }
    }
}