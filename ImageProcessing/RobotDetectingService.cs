using System;
using System.Drawing;
using System.Linq.Expressions;
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

        //TODO:incomplete
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
                    Point oldCenter = this.board.Robo.Center;
                    Point newCenter = GeometryUtilis.PointBetweenPoints(c1.MassCenter, c2.MassCenter);
                    Point diffVector = GeometryUtilis.DiffrenceVector(newCenter, oldCenter);
                    this.board.Robo.FrontCircle = c1.MassCenter.X < c2.MassCenter.X ? c1 : c2;
                    this.board.Robo.BackCircle = c1.MassCenter.X < c2.MassCenter.X ? c2 : c1;
                    this.board.Robo.GlobalPosition = new Point(
                        this.board.Robo.GlobalPosition.X + diffVector.X,
                        this.board.Robo.GlobalPosition.Y + diffVector.Y);
                    this.DefineRobotRegion(newCenter,
                        (int)GeometryUtilis.NormOfVecotr(this.board.Robo.FrontVector), false);
                    return true;
                }
            }
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
                    this.board.Robo.BackCircle = c1.MassCenter.X < c2.MassCenter.X ? c2 : c1;
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
                    this.board.Robo = new Robot(c1.MassCenter.X < c2.MassCenter.X ? c1 : c2, c1.MassCenter.X < c2.MassCenter.X ? c2 : c1);
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
            this.DefineRobotRegion(GeometryUtilis.PointBetweenPoints(c1.MassCenter, c2.MassCenter),
                (int) (GeometryUtilis.DistanceBetweenPoints(c1.MassCenter, c2.MassCenter)), init);
        }

        //TODO: problem ejst taki że jak nie znajdzie raz to koniec, definiować ten sam region w momencie jak nie znajdzie trzeba
        private void DefineRobotRegion(Point center, int radius, bool init)
        {
            SquareBoundsCurve curve = new SquareBoundsCurve(center, radius);
#if DEBUG
            DrawingService.PutSquareOnImage(init ? this.cameraService.ActualFrame : this.Image.Mat, curve);
#endif
            this.Image?.Dispose();
            this.Image =
                SimpleImageProcessingServices.CutFragmentOfImage(
                    this.cameraService.ActualFrame,
                    init
                        ? curve
                        : new SquareBoundsCurve(
                            new Point(center.X + this.board.Robo.DiffVector.X,
                                center.Y + this.board.Robo.DiffVector.Y), radius));
#if DEBUG
            var resized = this.Image.Mat.Clone();
            this.cameraService.ShowMatrix(resized, "robot");
#endif
        }
    }
}
