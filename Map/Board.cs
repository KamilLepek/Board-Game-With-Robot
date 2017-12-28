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
        public List<BlueSquareTracker> TrackersList { get; }

        public List<Field> FieldsList { get; }

        public Board()
        {
            this.TrackersList = new List<BlueSquareTracker>();
            this.FieldsList = new List<Field>();
        }

        public bool LookForField(Point position)
        {
            if (!this.FieldsList.Any())
                return false;
            foreach (var field in this.FieldsList)
            {
                if (GeometryUtilis.DistanceBetweenPoints(field.Center, position) <
                    Constants.DistanceFromLastPositionIgnoringMargin)
                    return true;
            }
            return false;
        }

        public void SetFieldLabel(Field field)
        {
            List<Field> sortedFieldList;
            string result;
            if (field.Player == Enums.Player.Human)
            {
                sortedFieldList = this.FieldsList.Where(v => v.Player == Enums.Player.Human).OrderBy(v => v.Center.X)
                    .ToList();
                result = "H";
            }
            else
            {
                sortedFieldList = this.FieldsList.Where(v => v.Player == Enums.Player.Robot).OrderBy(v => v.Center.X)
                    .ToList();
                result = "R";
            }
            int i = sortedFieldList.IndexOf(field);
            result += i;
            field.Label = result;
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
                    Constants.DistanceFromLastPositionIgnoringMargin)
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

        public void PrintFieldsOnBoard(Mat image)
        {
            if (this.FieldsList.Count != Constants.NumberOfFields)
                throw new ArgumentOutOfRangeException();
            foreach (var field in this.FieldsList)
            {
                field.Print(image);
            }
        }
    }
}
