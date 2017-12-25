using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot.Map
{
    internal class Board
    {
        public List<BlueSquareTracker> TrackersList { get; set; }

        public Board()
        {
            this.TrackersList = new List<BlueSquareTracker>();
        }

        /// <summary>
        /// Looks for tracker with specified position
        /// </summary>
        /// <param name="position">given position</param>
        /// <returns>Returns true if any tracker is found nearby</returns>
        public bool LookForTracker(Point position)
        {
            if (!this.TrackersList.Any())
                return false;
            foreach (var tracker in this.TrackersList)
            {
                if (GeometryUtilis.DistanceBetweenPoints(tracker.Center, position) <
                    Constants.TrackerDistanceDifferenceFromLastPosition)
                {
                    tracker.UpdateParamsOnDetection();
                    return true;
                }
            }
            return false;
        }

        public void PrintTrackersOnImage(Mat image)
        {
            foreach (var blueSquareTracker in this.TrackersList)
            {
                blueSquareTracker.PrintTracker(image);
            }
        }
    }
}
