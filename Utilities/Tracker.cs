using System.Drawing;

namespace BoardGameWithRobot.Utilities
{
    internal static class Tracker
    {
        /// <summary>
        /// Determines top left corner of square that we search for this blue square tracker
        /// </summary>
        public static Point Position { get; set; }

        /// <summary>
        /// Length of square side that we search in in pixels
        /// </summary>
        public static int Size { get; set; }
    }
}
