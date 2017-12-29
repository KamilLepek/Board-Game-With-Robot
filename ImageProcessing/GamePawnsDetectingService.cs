
using System.Drawing;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.Util;

namespace BoardGameWithRobot.ImageProcessing
{
    internal class GamePawnsDetectingService
    {
        private readonly CameraService cameraService;

        private readonly Board board;


        public GamePawnsDetectingService(CameraService cam, Board b)
        {
            this.cameraService = cam;
            this.board = b;
        }

        public void DetectPawnsOnInit()
        {
            VectorOfVectorOfPoint curves =
                SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.cameraService.ActualFrame);
            for (int i = 0; i < curves.Size; i++)
            {
                VectorOfPoint approxCurve = SimpleImageProcessingServices.ApproximateCurve(curves[i]);

                Point massCntr = GeometryUtilis.MassCenter(approxCurve);
                int r = GeometryUtilis.CalculateSquareRadius(approxCurve, massCntr);

                // ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        approxCurve, Constants.PawnContourSizeTopConstraint,
                        Constants.PawnContourSizeBottomConstraint) ||
                    r > 40 || (GeometryUtilis.DistanceBetweenPoints(massCntr,
                                   this.board.FieldsList.Find(v => v.Label == "H1").Center) >
                               Constants.PawnDistanceFromFieldMargin &&
                               GeometryUtilis.DistanceBetweenPoints(massCntr,
                                   this.board.FieldsList.Find(v => v.Label == "R1").Center) >
                               Constants.PawnDistanceFromFieldMargin))
                    continue;
#if DEBUG
                DrawingService.PutSquareOnBoard(this.cameraService.ActualFrame, massCntr, r);
#endif
                this.AddPawnsIfNecessary(approxCurve);
            }
        }

        private void AddPawnsIfNecessary(VectorOfPoint curve)
        {
            if (!this.board.LookForPawnOnInit(GeometryUtilis.MassCenter(curve)))
            {
                Enums.Player player =
                    GeometryUtilis.DistanceBetweenPoints(GeometryUtilis.MassCenter(curve),
                        this.board.FieldsList.Find(v => v.Label == "H1").Center) <
                    GeometryUtilis.DistanceBetweenPoints(GeometryUtilis.MassCenter(curve),
                        this.board.FieldsList.Find(v => v.Label == "R1").Center)
                        ? Enums.Player.Human
                        : Enums.Player.Robot;
                GamePawn pawn = new GamePawn(curve, player);
                this.board.PawnsList.Add(pawn);
            }
        }
    }
}
