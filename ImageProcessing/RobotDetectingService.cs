using System;
using System.Drawing;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.Structure;

namespace BoardGameWithRobot.ImageProcessing
{
    internal class RobotDetectingService
    {

        private readonly Board board;
        private readonly CameraService cameraService;

        public Image<Rgb, byte> Image { get; set; }

        public RobotDetectingService(Board b, CameraService cam)
        {
            this.board = b;
            this.cameraService = cam;
        }

        public bool DetectRobotDuringGame()
        {
            var curves =
                SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.Image.Mat);
            int curvesCount = 0;
            SquareBoundsCurve c1 = null;
            SquareBoundsCurve c2 = null;
            for (int i = 0; i < curves.Size; i++)
            {
                var boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));
                // ignore if curve is relatively small or not convex 
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        boundary.Curve, Constants.RobotTrackerContourSizeTopConstraint,
                        Constants.RobotTrackerContourSizeBottomConstraint) ||
                    boundary.Radius > 40 ||
                    !this.IsCurveYellow(this.Image.Mat,
                        FilteringServices.BGRToHSV(this.Image.Mat), boundary))
                    continue;
#if DEBUG
                DrawingService.PutSquareOnImage(this.Image.Mat, boundary);
                DrawingService.PutTextOnImage(this.Image.Mat, boundary.MassCenter, Math.Abs(CvInvoke.ContourArea(boundary.Curve)).ToString());
#endif
                if (curvesCount == 1 && GeometryUtilis.DistanceBetweenPoints(c1.MassCenter, boundary.MassCenter) < Constants.RobotTrackersMinimumDistanceForRecognition)
                    continue;
                curvesCount++;
                if (curvesCount == 1)
                {
                    c1 = boundary;
                }
                else if (curvesCount == 2)
                {
                    c2 = boundary;
                    if (c1 == null || c2 == null)
                        throw new ArgumentOutOfRangeException();

                    SquareBoundsCurve newFront = c1.MassCenter.X < c2.MassCenter.X ? c1 : c2;
                    SquareBoundsCurve back = c1.MassCenter.X < c2.MassCenter.X ? c2 : c1;
                    this.board.Robo.FrontVector = GeometryUtilis.DifferenceVector(newFront.MassCenter, back.MassCenter);
                    Point diffVector = GeometryUtilis.DifferenceVector(newFront.MassCenter, this.board.Robo.FrontCircle.MassCenter);
                    this.board.Robo.GlobalPosition = new Point(
                        this.board.Robo.GlobalPosition.X + diffVector.X,
                        this.board.Robo.GlobalPosition.Y + diffVector.Y);
                    this.DefineRobotRegion(null, false);
                    return true;
                }
            }
            this.DefineRobotRegion(null, false);
            return false;
        }

        public bool SecondDetectionOnInit()
        {
            var curves =
                SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.Image.Mat);

            int curvesCount = 0;
            SquareBoundsCurve c1 = null;
            SquareBoundsCurve c2 = null;

            for (int i = 0; i < curves.Size; i++)
            {
                var boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));
                // ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        boundary.Curve, Constants.RobotTrackerContourSizeTopConstraint,
                        Constants.RobotTrackerContourSizeBottomConstraint) ||
                    boundary.Radius > Constants.RobotCurveSizeIgnoringMargin ||
                    !this.IsCurveYellow(this.Image.Mat,
                        FilteringServices.BGRToHSV(this.Image.Mat), boundary))
                    continue;
                if (curvesCount == 1 && GeometryUtilis.DistanceBetweenPoints(c1.MassCenter, boundary.MassCenter) < Constants.RobotTrackersMinimumDistanceForRecognition)
                    continue;
                curvesCount++;
                if (curvesCount == 1)
                {
                    c1 = boundary;
                }
                else if (curvesCount == 2)
                {
                    c2 = boundary;
                    if (c1 == null || c2 == null)
                        throw new ArgumentOutOfRangeException();
                    this.board.Robo.FrontCircle = c1.MassCenter.X < c2.MassCenter.X ? c1 : c2;
                    return true;
                }
            }
            return false;
        }

        public bool DetectRobotOnInit()
        {
            var curves =
                SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(this.cameraService.ActualFrame);

            int curvesCount = 0;
            SquareBoundsCurve c1 = null;
            SquareBoundsCurve c2 = null;
            
            for (int i = 0; i < curves.Size; i++)
            {
                var boundary = new SquareBoundsCurve(SimpleImageProcessingServices.ApproximateCurve(curves[i]));
                // ignore if curve is relatively small or not convex
                if (!SimpleImageProcessingServices.IsCurveSizeBetweenMargins(
                        boundary.Curve, Constants.RobotTrackerContourSizeTopConstraint,
                        Constants.RobotTrackerContourSizeBottomConstraint) ||
                    boundary.Radius > Constants.RobotCurveSizeIgnoringMargin ||
                    !this.IsCurveYellow(this.cameraService.ActualFrame,
                        FilteringServices.BGRToHSV(this.cameraService.ActualFrame), boundary))
                    continue;
                if (curvesCount == 1 && GeometryUtilis.DistanceBetweenPoints(c1.MassCenter, boundary.MassCenter) < Constants.RobotTrackersMinimumDistanceForRecognition)
                    continue;
                curvesCount++;
                if (curvesCount == 1)
                {
                    c1 = boundary;
                }
                else if (curvesCount == 2)
                {
                    c2 = boundary;
                    if (c1 == null || c2 == null)
                        throw new ArgumentOutOfRangeException();
                    this.board.Robo = new Robot(c1.MassCenter.X < c2.MassCenter.X ? c1 : c2);
                    this.DefineRobotRegion(c1, c2, true);
                    return this.SecondDetectionOnInit();
                }
            }
            return false;
        }

        private bool IsCurveYellow(Mat image, Mat HSV, SquareBoundsCurve boundary)
        {
            return SimpleImageProcessingServices.IsHueInSquareBetweenConstraints(
                Constants.YellowHueBottomConstraint,
                Constants.YellowHueTopConstraint,
                image,
                HSV,
                boundary,
                false);
        }

        private void DefineRobotRegion(SquareBoundsCurve c1, SquareBoundsCurve c2, bool init)
        {
            this.DefineRobotRegion(GeometryUtilis.PointBetweenPoints(c1.MassCenter, c2.MassCenter), init);
        }

        private void DefineRobotRegion(Point? center, bool init)
        {
            center = center ?? new Point(0, 0);
            SquareBoundsCurve curve = new SquareBoundsCurve(center.Value, Constants.RobotImageRadius);
#if DEBUG
            if (this.Image != null)
            {
                var rob = this.Image.Mat.Clone();
                this.cameraService.ShowMatrix(rob, "robot");
            }
#endif
            this.Image?.Dispose();
            this.Image =
                SimpleImageProcessingServices.CutFragmentOfImage(
                    this.cameraService.ActualFrame,
                    init
                        ? curve
                        : new SquareBoundsCurve(
                            new Point(this.board.Robo.GlobalPosition.X + Constants.RobotImageRadius / 2, this.board.Robo.GlobalPosition.Y), Constants.RobotImageRadius));
        }
    }
}
