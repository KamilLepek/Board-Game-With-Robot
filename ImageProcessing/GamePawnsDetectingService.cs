using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;

namespace BoardGameWithRobot.ImageProcessing
{
    internal class GamePawnsDetectingService
    {
        private readonly Board board;
        private readonly CameraService cameraService;


        public GamePawnsDetectingService(CameraService cam, Board b)
        {
            this.cameraService = cam;
            this.board = b;
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
                DrawingService.PutSquareOnBoard(this.cameraService.ActualFrame, boundary);
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
    }
}