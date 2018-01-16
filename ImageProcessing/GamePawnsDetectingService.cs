using System;
using System.Drawing;
using System.Linq;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.Structure;

namespace BoardGameWithRobot.ImageProcessing
{
    internal class GamePawnsDetectingService
    {
        private readonly Board board;
        private readonly CameraService cameraService;

        public int PawnDetectionAfterMovementFramesAmount { get; set; }

        public GamePawnsDetectingService(CameraService cam, Board b)
        {
            this.cameraService = cam;
            this.board = b;
            this.PawnDetectionAfterMovementFramesAmount = 0;
        }

        public void DetectPawnsOnInit()
        {
            var curves =
                SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.cameraService.ActualFrame);
            for (int i = 0; i < curves.Size; i++)
            {
                var boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));

                // ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        boundary.Curve, Constants.PawnContourSizeTopConstraint,
                        Constants.PawnContourSizeBottomConstraint) ||
                    boundary.Radius > 40 || GeometryUtilis.DistanceBetweenPoints(boundary.MassCenter,
                        this.board.FieldsList.Find(v => v.Label == "H1").Boundary.MassCenter) >
                    Constants.PawnDistanceFromFieldMargin &&
                    GeometryUtilis.DistanceBetweenPoints(boundary.MassCenter,
                        this.board.FieldsList.Find(v => v.Label == "R1").Boundary.MassCenter) >
                    Constants.PawnDistanceFromFieldMargin)
                    continue;
#if DEBUG
                DrawingService.PutSquareOnImage(this.cameraService.ActualFrame, boundary);
                DrawingService.PutTextOnImage(this.cameraService.ActualFrame, boundary.MassCenter, Math.Abs(CvInvoke.ContourArea(boundary.Curve)).ToString());
#endif
                this.AddPawnsIfNecessary(boundary);
            }
        }

        private void AddPawnsIfNecessary(SquareBoundsCurve boundary)
        {
            if (!this.board.LookForPawnOnInit(boundary.MassCenter))
            {
                var player =
                    GeometryUtilis.DistanceBetweenPoints(boundary.MassCenter,
                        this.board.FieldsList.Find(v => v.Label == "H1").Boundary.MassCenter) <
                    GeometryUtilis.DistanceBetweenPoints(boundary.MassCenter,
                        this.board.FieldsList.Find(v => v.Label == "R1").Boundary.MassCenter)
                        ? Enums.Player.Human
                        : Enums.Player.Robot;
                var pawn = new GamePawn(boundary, player);
                this.board.PawnsList.Add(pawn);
            }
        }

        public bool DetectPawnOnExpectedField(ref int movementNumber, Enums.Player turn)
        {
            GamePawn movingPawn = this.board.PawnsList.First(v => v.Player == turn);
            if (movementNumber + movingPawn.SquareNumber > Constants.NumberOfFields / 2)
                movementNumber = Constants.NumberOfFields / 2 - movingPawn.SquareNumber;
            int expectedFieldNumber = movementNumber + movingPawn.SquareNumber;


            if (this.DetectPawnOnSpecifiedField(expectedFieldNumber, turn))
                this.PawnDetectionAfterMovementFramesAmount++;
            else
            {
                this.PawnDetectionAfterMovementFramesAmount--;
                if (this.PawnDetectionAfterMovementFramesAmount < 0)
                    this.PawnDetectionAfterMovementFramesAmount = 0;
            }
            if (this.PawnDetectionAfterMovementFramesAmount >
                Constants.AmountOfFramesToDeterminePawn) //validate that player has finished his movement!
            {
                movingPawn.SquareNumber = expectedFieldNumber;
                return true;
            }
            return false;
        }

        private bool DetectPawnOnSpecifiedField(int fieldNumber, Enums.Player turn)
        {
            string label = turn == Enums.Player.Human ? "H" : "R";
            label += fieldNumber;
            Field searchingFiled = this.board.FieldsList.First(v => v.Label == label);
            Image<Rgb, byte> img = SimpleImageProcessingServices.CutFragmentOfImage(this.cameraService.ActualFrame, searchingFiled.Boundary);
            var curves =
                SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(img.Mat);
            for (int i = 0; i < curves.Size; i++)
            {
                var boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));

                // ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        boundary.Curve, Constants.PawnContourSizeTopConstraint,
                        Constants.PawnContourSizeBottomConstraint) ||
                    boundary.Radius > 40)
                    continue;
                Point centerPoint =
                    new Point(searchingFiled.Boundary.MassCenter.X + boundary.MassCenter.X - img.Mat.Width / 2,
                        searchingFiled.Boundary.MassCenter.Y + boundary.MassCenter.Y - img.Mat.Height / 2);
                SquareBoundsCurve detectedFragment = new SquareBoundsCurve(centerPoint, boundary.Radius);
                DrawingService.PutSquareOnImage(this.cameraService.ActualFrame, detectedFragment);
#if DEBUG
                DrawingService.PutTextOnImage(this.cameraService.ActualFrame, detectedFragment.MassCenter, Math.Abs(CvInvoke.ContourArea(boundary.Curve)).ToString());
#endif
                return true;
            }
            return false;
        }
    }
}