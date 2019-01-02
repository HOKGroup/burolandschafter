using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Geometry;
using HOK.OfficeManager.Factory;

namespace HOK.OfficeManager.Tests.Transformations.Breps
{
    [TestClass]
    public class SplitTwoBrepsTests
    {
        [TestMethod]
        public void CircleInSquare_ReturnsTwo()
        {
            List<Brep> TestEnv = BrepFactory.CircleInSquare();

            List<Brep> resultGeometry = Logic.Transformations.Breps.SplitTwoBreps(TestEnv[1], TestEnv[0]);

            Assert.AreEqual(resultGeometry.Count, 2);
        }

        [TestMethod]
        public void TwoDisjointSquares_ReturnsNone()
        {
            List<Brep> TestEnv = BrepFactory.TwoDisjointSquares();

            Console.WriteLine(TestEnv.Count);

            List<Brep> resultGeometry = Logic.Transformations.Breps.SplitTwoBreps(TestEnv[0], TestEnv[1]);

            Assert.AreEqual(resultGeometry.Count, 0);
        }
    }

    [TestClass]
    public class SplitByCurves
    {
        [TestMethod]
        public void UnitSquare_CrossHair_ReturnsFour()
        {
            Brep testSquare = Brep.CreatePlanarBreps(CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1), 0.1)[0];
            List<Curve> testCurves = new List<Curve>();
            testCurves.Add(CurvesFactory.CenteredYCurve());
            testCurves.Add(CurvesFactory.CenteredXCurve());

            List<Brep> resultGeometry = Logic.Transformations.Breps.SplitByCurves(testSquare, testCurves);

            Assert.AreEqual(4, resultGeometry.Count);
        }

        [TestMethod]
        public void UnitSquare_DisjointCurve_ReturnsOne()
        {
            Brep testSquare = Brep.CreatePlanarBreps(CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1), 0.1)[0];
            List<Curve> testCurves = new List<Curve>();
            testCurves.Add(CurvesFactory.LineSDL(new Point2d(10, 10), new Vector2d(1, 1), 1));

            List<Brep> resultGeometry = Logic.Transformations.Breps.SplitByCurves(testSquare, testCurves);

            Assert.AreEqual(1, resultGeometry.Count);
        }

        [TestMethod]
        public void UnitSquare_ContainedCurve_ReturnsOne()
        {
            Brep testSquare = Brep.CreatePlanarBreps(CurvesFactory.RectangleCWH(Point3d.Origin, 5, 5), 0.1)[0];
            List<Curve> testCurves = new List<Curve>();
            testCurves.Add(CurvesFactory.CenteredYCurve());

            List<Brep> resultGeometry = Logic.Transformations.Breps.SplitByCurves(testSquare, testCurves);

            Assert.AreEqual(1, resultGeometry.Count);
        }
    }
}
