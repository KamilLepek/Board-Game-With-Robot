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
        public Board()
        {
            this.TrackersList = new List<BlueSquareTracker>();
            this.FieldsList = new List<Field>();
            this.PawnsList = new List<GamePawn>();
        }

        public List<BlueSquareTracker> TrackersList { get; }

        public List<Field> FieldsList { get; }

        public List<GamePawn> PawnsList { get; }

        public bool LookForPawnOnInit(Point position)
        {
            if (!this.PawnsList.Any())
                return false;
            foreach (var pawn in this.PawnsList)
                if (GeometryUtilis.DistanceBetweenPoints(pawn.Boundary.MassCenter, position) <
                    Constants.DistanceFromLastPositionIgnoringMargin)
                    return true;
            return false;
        }

        public bool LookForField(Point position)
        {
            if (!this.FieldsList.Any())
                return false;
            foreach (var field in this.FieldsList)
                if (GeometryUtilis.DistanceBetweenPoints(field.Boundary.MassCenter, position) <
                    Constants.DistanceFromLastPositionIgnoringMargin)
                    return true;
            return false;
        }

        public void SetFieldLabel(Field field)
        {
            List<Field> sortedFieldList;
            string result;
            if (field.Player == Enums.Player.Human)
            {
                sortedFieldList = this.FieldsList.Where(v => v.Player == Enums.Player.Human)
                    .OrderBy(v => v.Boundary.MassCenter.X)
                    .ToList();
                result = "H";
            }
            else
            {
                sortedFieldList = this.FieldsList.Where(v => v.Player == Enums.Player.Robot)
                    .OrderBy(v => v.Boundary.MassCenter.X)
                    .ToList();
                result = "R";
            }
            int i = Constants.NumberOfFields / 2 - sortedFieldList.IndexOf(field);
            result += i;
            field.Label = result;
        }

        /// <summary>
        ///     Looks for tracker with specified position
        /// </summary>
        /// <param name="position">given position</param>
        /// <returns>Returns true if any tracker is found nearby</returns>
        public bool LookForTracker(Point position)
        {
            if (!this.TrackersList.Any())
                return false;
            foreach (var tracker in this.TrackersList)
                if (GeometryUtilis.DistanceBetweenPoints(tracker.Boundary.MassCenter, position) <
                    Constants.DistanceFromLastPositionIgnoringMargin)
                {
                    tracker.UpdateParamsOnDetection();
                    return true;
                }
            return false;
        }

        public void PrintTrackersOnImage(Mat image)
        {
            if (!this.TrackersList.Any())
                throw new Exception("Empty trackers list!");
            foreach (var blueSquareTracker in this.TrackersList)
                blueSquareTracker.PrintTracker(image);
        }

        public Point DetermineTopLeftCornerOfTrackersListRectangle()
        {
            var p = new Point(int.MaxValue, int.MaxValue);
            foreach (var blueSquareTracker in this.TrackersList)
                if (blueSquareTracker.Boundary.MassCenter.X + blueSquareTracker.Boundary.MassCenter.Y < p.X + p.Y)
                    p = blueSquareTracker.Boundary.MassCenter;
            return p;
        }

        public Point DetermineBottomRightCornerOfTrackersListRectangle()
        {
            var p = new Point(0, 0);
            foreach (var blueSquareTracker in this.TrackersList)
                if (blueSquareTracker.Boundary.MassCenter.X + blueSquareTracker.Boundary.MassCenter.Y > p.X + p.Y)
                    p = blueSquareTracker.Boundary.MassCenter;
            return p;
        }

        public void PrintFieldsOnBoard(Mat image)
        {
            if (this.FieldsList.Count != Constants.NumberOfFields)
                throw new ArgumentOutOfRangeException();
            foreach (var field in this.FieldsList)
                field.Print(image);
        }

        public void PrintPawnsSquarePlaceOnBoard(Mat image)
        {
            if(this.PawnsList.Count != Constants.NumberOfPawns)
                throw new ArgumentOutOfRangeException();
            foreach (var gamePawn in this.PawnsList)
            {
                gamePawn.PrintPawnPlace(image);
            }
        }
    }
}