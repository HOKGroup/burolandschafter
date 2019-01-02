using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Factory;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;

using HOK.OfficeManager.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace HOK.OfficeManager.Tests.Utils
{
    [TestClass]
    public class GetSlopeTests
    {
        [TestMethod]
        public void UnitY_IsUndefined()
        {
            Curve testCurve = CurvesFactory.CenteredYCurve();

            double slope = Logic.Utils.GetSlope(testCurve);

            Assert.AreEqual(-1, slope);
        }

        [TestMethod]
        public void UnitX_IsZero()
        {
            Curve testCurve = CurvesFactory.CenteredXCurve();

            double slope = Logic.Utils.GetSlope(testCurve);

            Assert.AreEqual(0, slope);
        }

        [TestMethod]
        public void CurveSE_IsNegativeOne()
        {
            Curve testCurve = CurvesFactory.LineSDL(Point2d.Origin, new Vector2d(1, -1), 1);

            double slope = Logic.Utils.GetSlope(testCurve);

            Assert.AreEqual(-1, slope);
        }

        [TestMethod]
        public void CurveNE_IsOne()
        {
            Curve testCurve = CurvesFactory.LineSDL(Point2d.Origin, new Vector2d(1, 1), 1);

            double slope = Logic.Utils.GetSlope(testCurve);

            Assert.AreEqual(1, slope);
        }

        [TestMethod]
        public void CurveNW_IsNegativeOne()
        {
            Curve testCurve = CurvesFactory.LineSDL(Point2d.Origin, new Vector2d(-1, 1), 1);

            double slope = Logic.Utils.GetSlope(testCurve);

            Assert.AreEqual(-1, slope);
        }

        [TestMethod]
        public void CurveSW_IsOne()
        {
            Curve testCurve = CurvesFactory.LineSDL(Point2d.Origin, new Vector2d(-1, -1), 1);

            double slope = Logic.Utils.GetSlope(testCurve);

            Assert.AreEqual(1, slope);
        }
    }

    [TestClass]
    public class GetRectangleDomainTests
    {
        [TestMethod]
        public void CenteredUnitRectangle_WidthIsOne()
        {
            Curve testRect = CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1);

            var result = Logic.Utils.DeconstructRectangle(testRect);

            Assert.AreEqual(1, result.Width);
        }

        [TestMethod]
        public void CenteredUnitRectangle_HeightIsOne()
        {
            Curve testRect = CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1);

            var result = Logic.Utils.DeconstructRectangle(testRect);

            Assert.AreEqual(1, result.Height);
        }

        [TestMethod]
        public void CenteredUnitRectangle_AreaIsOne()
        {
            Curve testRect = CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1);

            var result = Logic.Utils.DeconstructRectangle(testRect);

            Assert.AreEqual(1, result.Area);
        }

        [TestMethod]
        public void NonCenteredUnitRectangle_WidthIsOne()
        {
            Curve testRect = CurvesFactory.RectangleCWH(new Point3d(5, 5, 0), 1, 1);

            var result = Logic.Utils.DeconstructRectangle(testRect);

            Assert.AreEqual(1, result.Width);
        }

        [TestMethod]
        public void NonCenteredUnitRectangle_HeightIsOne()
        {
            Curve testRect = CurvesFactory.RectangleCWH(new Point3d(5, 5, 0), 1, 1);

            var result = Logic.Utils.DeconstructRectangle(testRect);

            Assert.AreEqual(1, result.Height);
        }

        [TestMethod]
        public void NonCenteredUnitRectangle_AreaIsOne()
        {
            Curve testRect = CurvesFactory.RectangleCWH(new Point3d(5, 5, 0), 1, 1);

            var result = Logic.Utils.DeconstructRectangle(testRect);

            Assert.AreEqual(1, result.Area);
        }
    }

    [TestClass]
    public class GetAllCurveIntersectionsTests
    {
        [TestMethod]
        public void UnitX_TenUnitY_ReturnsTen()
        {
            Curve testBaseCurve = CurvesFactory.LineSDL(Point2d.Origin, new Vector2d(1, 0), 20);
            List<Curve> testIntersectionCurves = new List<Curve>();

            for (int i = 0; i < 10; i++)
            {
                testIntersectionCurves.Add(CurvesFactory.LineSDL(new Point2d(i, -1), new Vector2d(0, 1), 5));
            }

            //Console.WriteLine("TestEnv done.");

            List<Point3d> resultGeometry = Logic.Utils.GetAllCurveIntersections(testBaseCurve, testIntersectionCurves, true);

            Assert.AreEqual(10, resultGeometry.Count);
        }

        [TestMethod]
        public void UnitXTenUnitY_ReturnsTen()
        {
            Curve testBaseCurve = CurvesFactory.LineSDL(new Point2d(-1, 0), new Vector2d(1, 0), 20);
            List<Curve> testIntersectionCurves = new List<Curve>();

            for (int i = 0; i < 10; i++)
            {
                testIntersectionCurves.Add(new LineCurve(new Point2d(i, -2), new Point2d(i, 2)));
                //testIntersectionCurves.Add(CurvesFactory.LineSDL(new Point2d(i*2, -1), new Vector2d(0, 1), 5));
            }

            testIntersectionCurves.Add(testBaseCurve);

            List<Point3d> resultGeometry = Logic.Utils.GetAllCurveIntersections(testIntersectionCurves, true);

            Assert.AreEqual(10, resultGeometry.Count);
        }
    }

    [TestClass]
    public class RoundWithinTotalTests
    {
        [TestMethod]
        public void HalfStepSeries_Sum5_ResultSumIs5()
        {
            List<double> testValues = new List<double>(new[] {0, 0.5, 1, 1.5, 2}.ToArray());

            List<int> roundedValues = Logic.Utils.RoundWithinTotal(testValues, 5);

            Assert.AreEqual(5, roundedValues.Sum());
        }

        [TestMethod]
        public void LargeHalfStepSeries_ResultSumIsSame()
        {
            List<double> testValues = new List<double>();

            for (int i = 0; i < 33; i++)
            {
                testValues.Add(i * 0.5);
            }

            int baseSum = Convert.ToInt32(testValues.Sum());

            List<int> roundedValues = Logic.Utils.RoundWithinTotal(testValues, baseSum);

            Assert.AreEqual(baseSum, roundedValues.Sum());
        }
    }

    [TestClass]
    public class InstancesInListTests
    {
        [TestMethod]
        public void String_NoInstances_ReturnsZero()
        {
            var testList = new List<string>(new[] {"S", "I", "X", "S", "M", "I", "T", "H"});
            var testKey = "Z";

            var result = Logic.Utils.InstancesInList(ref testList, ref testKey);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void String_TwoInstances_ReturnsTwo()
        {
            var testList = new List<string>(new[] { "S", "I", "X", "S", "M", "I", "T", "H" });
            var testKey = "S";

            var result = Logic.Utils.InstancesInList(ref testList, ref testKey);

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void Int_NoInstances_ReturnsZero()
        {
            var testList = new List<int>(new[] { 8, 6, 7, 5, 3, 0, 9 });
            var testKey = 1;

            var result = Logic.Utils.InstancesInList(ref testList, ref testKey);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Int_OneInstance_ReturnsOne()
        {
            var testList = new List<int>(new[] { 8, 6, 7, 5, 3, 0, 9 });
            var testKey = 9;

            var result = Logic.Utils.InstancesInList(ref testList, ref testKey);

            Assert.AreEqual(1, result);
        }
    }

    [TestClass]
    public class NestedListCountTests
    {
        [TestMethod]
        public void TwoListsOfTwoItems_Ints_ReturnsFour()
        {
            var testList = new List<List<int>>();

            testList.Add(new List<int>(new []{1, 5}));
            testList.Add(new List<int>(new []{5, 27}));

            var result = Logic.Utils.NestedListCount(ref testList);

            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void OneListOfTenItems_Strings_ReturnsTen()
        {
            var testList = new List<List<int>>();

            testList.Add(new List<int>(new[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 }));

            var result = Logic.Utils.NestedListCount(ref testList);

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void TenListsOfTenItems_Ints_ReturnsOneHundred()
        {
            var testList = new List<List<int>>();

            for (int i = 0; i < 10; i++)
            {
                testList.Add(new List<int>(new []{10, 10, 10, 10, 10, 10, 10, 10, 10, 10}));
            }

            var result = Logic.Utils.NestedListCount(ref testList);

            Assert.AreEqual(100, result);
        }

        [TestMethod]
        public void TenListsOfIncreasingSize_Ints_ReturnsFiftyFive()
        {
            var testList = new List<List<int>>();

            for (int i = 0; i < 10; i++)
            {
                var listToAdd = new List<int>();

                for (int j = 0; j < i + 1; j++)
                {
                    listToAdd.Add(j);
                }

                testList.Add(listToAdd);
            }

            var result = Logic.Utils.NestedListCount(ref testList);

            Assert.AreEqual(55, result);
        }
    }

    [TestClass]
    public class GetBoundingBoxCurveTests
    {
        [TestMethod]
        public void UnitCircle_ReturnsValue()
        {
            var testBrep = BrepFactory.CircleInSquare()[0];

            var resultGeometry = Logic.Utils.GetBoundingBoxCurve(testBrep);

            Assert.IsNotNull(resultGeometry);
        }

        [TestMethod]
        public void UnitCircle_ReturnsQuad()
        {
            var testBrep = BrepFactory.CircleInSquare()[0];

            var resultGeometry = Logic.Utils.GetBoundingBoxCurve(testBrep);

            Assert.AreEqual(4, resultGeometry.SpanCount);
        }
    }

    [TestClass]
    public class GetLongestCurvesTests
    {
        [TestMethod]
        public void TwoLinearCurves_ReturnsLongest()
        {
            var testList = new List<Curve>();
            testList.Add(CurvesFactory.UnitXCurve(5));
            testList.Add(CurvesFactory.UnitXCurve(3));

            var resultGeometry = Logic.Utils.GetLongestCurve(testList);

            Assert.AreEqual(5, resultGeometry.GetLength());
        }

        [TestMethod]
        public void ThreeLinearCurves_ReturnsLongest()
        {
            var testList = new List<Curve>();
            testList.Add(CurvesFactory.UnitXCurve(5));
            testList.Add(CurvesFactory.UnitXCurve(3));
            testList.Add(CurvesFactory.UnitXCurve(1));

            var resultGeometry = Logic.Utils.GetLongestCurve(testList);

            Assert.AreEqual(5, resultGeometry.GetLength());
        }

        [TestMethod]
        public void MultipleVariedCurves_ReturnsLongest()
        {
            var testList = new List<Curve>();

            for (int i = 0; i < 15; i++)
            {
                testList.Add(CurvesFactory.RectangleCWH(Point3d.Origin, i + 1, i + 1));
                testList.Add(CurvesFactory.UnitXCurve(i + 1));
                testList.Add(new Arc(Plane.WorldXY, Point3d.Origin, i + 1, Math.PI / 2).ToNurbsCurve());
            }

            var resultGeometry = Logic.Utils.GetLongestCurve(testList);

            Assert.AreEqual(15 * 4, resultGeometry.GetLength());
        }
    }
}