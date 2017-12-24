using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Map
{
    internal class Board
    {
        public Point TopLeftCorner { get; private set; }

        public Size RectangleSize { get; private set; }

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
                    return true;
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

        public void SetMainRectangleSize()
        {
            //TODO :change min/max to LINQ
            if (this.TrackersList.Count != 4)
                throw new Exception();
            Point min = new Point();
            Point max = new Point();
            int distMin = Int32.MaxValue;
            int distMax = Int32.MinValue;
            foreach (var blueSquareTracker in this.TrackersList)
            {
                if (blueSquareTracker.Center.X + blueSquareTracker.Center.Y < distMin)
                {
                    distMin = blueSquareTracker.Center.X + blueSquareTracker.Center.Y;
                    min = blueSquareTracker.Center;
                }
                if (blueSquareTracker.Center.X + blueSquareTracker.Center.Y > distMax)
                {
                    distMax = blueSquareTracker.Center.X + blueSquareTracker.Center.Y;
                    max = blueSquareTracker.Center;
                }
            }
            this.TopLeftCorner = min;
            int width = max.X - min.X;
            int height = max.Y - min.Y;
            this.RectangleSize = new Size(width, height);
        }

        public void PrintMainRectangle(Mat image)
        {
            CvInvoke.Rectangle(
                image,
                new Rectangle(this.TopLeftCorner, this.RectangleSize),
                new Bgr(0, 255, 0).MCvScalar,
                2,
                LineType.EightConnected,
                0);
        }
    }
}
