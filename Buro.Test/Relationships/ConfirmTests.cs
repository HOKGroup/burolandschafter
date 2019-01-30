using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Geometry;
using HOK.Buro.Factory;
using HOK.Buro.Formats;
using HOK.Buro.Logic;

namespace HOK.Buro.Tests.Relationships.Confirm
{
    [TestClass]
    public class CoreIsSmallerTests
    {
        [TestMethod]
        public void CircleInSquare_CircleIsSmaller()
        {
            TestFitPackage TestEnv = TestFitFactory.DieFive(1);

            bool result = Logic.Relationships.Confirm.TestFit.CoreIsSmaller(TestEnv);

            Assert.IsTrue(result);
        }
    }

    [TestClass]
    public class VectorProportionIsVerticalTests
    {
        [TestMethod]
        public void UnitY_ReturnsTrue()
        {
            bool result = Logic.Relationships.Confirm.VectorProportionIsVertical(Vector3d.YAxis);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UnitX_ReturnsFalse()
        {
            bool result = Logic.Relationships.Confirm.VectorProportionIsVertical(Vector3d.XAxis);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SquareProportion_ReturnsFalse()
        {
            Vector3d testVector = new Vector3d(1, 1, 0);

            bool result = Logic.Relationships.Confirm.VectorProportionIsVertical(testVector);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TallProportion_ReturnsTrue()
        {
            Vector3d testVector = new Vector3d(1, 2, 0);

            bool result = Logic.Relationships.Confirm.VectorProportionIsVertical(testVector);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WideProportion_ReturnsFalse()
        {
            Vector3d testVector = new Vector3d(2, 1, 0);

            bool result = Logic.Relationships.Confirm.VectorProportionIsVertical(testVector);

            Assert.IsFalse(result);
        }
    }

    [TestClass]
    public class CurvesIntersectTests
    {
        [TestMethod]
        public void CenteredXCenteredY_ReturnsTrue()
        {
            Curve curveX = CurvesFactory.CenteredXCurve();
            Curve curveY = CurvesFactory.CenteredYCurve();

            bool result = Logic.Relationships.Confirm.CurvesIntersect(curveX, curveY, true);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TwoDisjointParallel_ReturnsFalse()
        {
            Curve leftCurve = new LineCurve(new Point2d(-1, 0), new Point2d(-1, 1));
            Curve rightCurve = new LineCurve(new Point2d(1, 0), new Point2d(1, 1));

            bool result = Logic.Relationships.Confirm.CurvesIntersect(leftCurve, rightCurve, true);

            Assert.IsFalse(result);
        }
    }

    [TestClass]
    public class RegionsIntersectTests
    {
        [TestMethod]
        public void CircleInSquare_ReturnsTrue()
        {
            List<Brep> TestEnv = BrepFactory.CircleInSquare();

            bool result = Logic.Relationships.Confirm.RegionsIntersect(TestEnv[0], TestEnv[1]);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SquareInCircle_ReturnsTrue()
        {
            List<Brep> TestEnv = BrepFactory.SquareInCircle();

            bool result = Logic.Relationships.Confirm.RegionsIntersect(TestEnv[0], TestEnv[1]);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CircleWithTransverseRectangle_ReturnsTrue()
        {
            List<Brep> TestEnv = BrepFactory.CircleWithTransverseRectangle();

            bool result = Logic.Relationships.Confirm.RegionsIntersect(TestEnv[0], TestEnv[1]);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SquareWithInscribedCircle_ReturnsTrue()
        {
            List<Brep> TestEnv = BrepFactory.SqaureWithInscribedCircle();

            bool result = Logic.Relationships.Confirm.RegionsIntersect(TestEnv[0], TestEnv[1]);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TwoDisjointSquares_ReturnsFalse()
        {
            List<Brep> TestEnv = BrepFactory.TwoDisjointSquares();

            bool result = Logic.Relationships.Confirm.RegionsIntersect(TestEnv[0], TestEnv[1]);

            Assert.IsFalse(result);
        }
    }

    [TestClass]
    public class AllAxisColinearTests
    {
        [TestMethod]
        public void TwoDisjointUnitY_ReturnsFalse()
        {
            List<Curve> TestEnv = new List<Curve>();
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(1, 0), new Vector2d(0, 1), 1));
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(-1, 0), new Vector2d(0, 1), 1));

            bool result = Logic.Relationships.Confirm.AllAxisColinear(TestEnv);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TwoColinearUnitY_ReturnsTrue()
        {
            List<Curve> TestEnv = new List<Curve>();
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(0, 0), new Vector2d(0, 1), 1));
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(0, 0), new Vector2d(0, -1), 1));

            bool result = Logic.Relationships.Confirm.AllAxisColinear(TestEnv);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ThreeDisjointNECurves_ReturnsFalse()
        {
            List<Curve> TestEnv = new List<Curve>();
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(0, 0), new Vector2d(1, 1), 1));
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(2, 0), new Vector2d(1, 1), 1));
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(4, 0), new Vector2d(1, 1), 1));

            bool result = Logic.Relationships.Confirm.AllAxisColinear(TestEnv);

            Assert.IsFalse(result);
        }
    }

    [TestClass]
    public class SegmentsFormOneCurveTests
    {
        [TestMethod]
        public void TwoDisjointUnitY_ReturnsFalse()
        {
            List<Curve> TestEnv = new List<Curve>();
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(1, 0), new Vector2d(0, 1), 1));
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(-1, 0), new Vector2d(0, 1), 1));

            bool result = Logic.Relationships.Confirm.SegmentsFormOneCurve(TestEnv);

            Assert.IsFalse(result);
        }

        //[TestMethod]
        public void VSegments_ReturnsTrue()
        {
            List<Curve> TestEnv = new List<Curve>();
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(0, 0), new Vector2d(1, 1), 1));
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(0, 0), new Vector2d(-1, 1), 1));

            bool result = Logic.Relationships.Confirm.SegmentsFormOneCurve(TestEnv);

            Assert.IsTrue(result);
        }

        //[TestMethod]
        public void ThreeHorizontalInSequence_ReturnsTrue()
        {
            List<Curve> TestEnv = new List<Curve>();
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(0, 0), new Vector2d(1, 0), 1));
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(1, 0), new Vector2d(1, 0), 1));
            TestEnv.Add(CurvesFactory.LineSDL(new Point2d(2, 0), new Vector2d(1, 0), 1));

            bool result = Logic.Relationships.Confirm.SegmentsFormOneCurve(TestEnv);

            Assert.IsTrue(result);
        }
    }

    [TestClass]
    public class CurveRegionIntersectionTests
    {
        [TestMethod]
        public void OverlappingSquareAndSquareRegion_ReturnsTrue()
        {
            Curve testCurve = CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1);
            Brep testRegion = Brep.CreatePlanarBreps(testCurve, 0.1)[0];

            bool result = Logic.Relationships.Confirm.CurveRegionIntersection(testCurve, testRegion);

            Assert.IsTrue(result);
        }

        //[TestMethod]
        public void DisjointSquareAndSquareRegion_ReturnsFalse()
        {
            Curve testCurve = CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1);
            Curve regionCurve = CurvesFactory.RectangleCWH(new Point3d(5, 5, 0), 1, 1);
            Brep testRegion = Brep.CreatePlanarBreps(regionCurve, 0.1)[0];

            bool result = Logic.Relationships.Confirm.CurveRegionIntersection(testCurve, testRegion);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdjacentSquareAndSquareRegion_ReturnsTrue()
        {
            Curve testCurve = CurvesFactory.RectangleCWH(new Point3d(1, 0, 0), 2, 2);
            Curve regionCurve = CurvesFactory.RectangleCWH(new Point3d(-1, 0, 0), 2, 2);
            Brep testRegion = Brep.CreatePlanarBreps(regionCurve, 0.1)[0];

            bool result = Logic.Relationships.Confirm.CurveRegionIntersection(testCurve, testRegion);

            Assert.IsTrue(result);
        }
    }

    [TestClass]
    public class PointInRegionTests
    {
        [TestMethod]
        public void PointInsideUnitSquare_ReturnsTrue()
        {
            Brep testRegion = Brep.CreatePlanarBreps(CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1), 0.1)[0];
            Point3d testPoint = Point3d.Origin;

            bool result = Logic.Relationships.Confirm.PointInRegion(testRegion, testPoint);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PointCoincidentUnitSquare_ReturnsTrue()
        {
            Brep testRegion = Brep.CreatePlanarBreps(CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1), 0.1)[0];
            Point3d testPoint = new Point3d(0.5, 0.5, 0);

            bool result = Logic.Relationships.Confirm.PointInRegion(testRegion, testPoint);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PointOutsideUnitSquare_ReturnsFalse()
        {
            Brep testRegion = Brep.CreatePlanarBreps(CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1), 0.1)[0];
            Point3d testPoint = new Point3d(5, 5, 0);

            bool result = Logic.Relationships.Confirm.PointInRegion(testRegion, testPoint);

            Assert.IsFalse(result);
        }
    }

    [TestClass]
    public class TargetFulfilledTests
    {
        [TestMethod]
        public void IsNotFulfilled_ReturnsFalse()
        {
            var testQuota = new List<int>(new []{5, 10, 0, 4});
            var testFilled = new List<int>(new[] {6, 2, 0, 1});

            var result = Logic.Relationships.Confirm.Zone.TargetFulfilled(testFilled, testQuota);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsFulfilled_ReturnsTrue()
        {
            var testQuota = new List<int>(new[] { 5, 10, 0, 4 });
            var testFilled = new List<int>(new[] { 5, 10, 0, 4 });

            var result = Logic.Relationships.Confirm.Zone.TargetFulfilled(testFilled, testQuota);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsOverfilled_ReturnsTrue()
        {
            var testQuota = new List<int>(new[] { 5, 10, 0, 4 });
            var testFilled = new List<int>(new[] { 50, 50, 50, 50 });

            var result = Logic.Relationships.Confirm.Zone.TargetFulfilled(testFilled, testQuota);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void OnlyOneUnfulfilled_ReturnsFalse()
        {
            var testQuota = new List<int>(new[] { 5, 10, 0, 4 });
            var testFilled = new List<int>(new[] { 5, 10, 0, 3 });

            var result = Logic.Relationships.Confirm.Zone.TargetFulfilled(testFilled, testQuota);

            Assert.IsFalse(result);
        }
    }
}
