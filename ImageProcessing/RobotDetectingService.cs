using System;
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

        public bool DetectRobot(bool init = false)
        {
            Mat imageToSearch = init ? this.cameraService.ActualFrame : this.Image.Mat;

            var curves =
                SimpleImageProcessingServices.DetectEdgesAsCurvesOnImage(imageToSearch);

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
                    !this.IsCurveYellow(imageToSearch,
                        FilteringServices.BGRToHSV(imageToSearch), boundary))
                    continue;
#if DEBUG
                DrawingService.PutSquareOnImage(imageToSearch, boundary);
                DrawingService.PutTextOnImage(imageToSearch, boundary.MassCenter, Math.Abs(CvInvoke.ContourArea(boundary.Curve)).ToString());
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
                    if (init)
                        this.board.Robo = new Robot(c1.MassCenter.X < c2.MassCenter.X ? c1 : c2,
                            c1.MassCenter.X < c2.MassCenter.X ? c2 : c1);
                    else
                    {
                        this.board.Robo.FrontCircle = c1.MassCenter.X < c2.MassCenter.X ? c1 : c2;
                        this.board.Robo.BackCircle = c1.MassCenter.X < c2.MassCenter.X ? c1 : c2;
                    }
                    this.DefineRobotRegion(c1, c2, init);
                    return true;
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

        private void DefineRobotRegion(SquareBoundsCurve c1, SquareBoundsCurve c2, bool init = false)
        {
            SquareBoundsCurve curve = new SquareBoundsCurve(
                GeometryUtilis.PointBetweenPoints(c1.MassCenter, c2.MassCenter),
                (int) (GeometryUtilis.DistanceBetweenPoints(c1.MassCenter, c2.MassCenter)*(Math.Sqrt(2)/2)));
#if DEBUG
            DrawingService.PutSquareOnImage(this.cameraService.ActualFrame, curve);
#endif
            this.Image?.Dispose();
            this.Image =
                SimpleImageProcessingServices.CutFragmentOfImage(
                    this.cameraService.ActualFrame,
                    curve);
#if DEBUG
            var resized = this.Image.Mat.Clone();
            this.cameraService.ShowMatrix(resized, "robot");
#endif
            if (init)
            {
                //odswiezyc wspolrzedne 2 punktow na robocie do lokalnego ukladu wspolrzednych
            }
        }
    }
}
