using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot.ImageProcessing
{
    internal class FieldsDetectingService
    {
        private readonly Board board;
        private readonly CameraService cameraService;

        public FieldsDetectingService(CameraService cam, Board b)
        {
            this.cameraService = cam;
            this.board = b;
        }

        public void DetectFieldsOnInit()
        {
            var curves = SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.cameraService.ActualFrame);
            for (int i = 0; i < curves.Size; i++)
            {
                var boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));

                // ignore if curve is relatively small or not convex
                if (SimpleImageProcessingServices.IsCurveSmallEnoughToBeIgnored(boundary.Curve,
                        Constants.FieldSizeIgnoringMargin) ||
                    !CvInvoke.IsContourConvex(boundary.Curve))
                    continue;

                // Check if it is big square
                if (SimpleImageProcessingServices.IsSquare(this.cameraService.ActualFrame, boundary) &&
                    !this.board.LookForTracker(boundary.MassCenter))
                {
#if DEBUG
                    DrawingService.PutTextOnImage(this.cameraService.ActualFrame, boundary.MassCenter, "field");
#endif
                    this.AddFieldIfNecessary(boundary);
                }
            }
        }

        private void AddFieldIfNecessary(SquareBoundsCurve boundary)
        {
            if (!this.board.LookForField(boundary.MassCenter))
                this.board.FieldsList.Add(new Field(boundary, this.cameraService.ActualFrame));
        }
    }
}