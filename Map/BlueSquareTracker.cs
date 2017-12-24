using System;
using System.Drawing;
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
        /// Diagonal of the square
        /// </summary>
        public int Radius { get; set; }

        public BlueSquareTracker(VectorOfPoint curve)
        {
            this.Center = GeometryUtilis.MassCenter(curve);
            this.Radius = GeometryUtilis.CalculateSquareRadius(curve, this.Center);
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
    }
}
