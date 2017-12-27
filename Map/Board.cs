using System;
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
            if (!this.TrackersList.Any())
                throw new Exception("Empty trackers list!");
            foreach (var blueSquareTracker in this.TrackersList)
            {
                blueSquareTracker.PrintTracker(image);
            }
        }

        public Point DetermineTopLeftCornerOfTrackersListRectangle()
        {
            Point p = new Point(Int32.MaxValue, Int32.MaxValue);
            foreach (var blueSquareTracker in this.TrackersList)
            {
                if (blueSquareTracker.Center.X + blueSquareTracker.Center.Y < p.X + p.Y)
                {
                    p = blueSquareTracker.Center;
                }
            }
            return p;
        }

        public Point DetermineBottomRightCornerOfTrackersListRectangle()
        {
            Point p = new Point(0, 0);
            foreach (var blueSquareTracker in this.TrackersList)
            {
                if (blueSquareTracker.Center.X + blueSquareTracker.Center.Y > p.X + p.Y)
                {
                    p = blueSquareTracker.Center;
                }
            }
            return p;
        }
    }
}
