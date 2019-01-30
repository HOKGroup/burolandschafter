using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Formats;
using Rhino.Geometry;

using HOK.Buro.Logic;
using Grasshopper.Kernel;

namespace HOK.Buro.Factory
{
    //All geometry generated is appended to any output lists in the order that they're mentioned in the method name.
    //SquareInCircle[0] is the square, SquareInCircle[1] is the Circle.
    //CircleInSquare[0] is the circle, CircleInSquare[1] is the Square.
    public static class BrepFactory
    {
        public static List<Brep> BrepEnvGen(List<Brep> items)
        {
            //Streamline brep generation and adding to lists? Lots of small repetition.

            return null;
        }
        public static List<Brep> TwoIntersectingCircles()
        {
            List<Brep> TestEnvironment = new List<Brep>();

            Brep CircleLeft = Brep.CreatePlanarBreps(RegionsFactory.TwoDisjointCircles()[0], .1)[0];
            Brep CircleRight = Brep.CreatePlanarBreps(RegionsFactory.TwoDisjointCircles()[1], .1)[0];

            TestEnvironment.Add(CircleLeft);
            TestEnvironment.Add(CircleRight);

            return TestEnvironment;
        }

        public static List<Brep> CircleWithTransverseRectangle()
        {
            List<Brep> TestEnvironment = new List<Brep>();

            Brep testCircle = Brep.CreatePlanarBreps(RegionsFactory.CenteredCircleRadius(1), .1)[0];
            Brep testRectangle = Brep.CreatePlanarBreps(RegionsFactory.RectangleWHC(1, 4, Point3d.Origin), .1)[0];

            TestEnvironment.Add(testCircle);
            TestEnvironment.Add(testRectangle);

            return TestEnvironment;
        }

        public static List<Brep> SqaureWithInscribedCircle()
        {
            List<Brep> TestEnvironment = new List<Brep>();

            Brep testCircle = Brep.CreatePlanarBreps(RegionsFactory.CenteredCircleRadius(0.5), .01)[0];
            Brep testSquare = Brep.CreatePlanarBreps(RegionsFactory.RectangleWHC(1, 1, Point3d.Origin), .01)[0];

            TestEnvironment.Add(testSquare);
            TestEnvironment.Add(testCircle);

            return TestEnvironment;
        }

        public static List<Brep> SquareWithCircumscribedCircle()
        {
            List<Brep> TestEnvironment = new List<Brep>();

            Brep testCircle = Brep.CreatePlanarBreps(RegionsFactory.CenteredCircleRadius(Math.Sqrt(2) / 2), .01)[0];
            Brep testSquare = Brep.CreatePlanarBreps(RegionsFactory.RectangleWHC(1, 1, Point3d.Origin), .01)[0];

            TestEnvironment.Add(testSquare);
            TestEnvironment.Add(testCircle);

            return TestEnvironment;
        }

        public static List<Brep> SquareInCircle()
        {
            List<Brep> TestEnvironment = new List<Brep>();

            Brep testCircle = Brep.CreatePlanarBreps(RegionsFactory.CenteredCircleRadius(5), .1)[0];
            Brep testSquare = Brep.CreatePlanarBreps(RegionsFactory.RectangleWHC(1, 1, Point3d.Origin), .1)[0];

            TestEnvironment.Add(testSquare);
            TestEnvironment.Add(testCircle);

            return TestEnvironment;
        }

        public static List<Brep> CircleInSquare()
        {
            List<Brep> TestEnvironment = new List<Brep>();

            Brep testCircle = Brep.CreatePlanarBreps(RegionsFactory.CenteredCircleRadius(1), .1)[0];
            Brep testSquare = Brep.CreatePlanarBreps(RegionsFactory.RectangleWHC(5, 5, Point3d.Origin), .1)[0];

            TestEnvironment.Add(testCircle);
            TestEnvironment.Add(testSquare);

            return TestEnvironment;
        }

        public static List<Brep> TwoDisjointSquares()
        {
            List<Brep> TestEnvironment = new List<Brep>();

            Brep leftSquare = Brep.CreatePlanarBreps(RegionsFactory.RectangleWHC(1, 1, new Point3d(-5, 0, 0)), .1)[0];
            Brep rightSquare = Brep.CreatePlanarBreps(RegionsFactory.RectangleWHC(1, 1, new Point3d(5, 0, 0)), .1)[0];

            TestEnvironment.Add(leftSquare);
            TestEnvironment.Add(rightSquare);

            return TestEnvironment;
        }
    }

    public static class CurvesFactory
    {
        public static Curve LineSDL(Point2d startPoint, Vector2d direction, double length)
        {
            double startX = startPoint.X;
            double startY = startPoint.Y;

            double scaleFactor = length / direction.Length;
            Vector2d placementVector = direction * scaleFactor;

            double endX = placementVector.X;
            double endY = placementVector.Y;
            Point2d endPoint = new Point2d(endX, endY);

            Curve resultCurve = new LineCurve(startPoint, endPoint);

            return resultCurve;
        }

        public static Curve UnitXCurve()
        {
            Point3d startPoint = new Point3d(0, 0, 0);
            Point3d endPoint = new Point3d(1, 0, 0);

            Curve curve = new LineCurve(startPoint, endPoint);

            return curve;
        }

        public static Curve UnitXCurve(double length)
        { 
            Point3d startPoint = new Point3d(0, 0, 0);
            Point3d endPoint = new Point3d(length, 0, 0);

            Curve curve = new LineCurve(startPoint, endPoint);

            return curve;
        }

        public static Curve CenteredXCurve()
        {
            Point3d startPoint = new Point3d(-0.5, 0, 0);
            Point3d endPoint = new Point3d(0.5, 0, 0);

            Curve curve = new LineCurve(startPoint, endPoint);

            return curve;
        }

        public static Curve CenteredXCurve(double length)
        {
            double dist = length / 2;

            Point3d startPoint = new Point3d(-dist, 0, 0);
            Point3d endPoint = new Point3d(dist, 0, 0);

            Curve curve = new LineCurve(startPoint, endPoint);

            return curve;
        }

        public static Curve UnitYCurve()
        {
            Point3d startPoint = new Point3d(0, 0, 0);
            Point3d endPoint = new Point3d(0, 1, 0);

            Curve curve = new LineCurve(startPoint, endPoint);

            return curve;
        }

        public static Curve UnitYCurve(double length)
        {
            Point3d startPoint = new Point3d(0, 0, 0);
            Point3d endPoint = new Point3d(0, length, 0);

            Curve curve = new LineCurve(startPoint, endPoint);

            return curve;
        }

        public static Curve CenteredYCurve()
        {
            Point3d startPoint = new Point3d(0, -0.5, 0);
            Point3d endPoint = new Point3d(0, 0.5, 0);

            Curve curve = new LineCurve(startPoint, endPoint);

            return curve;
        }

        public static Curve CenteredYCurve(double length)
        {
            double dist = length / 2;

            Point3d startPoint = new Point3d(0, -dist, 0);
            Point3d endPoint = new Point3d(0, dist, 0);

            Curve curve = new LineCurve(startPoint, endPoint);

            return curve;
        }

        public static Curve UnitZCurve()
        {
            Point3d startPoint = new Point3d(0, 0, 0);
            Point3d endPoint = new Point3d(0, 0, 1);

            Curve curve = new LineCurve(startPoint, endPoint);

            return curve;
        }

        public static Curve RectangleCWH(Point3d center, double width, double height)
        {
            double frcW = width / 2;
            double frcH = height / 2;

            Curve rectangle = new Rectangle3d(Plane.WorldXY, new Interval(-frcW, frcW), new Interval(-frcH, frcH)).ToNurbsCurve();

            return rectangle;
        }
    }

    public static class RegionsFactory
    {
        public static List<Curve> TwoDisjointCircles()
        {
            List<Curve> TestEnvironment = new List<Curve>();

            Circle CircleLeft = new Circle(new Point3d(-2, 0, 0), 1);
            Circle CircleRight = new Circle(new Point3d(2, 0, 0), 1);

            TestEnvironment.Add(CircleLeft.ToNurbsCurve());
            TestEnvironment.Add(CircleRight.ToNurbsCurve());

            return TestEnvironment;
        }

        public static List<Curve> VennDiagramCircles()
        {
            List<Curve> TestEnvironment = new List<Curve>();

            Curve CircleLeft = null;
            Curve CircleRight = null;

            GH_Convert.ToCurve(new Circle(new Point3d(-1, 0, 0), 2), ref CircleLeft, GH_Conversion.Both);
            GH_Convert.ToCurve(new Circle(new Point3d(1, 0, 0), 2), ref CircleRight, GH_Conversion.Both);

            TestEnvironment.Add(CircleLeft);
            TestEnvironment.Add(CircleRight);

            return TestEnvironment;
        }

        public static Curve CenteredCircleRadius(double r)
        {
            return new Circle(Point3d.Origin, r).ToNurbsCurve();
        }

        public static Curve CircleCR(Point3d center, double r)
        {
            return new Circle(center, r).ToNurbsCurve();
        }

        public static Curve CenteredRectangleWH(double width, double height)
        {
            return new Rectangle3d(Utils.GetDefaultPlane(), width, height).ToNurbsCurve();
        }

        public static Curve RectangleWHC(double width, double height, Point3d center)
        {
            double deltaX = width / 2;
            double deltaY = height / 2;

            double centerX = center.X;
            double centerY = center.Y;

            List<Point3d> CornerPoints = new List<Point3d>();

            Point3d TopRightPoint = new Point3d(centerX + deltaX, centerY + deltaY, 0);
            Point3d TopLeftPoint = new Point3d(centerX - deltaX, centerY + deltaY, 0);
            Point3d BottomLeftPoint = new Point3d(centerX - deltaX, centerY - deltaY, 0);
            Point3d BottomRightPoint = new Point3d(centerX + deltaX, centerY - deltaY, 0);

            CornerPoints.Add(TopRightPoint);
            CornerPoints.Add(TopLeftPoint);
            CornerPoints.Add(BottomLeftPoint);
            CornerPoints.Add(BottomRightPoint);
            CornerPoints.Add(TopRightPoint);

            Rectangle3d Rectangle = new Rectangle3d(new Plane(center, Vector3d.ZAxis), TopRightPoint, BottomLeftPoint);

            return Rectangle.ToNurbsCurve();
        }
    }

    public static class RectangleFactory
    {

    }

    public static class TestFitFactory
    {
        public static TestFitPackage DieFive(double side)
        {
            //A simple test fit configuration modeled after the five side of a since die.
            //Center point is core.
            //Other four are structure.
            //TODO: Implement circulation.

            double frc = side / 6;
            double r = side / 5;

            Curve floorProfile = RegionsFactory.RectangleWHC(side, side, Point3d.Origin);
            Curve coreCurve = RegionsFactory.CenteredCircleRadius(r);

            List<Curve> strCurves = new List<Curve>();
            strCurves.Add(RegionsFactory.CircleCR(new Point3d(2 * frc, 2 * frc, 0), r));
            strCurves.Add(RegionsFactory.CircleCR(new Point3d(-2 * frc, 2 * frc, 0), r));
            strCurves.Add(RegionsFactory.CircleCR(new Point3d(-2 * frc, -2 * frc, 0), r));
            strCurves.Add(RegionsFactory.CircleCR(new Point3d(2 * frc, -2 * frc, 0), r));

            FloorPlanPackage FloorPlan = new FloorPlanPackage(floorProfile, coreCurve, null, strCurves, null, null);

            TestFitPackage TestFit = new TestFitPackage(FloorPlan, null);

            return TestFit;
        }
    }
}
