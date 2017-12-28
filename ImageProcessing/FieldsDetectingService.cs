using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.Util;

namespace BoardGameWithRobot.ImageProcessing
{
    internal class FieldsDetectingService
    {
        private readonly CameraService cameraService;

        private readonly Board board;

        public FieldsDetectingService(CameraService cam, Board b)
        {
            this.cameraService = cam;
            this.board = b;
        }

        public void DetectFieldsOnInit()
        {
            VectorOfVectorOfPoint curves = SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.cameraService.ActualFrame);
            for (int i = 0; i < curves.Size; i++)
            {
                VectorOfPoint approxCurve = SimpleImageProcessingServices.ApproximateCurve(curves[i]);

                // ignore if curve is relatively small or not convex
                if (SimpleImageProcessingServices.IsCurveSmallEnoughToBeIgnored(approxCurve, Constants.FieldSizeIgnoringMargin) ||
                    !CvInvoke.IsContourConvex(approxCurve))
                    continue;

                // Check if it is big square
                if (SimpleImageProcessingServices.IsSquare(this.cameraService.ActualFrame, approxCurve) &&
                    !this.board.LookForTracker(GeometryUtilis.MassCenter(approxCurve)))
                {
#if DEBUG
                    DrawingService.PutTextOnMassCenterOfCurve(this.cameraService.ActualFrame, approxCurve, "field");
#endif
                    this.AddFieldIfNecessary(approxCurve);
                }
                
            }
        }

        private void AddFieldIfNecessary(VectorOfPoint approxCurve)
        {
            if (!this.board.LookForField(GeometryUtilis.MassCenter(approxCurve)))
            {
                Field f = new Field(approxCurve, this.cameraService.ActualFrame);
                this.board.FieldsList.Add(f);
            }
        }

        
    }
}
