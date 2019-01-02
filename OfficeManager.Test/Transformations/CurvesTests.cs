using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Geometry;
using HOK.OfficeManager.Factory;
using HOK.OfficeManager.Logic.Transformations;
using System.Windows.Forms;

namespace HOK.OfficeManager.Tests.Transformations.Curves
{
    [TestClass]
    public class OffsetClosedTests
    {
        [TestMethod]
        public void NotImplemented()
        {
            Curve TestCurve = CurvesFactory.UnitXCurve().ToNurbsCurve();

            //Console.WriteLine("Generated curve.");
            //Console.WriteLine(TestCurve.GetLength());

            Curve[] offset = TestCurve.Offset(Plane.WorldXY, 1, 0.1, CurveOffsetCornerStyle.None);

            Console.WriteLine("Curve.Offset is not currently implemented for Rhino Compute. It will only return null.");

            Assert.IsNull(offset);
            //Assert.IsNotNull(resultGeometry);
        }
    }

    [TestClass]
    public class ShatterToSegmentsTests
    {
        [TestMethod]
        public void TwoIntersectingCurves_ReturnsFourSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(CurvesFactory.CenteredXCurve());
            TestEnv.Add(CurvesFactory.CenteredYCurve());

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 4);
        }

        [TestMethod]
        public void TwoDisjointCurves_ReturnsTwoSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(1, 0), new Vector2d(0, 1), 1));
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(-1, 0), new Vector2d(0, 1), 1));

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 2);
        }

        [TestMethod]
        public void TwoDisjointCurves_CurvesUnchanged()
        {
            List<Curve> TestEnv = new List<Curve>();

            Curve curveA = CurvesFactory.LineSDL(new Point2d(1, 0), new Vector2d(0, 1), 1);
            Curve curveB = CurvesFactory.LineSDL(new Point2d(-1, 0), new Vector2d(0, 1), 1);

            //Console.WriteLine(curveA.GetLength() + " & " + curveB.GetLength());

            TestEnv.Add(curveA);
            TestEnv.Add(curveB);

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            //Console.WriteLine(resultGeometry[0].GetLength() + " & " + resultGeometry[1].GetLength());

            Assert.AreEqual(curveA.GetLength(), resultGeometry[0].GetLength());
            Assert.AreEqual(curveB.GetLength(), resultGeometry[1].GetLength());
        }

        [TestMethod]
        public void FourCurveAsterisk_ReturnsEightSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(new LineCurve(new Point2d(0, 1), new Point2d(0, -1)));
            TestEnv.Add(new LineCurve(new Point2d(-1, 0), new Point2d(1, 0)));
            TestEnv.Add(new LineCurve(new Point2d(-1, -1), new Point2d(1, 1)));
            TestEnv.Add(new LineCurve(new Point2d(-1, 1), new Point2d(1, -1)));

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 8);
        }

        [TestMethod]
        public void TriangleExtendedEdges_ReturnsNineSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(new LineCurve(new Point2d(1, 1), new Point2d(-2, -2)));
            TestEnv.Add(new LineCurve(new Point2d(-1, 1), new Point2d(2, -2)));
            TestEnv.Add(new LineCurve(new Point2d(-2, -1), new Point2d(2, -1)));

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 9);
        }

        [TestMethod]
        public void CircleWithTransverseLine_ReturnsFiveSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(new Circle(Point3d.Origin, 0.25).ToNurbsCurve());
            TestEnv.Add(CurvesFactory.CenteredYCurve());

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 5);
        }

        [TestMethod]
        public void CircleWithChord_ReturnsThreeSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(new Circle(Point3d.Origin, 0.5).ToNurbsCurve());
            TestEnv.Add(CurvesFactory.CenteredYCurve());

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 3);
        }

        [TestMethod]
        public void CircleWithTrimmedCrosshair_ReturnsEightSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(new Circle(Point3d.Origin, 0.5).ToNurbsCurve());
            TestEnv.Add(CurvesFactory.CenteredYCurve());
            TestEnv.Add(CurvesFactory.CenteredXCurve());

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 8);
        }

        [TestMethod]
        public void CircleAroundSmallCrosshair_ReturnsFiveSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(new Circle(Point3d.Origin, 0.5).ToNurbsCurve());
            TestEnv.Add(CurvesFactory.CenteredYCurve(0.1));
            TestEnv.Add(CurvesFactory.CenteredXCurve(0.1));

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 5);
        }

        [TestMethod]
        public void CircleWithIntersectingCrosshair_ReturnsTwelveSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(new Circle(Point3d.Origin, 0.5).ToNurbsCurve());
            TestEnv.Add(CurvesFactory.CenteredYCurve(5));
            TestEnv.Add(CurvesFactory.CenteredXCurve(5));

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 12);
        }

        [TestMethod]
        public void TwoIntersectingCircles_ReturnsFourSegments()
        {
            List<Curve> TestEnv = new List<Curve>();

            TestEnv.Add(new Circle(new Point3d(-1, 0, 0), 1.5).ToNurbsCurve());
            TestEnv.Add(new Circle(new Point3d(1, 0, 0), 1.5).ToNurbsCurve());

            List<Curve> resultGeometry = Logic.Transformations.Curves.ShatterToSegments(TestEnv);

            Assert.AreEqual(resultGeometry.Count, 4);
        }
    }

    [TestClass]
    public class RebuildPerpendicularToTests
    {
        [TestMethod]
        public void UnitX_YAxis_LengthUnchanged()
        {
            Curve testCurve = CurvesFactory.UnitXCurve(10);
            Vector3d direction = Vector3d.YAxis;

            double originalLength = testCurve.GetLength();

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            double resultLength = resultCurve.GetLength();

            bool result = (resultLength - originalLength < 0.1) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UnitX_XAxis_LengthUnchanged()
        {
            Curve testCurve = CurvesFactory.UnitXCurve(10);
            Vector3d direction = Vector3d.XAxis;

            double originalLength = testCurve.GetLength();

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            double resultLength = resultCurve.GetLength();

            bool result = (resultLength - originalLength < 0.1) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void VerticalCurve_YAxis_Oriented()
        {
            Curve testCurve = CurvesFactory.UnitXCurve(10);
            Vector3d direction = Vector3d.YAxis;

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            Vector3d orientation = new Vector3d(resultCurve.PointAtEnd - resultCurve.PointAtStart);

            double difference = Vector3d.VectorAngle(orientation, direction);

            bool result = (1 - (Math.Abs(difference) % 90) < 0.1) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void VerticalCurve_XAxis_Oriented()
        {
            Curve testCurve = CurvesFactory.UnitXCurve(10);
            Vector3d direction = Vector3d.XAxis;

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            Vector3d orientation = new Vector3d(resultCurve.PointAtEnd - resultCurve.PointAtStart);

            double difference = Vector3d.VectorAngle(orientation, direction);

            bool result = (1 - (Math.Abs(difference) % 90) < 0.1) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Angle45Curve_YAxis_Oriented()
        {
            Curve testCurve = new LineCurve(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            Vector3d direction = Vector3d.YAxis;

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            Vector3d orientation = new Vector3d(resultCurve.PointAtEnd - resultCurve.PointAtStart);

            double difference = Vector3d.VectorAngle(orientation, direction);

            bool result = (1 - (Math.Abs(difference) % 90) < 0.1) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Angle45CurveReversed_YAxis_Oriented()
        {
            Curve testCurve = new LineCurve(new Point3d(1, 1, 0), new Point3d(0, 01, 0));
            Vector3d direction = Vector3d.YAxis;

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            Vector3d orientation = new Vector3d(resultCurve.PointAtEnd - resultCurve.PointAtStart);

            double difference = Vector3d.VectorAngle(orientation, direction);

            bool result = (1 - (Math.Abs(difference) % 90) < 0.1) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Angle45Curve_XAxis_Oriented()
        {
            Curve testCurve = new LineCurve(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            Vector3d direction = Vector3d.XAxis;

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            Vector3d orientation = new Vector3d(resultCurve.PointAtEnd - resultCurve.PointAtStart);

            double difference = Vector3d.VectorAngle(orientation, direction);

            bool result = (1 - (Math.Abs(difference) % 90) < 0.1) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Angle45CurveReversed_XAxis_Oriented()
        {
            Curve testCurve = new LineCurve(new Point3d(1, 1, 0), new Point3d(0, 0, 0));
            Vector3d direction = Vector3d.XAxis;

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            Vector3d orientation = new Vector3d(resultCurve.PointAtEnd - resultCurve.PointAtStart);

            double difference = Vector3d.VectorAngle(orientation, direction);

            bool result = (1 - (Math.Abs(difference) % 90) < 0.1) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Angle45Curve_Angle45_Oriented()
        {
            Curve testCurve = new LineCurve(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            Vector3d direction = new Vector3d(1, 1, 0);

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            Vector3d orientation = new Vector3d(resultCurve.PointAtEnd - resultCurve.PointAtStart);

            double difference = Vector3d.VectorAngle(orientation, direction);

            bool result = (1 - (Math.Abs(difference) % 90) < 0.1) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Angle45CurveReversed_Angle45_Oriented()
        {
            Curve testCurve = new LineCurve(new Point3d(1, 1, 0), new Point3d(0, 0, 0));
            Vector3d direction = new Vector3d(1, 1, 0);

            Curve resultCurve = Logic.Transformations.Curves.RebuildPerpendicularTo(testCurve, direction);

            Vector3d orientation = new Vector3d(resultCurve.PointAtEnd - resultCurve.PointAtStart);

            double difference = Vector3d.VectorAngle(orientation, direction);

            bool result = (1 - (Math.Abs(difference) % 90) < 0.1) ? true : false;

            Assert.IsTrue(result);
        }
    }

    [TestClass]
    public class TrimWithClosedCurveTests
    {
        [TestMethod]
        public void UnitCircle_TransverseCurve_CorrectLength()
        {
            Curve unitCircle = new Circle(Point3d.Origin, 0.5).ToNurbsCurve();
            Curve transverseCurve = CurvesFactory.CenteredXCurve(2);

            Curve resultCurve = Logic.Transformations.Curves.TrimWithClosedCurve(unitCircle, transverseCurve);

            Assert.AreEqual(1, resultCurve.GetLength());
        }

        [TestMethod]
        public void UnitCircle_TrimmedCurve_CurveUnchanged()
        {
            Curve unitCircle = new Circle(Point3d.Origin, 0.5).ToNurbsCurve();
            Curve transverseCurve = CurvesFactory.CenteredXCurve(1);

            double originalLength = transverseCurve.GetLength();

            Curve resultCurve = Logic.Transformations.Curves.TrimWithClosedCurve(unitCircle, transverseCurve);

            Assert.AreEqual(originalLength, resultCurve.GetLength());
        }

        [TestMethod]
        public void UnitCircle_DisjointCurve_CurveUnchanged()
        {
            Curve unitCircle = new Circle(Point3d.Origin, 0.5).ToNurbsCurve();
            Curve disjointCurve = new LineCurve(new Point2d(5, 5), new Point2d(5, 10));

            double originalLength = disjointCurve.GetLength();

            Curve resultCurve = Logic.Transformations.Curves.TrimWithClosedCurve(unitCircle, disjointCurve);

            Assert.AreEqual(originalLength, resultCurve.GetLength());
        }

        [TestMethod]
        public void UnitCircle_UnitRectangle_InvalidInput()
        {
            Curve unitCircle = new Circle(Point3d.Origin, 0.5).ToNurbsCurve();
            Curve unitRectangle = new Rectangle3d(Plane.WorldXY, new Interval(-0.5, 0.5), new Interval(-0.5, 0.5)).ToNurbsCurve();

            double originalLength = unitRectangle.GetLength();

            Curve resultCurve = Logic.Transformations.Curves.TrimWithClosedCurve(unitCircle, unitRectangle);

            Assert.AreEqual(originalLength, resultCurve.GetLength());
        }

        [TestMethod]
        public void UnitXCurve_UnitYCurve_InvalidInput()
        {
            Curve unitX = CurvesFactory.CenteredXCurve(2);
            Curve unitY = CurvesFactory.CenteredYCurve(2);

            Curve resultCurve = Logic.Transformations.Curves.TrimWithClosedCurve(unitX, unitY);

            double oldStartY = unitY.PointAtStart.Y;
            double newStartY = resultCurve.PointAtStart.Y;

            Assert.AreEqual(oldStartY, newStartY);
        }
    }
}
