using System.Drawing;
using BoardGameWithRobot.Utilities;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Map
{
    /// <summary>
    /// Class which determines every basic object on the board.
    /// </summary>
    internal abstract class BoardObject
    {
        /// <summary>
        /// Center of the square that the object is inside
        /// </summary>
        public Point Center { get; }

        /// <summary>
        /// Half of the diagonal of the square
        /// </summary>
        public int Radius { get; }

        protected BoardObject(VectorOfPoint curve)
        {
            this.Center = GeometryUtilis.MassCenter(curve);
            this.Radius = GeometryUtilis.CalculateSquareRadius(curve, this.Center);
        }
    }
}
