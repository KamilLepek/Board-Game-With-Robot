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

        public static double DistanceBetweenPoints(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public static int CalculateSquareRadius(VectorOfPoint approxCurve, Point massCenter)
        {
            return (int) DistanceBetweenPoints(approxCurve[0], massCenter);
        }
    }
}