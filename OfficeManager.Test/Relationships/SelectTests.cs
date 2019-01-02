//using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;

using HOK.OfficeManager.Logic;
using HOK.OfficeManager.Factory;
using HOK.OfficeManager.Logic.Transformations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Assert = NUnit.Framework.Assert;

namespace HOK.OfficeManager.Tests.Relationships.Select

{
    [TestClass]
    public class NotExemptRegionsTests
    {
        [TestMethod]
        public void SquareInCircle_SelectsOne()
        {
            List<Brep> TestEnv = BrepFactory.SquareInCircle();

            List<Brep> testSquare = new List<Brep>();
            testSquare.Add(TestEnv[0]);

            List<Brep> testCircle = new List<Brep>();
            testCircle.Add(TestEnv[1]);

            List<Brep> resultGeometry = Logic.Relationships.Select.NotExemptRegions(testCircle, testSquare);

            Assert.AreEqual(resultGeometry.Count, 1);
        }

        [TestMethod]
        public void SquareInCircle_SelectsCircle()
        {
            List<Brep> TestEnv = BrepFactory.SquareInCircle();

            List<Brep> testSquare = new List<Brep>();
            testSquare.Add(TestEnv[0]);

            List<Brep> testCircle = new List<Brep>();
            testCircle.Add(TestEnv[1]);

            List<Brep> resultGeometry = Logic.Relationships.Select.NotExemptRegions(testCircle, testSquare);

            bool result = (resultGeometry[0].GetArea() > TestEnv[0].GetArea()) ? true : false;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SquareInCircle_RemovesAreaFromCircle()
        {
            List<Brep> TestEnv = BrepFactory.SquareInCircle();

            List<Brep> testSquare = new List<Brep>();
            testSquare.Add(TestEnv[0]);

            List<Brep> testCircle = new List<Brep>();
            testCircle.Add(TestEnv[1]);

            List<Brep> resultGeometry = Logic.Relationships.Select.NotExemptRegions(testCircle, testSquare);

            double calculated = TestEnv[1].GetArea() - TestEnv[0].GetArea();
            double actual = resultGeometry[0].GetArea();

            bool result = (actual - calculated < 0.1) ? true : false;

            Assert.IsTrue(result);
        }
    }

    [TestClass]
    public class FloorFromCoreTests
    {
        [TestMethod]
        public void TwoIntersectingCircles_SelectsOne()
        {
            List<Brep> TestEnv = BrepFactory.TwoIntersectingCircles();
            Brep testCoreLeft = TestEnv[0];
            Brep testCoreRight = TestEnv[1];

            List<Brep> resultGeometryA = Logic.Relationships.Select.FloorFromCore(TestEnv, testCoreLeft);
            Assert.AreEqual(resultGeometryA.Count, 1);

            List<Brep> resultGeometryB = Logic.Relationships.Select.FloorFromCore(TestEnv, testCoreRight);
            Assert.AreEqual(resultGeometryB.Count, 1);
        }

        [TestMethod]
        public void TwoIntersectingCircles_NotCore()
        {
            List<Brep> TestEnv = BrepFactory.TwoIntersectingCircles();
            Brep testCircleLeft = TestEnv[0];
            Brep testCircleRight = TestEnv[1];

            List<Brep> resultGeometry = Logic.Relationships.Select.FloorFromCore(TestEnv, testCircleLeft);

            Point3d selectedZoneCenter = Logic.Utils.GetRegionCenter(resultGeometry[0]);
            Point3d leftCircleCenter = Logic.Utils.GetRegionCenter(testCircleLeft);
            Point3d rightCircleCenter = Logic.Utils.GetRegionCenter(testCircleRight);

            bool InitialIsCloser = (selectedZoneCenter.DistanceTo(leftCircleCenter) > selectedZoneCenter.DistanceTo(rightCircleCenter)) ? true : false;

            Assert.IsTrue(InitialIsCloser);
        }

        [TestMethod]
        public void CircleWithTransverseRectangle_SelectsTwo()
        {
            List<Brep> TestEnv = BrepFactory.CircleWithTransverseRectangle();
            Brep testCircle = TestEnv[0];
            Brep testRectangle = TestEnv[1];

            List<Brep> allRegions = Breps.SplitTwoBreps(testCircle, testRectangle);
            List<Brep> resultGeometry = Logic.Relationships.Select.FloorFromCore(allRegions, testRectangle);

            Assert.AreEqual(resultGeometry.Count, 2);
        }

        [TestMethod]
        public void CircleWithTransverseRectangle_NotCore()
        {
            List<Brep> TestEnv = BrepFactory.CircleWithTransverseRectangle();
            Brep testCircle = TestEnv[0];
            Brep testRectangle = TestEnv[1];

            List<Brep> allRegions = Breps.SplitTwoBreps(testCircle, testRectangle);
            List<Brep> resultGeometry = Logic.Relationships.Select.FloorFromCore(allRegions, testRectangle);

            Point3d coreCenter = Logic.Utils.GetRegionCenter(testRectangle);

            foreach (Brep region in resultGeometry)
            {
                Point3d regionCenter = Logic.Utils.GetRegionCenter(region);

                if (regionCenter.DistanceTo(coreCenter) < .05)
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void SquareWithInscribedCircle_SelectsFour()
        {
            List<Brep> TestEnv = BrepFactory.SqaureWithInscribedCircle();
            Brep testCircle = TestEnv[1];
            Brep testSquare = TestEnv[0];

            List<Brep> allRegions = Breps.SplitTwoBreps(testSquare, testCircle);

            Console.WriteLine(testCircle.Curves2D[0].IsCircle());
            Console.WriteLine(testSquare.GetArea());
            Console.WriteLine(testCircle.GetArea());
            Console.WriteLine(allRegions.Count.ToString());

            List<Brep> resultGeometry = Logic.Relationships.Select.FloorFromCore(allRegions, testCircle);

            Assert.AreEqual(resultGeometry.Count, 4);
        }

        [TestMethod]
        public void SquareWithCircumscribedCircle_SelectsFour()
        {
            List<Brep> TestEnv = BrepFactory.SquareWithCircumscribedCircle();
            Brep testCircle = TestEnv[1];
            Brep testSquare = TestEnv[0];

            List<Brep> allRegions = Breps.SplitTwoBreps(testCircle, testSquare);
            List<Brep> resultGeometry = Logic.Relationships.Select.FloorFromCore(allRegions, testSquare);

            Console.WriteLine(allRegions.Count);
            Console.WriteLine(testSquare.GetArea());
            Console.WriteLine(testCircle.GetArea());

            Assert.AreEqual(resultGeometry.Count, 4);
        }

        [TestMethod]
        public void SquareInCircle_SelectsOne()
        {
            List<Brep> TestEnv = BrepFactory.SquareInCircle();
            Brep testSquare = TestEnv[0];
            Brep testCircle = TestEnv[1];

            List<Brep> allRegions = Breps.SplitTwoBreps(testCircle, testSquare);
            List<Brep> resultGeometry = Logic.Relationships.Select.FloorFromCore(allRegions, testSquare);

            Assert.AreEqual(resultGeometry.Count, 1);
        }

        [TestMethod]
        public void SquareInCircle_NotCore()
        {
            List<Brep> TestEnv = BrepFactory.SquareInCircle();
            Brep testSquare = TestEnv[0];
            Brep testCircle = TestEnv[1];

            List<Brep> allRegions = Breps.SplitTwoBreps(testCircle, testSquare);
            List<Brep> resultGeometry = Logic.Relationships.Select.FloorFromCore(allRegions, testSquare);

            double resultArea = resultGeometry[0].GetArea();
            double profileArea = testCircle.GetArea();
            double coreArea = testSquare.GetArea();

            double difference = (profileArea - coreArea) - resultArea;
            bool tolerated = (difference < 0.01) ? true : false;

            Assert.IsTrue(tolerated);
        }

        [TestMethod]
        public void CircleInSquare_SelectsOne()
        {
            List<Brep> TestEnv = BrepFactory.CircleInSquare();
            Brep testCircle = TestEnv[0];
            Brep testSquare = TestEnv[1];

            List<Brep> allRegions = Breps.SplitTwoBreps(testSquare, testCircle);
            List<Brep> resultGeometry = Logic.Relationships.Select.FloorFromCore(allRegions, testCircle);

            Assert.AreEqual(resultGeometry.Count, 1);
        }

        [TestMethod]
        public void CircleInSquare_NotCore()
        {
            List<Brep> TestEnv = BrepFactory.CircleInSquare();
            Brep testCircle = TestEnv[0];
            Brep testSquare = TestEnv[1];

            List<Brep> allRegions = Breps.SplitTwoBreps(testSquare, testCircle);
            List<Brep> resultGeometry = Logic.Relationships.Select.FloorFromCore(allRegions, testCircle);

            double resultArea = resultGeometry[0].GetArea();
            double profileArea = testSquare.GetArea();
            double coreArea = testCircle.GetArea();

            double difference = (profileArea - coreArea) - resultArea;
            bool tolerated = (difference < 0.01) ? true : false;

            Assert.IsTrue(tolerated);
        }
    }
}
