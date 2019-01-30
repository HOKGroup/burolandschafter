using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using HOK.Buro.Formats;

namespace HOK.Buro.Logic.Relationships
{
    public class Confirm
    {
        //Tests that require user input rhino geometry happen here.

        public class TestFit
        {
            //Tests specifically about fidelity of the TestFit problem proposed.

            public static bool Fidelity(TestFitPackage tf)
            {
                List<bool> testResults = new List<bool>();

                //Tests may get a little redundant here for the sake of a readable list of restrictions.
                testResults.Add(MainProfileIsClosed(tf));
                testResults.Add(CoreProfileIsClosed(tf));
                testResults.Add(CoreIsSmaller(tf));
                testResults.Add(AllStructureCurvesAreClosed(tf));

                return true;
            }

            public static bool MainProfileIsClosed(TestFitPackage tf)
            {
                return tf.FloorPlanPackage.FloorProfile.IsClosed;
            }

            public static bool CoreProfileIsClosed(TestFitPackage tf)
            {
                return tf.FloorPlanPackage.CoreProfile.IsClosed;
            }

            public static bool CoreIsSmaller(TestFitPackage tf)
            {
                Brep floor = Brep.CreatePlanarBreps(tf.FloorPlanPackage.FloorProfile)[0];
                Brep core = Brep.CreatePlanarBreps(tf.FloorPlanPackage.CoreProfile)[0];

                double floorArea = floor.GetArea();
                double coreArea = core.GetArea();

                bool isSmaller = (coreArea < floorArea) ? true : false;

                return isSmaller;
            }

            public static bool AllStructureCurvesAreClosed(TestFitPackage tf)
            {
                List<Curve> curvesToCheck = tf.FloorPlanPackage.StructureProfiles;

                foreach (Curve curve in curvesToCheck)
                {
                    if (curve.IsClosed != true)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public class Program
        {
            public static bool RequestIsPossible(List<ProgramPackage> programs, double totalArea)
            {
                double requestedArea = 0;

                foreach (ProgramPackage program in programs)
                {
                    var rectInfo = new CurveBounds(program.OccupationBoundary);
                    double area = rectInfo.Area;

                    requestedArea = requestedArea + (area * program.Quota);
                }

                bool possible = (requestedArea < totalArea) ? true : false;

                return possible;
            }

            public static int MaximizedCount(List<ProgramPackage> programs)
            {
                var maxCount = 0;

                foreach (ProgramPackage program in programs)
                {
                    if (program.Quota == 0)
                    {
                        maxCount++;
                    }
                }

                return maxCount;
            }

            public static List<double> Distribution(List<ProgramPackage> programs, double totalArea)
            {
                int maximizedCount = 0;
                double nonMaximizedUse = 0;

                foreach (ProgramPackage program in programs)
                {
                    if (program.Quota == 0)
                    {
                        maximizedCount++;
                    }
                    else
                    {
                        double quotaArea = Brep.CreatePlanarBreps(program.OccupationBoundary)[0].GetArea() * program.Quota;
                        double usage = quotaArea / totalArea;

                        nonMaximizedUse = nonMaximizedUse + usage;
                    }
                }

                double maximizedUse = (maximizedCount != 0) ? (1 - nonMaximizedUse) / maximizedCount : 0;

                List<double> distribution = new List<double>();

                for (int i = 0; i < programs.Count; i++)
                {
                    if (programs[i].Quota == 0)
                    {
                        distribution.Add(maximizedUse * 100);
                    }
                    else
                    {
                        double quotaArea = Brep.CreatePlanarBreps(programs[i].OccupationBoundary)[0].GetArea() * programs[i].Quota;
                        double usage = quotaArea / totalArea;

                        distribution.Add(usage * 100);
                    }
                }

                return distribution;
            }

            public static List<int> Affinities(List<ProgramPackage> programs)
            {
                //Intended to codify where a program item "prefers" to be. Offices near core, desks near perimeter, for example.
                //Not yet decided how many categories to have or how to input.
                //All programs currently default to 0, or no affinity.
                List<int> affinities = new List<int>();

                foreach (ProgramPackage program in programs)
                {
                    affinities.Add(0);
                }

                return affinities;
            }

            public static List<double> Difficulty(List<ProgramPackage> programs, List<Brep> floorRegions)
            {
                //Considers geometry of floor regions and quantifies how difficult it is to find space for an item.
                //Last iteration was only marginally better than "smaller = easier." Proportion is considered but is it worth the computation time?
                //Normalized on a scale from 0 to 1 where 0 is the easiest and 1 is not possible.
                List<double> difficulty = new List<double>();

                foreach (ProgramPackage program in programs)
                {
                    difficulty.Add(0);
                }

                return difficulty;
            }

            public static List<int> Priority(List<ProgramPackage> programs)
            {
                //Tiebreaker statistic for cases where other measurements don't have enough difference, or make impossible conflicts.
                //Maybe used if "not possible" solutions are still filled as best as possible?
                List<int> priority = new List<int>();

                for (int i = 0; i < programs.Count; i++)
                {
                    priority.Add(i);
                }

                return priority;
            }
        }

        public class Zone
        {
            public static bool IsCoreAdjacent(EdgeCurves edges)
            {
                bool result = (edges.CoreAdjacent.Count > 0) ? true : false;

                return result;
            }

            public static bool IsPerimeterAdjacent(EdgeCurves edges)
            {
                bool result = (edges.PerimeterAdjacent.Count > 0) ? true : false;

                return result;
            }

            public static bool TargetFulfilled(List<int> current, List<int> target)
            {
                for (int i = 0; i < current.Count; i++)
                {
                    if (current[i] < target[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public static bool RegionsIntersect(Brep regionA, Brep regionB)
        {
            //Depreciated: brep split operation is slow and run twice if this is used.
            //Prefer running it in all cases, without this confirm method, and just counting number of results.

            Brep[] splitOperation = regionA.Split(regionB, 0.1);

            bool splitOcurred = (splitOperation.Length == 0) ? false : true;

            //Compensation for when a region is completely contained within another.
            Point3d regionCenterA = Utils.GetRegionCenter(regionA);
            Point3d regionCenterB = Utils.GetRegionCenter(regionB);

            double distAtoB = regionCenterA.DistanceTo(regionB.ClosestPoint(regionCenterA));
            double distBtoA = regionCenterB.DistanceTo(regionA.ClosestPoint(regionCenterB));

            bool AinB = (distAtoB < 0.1) ? true : false;
            bool BinA = (distBtoA < 0.1) ? true : false;

            Console.WriteLine(AinB.ToString() + " / " + BinA.ToString());

            bool isContained = (AinB == true || BinA == true) ? true : false;

            //If any of the above are true, consider it an intersection.
            bool result = (splitOcurred == true || isContained == true) ? true : false;

            return result;
        }

        public static bool CurvesIntersect(Curve curveA, Curve curveB, bool includeOverlap)
        {
            if (curveA == null || curveB == null)
            {
                return false;
            }

            var ccx = Intersection.CurveCurve(curveA, curveB, 0.1, 0.1);

            var count = 0;

            for (int i = 0; i < ccx.Count; i++)
            {
                if (ccx[i].IsPoint)
                {
                    count++;
                }
                else if (ccx[i].IsOverlap && includeOverlap == true)
                {
                    count++;
                }
            }

            var result = count > 0;

            return result;
        }

        public static bool VectorProportionIsVertical(Vector3d vector)
        {
            bool result = Math.Abs(vector.Y) > Math.Abs(vector.X);

            return result;
        }

        public static bool AllAxisColinear(List<Curve> curves)
        {
            //First check if there's only one L to R slope.
            double refSlope = Utils.GetSlope(curves[0]);
            bool uniform = false;

            for (int i = 0; i < curves.Count; i++)
            {
                double delta = Utils.GetSlope(curves[i]) - refSlope;

                if (delta > 0.1)
                {
                    break;
                }
                else if (i == curves.Count - 1)
                {
                    uniform = true;
                }
            }

            if (uniform == false)
            {
                Console.WriteLine("Slopes are not uniform.");
                return false;
            }

            //If only one slope exists, verify alignment along that axis.
            List<double> xVals = new List<double>();
            List<double> yVals = new List<double>();

            foreach (Curve curve in curves)
            {
                xVals.Add(curve.PointAtStart.X);
                xVals.Add(curve.PointAtEnd.X);
                yVals.Add(curve.PointAtStart.Y);
                yVals.Add(curve.PointAtEnd.Y);
            }

            List<double> dirVals = (refSlope < 0.01) ? xVals : yVals;

            double avg = dirVals.Average();
            bool inline = false;

            for (int i = 0; i < dirVals.Count; i++)
            {
                Console.WriteLine(dirVals[i]);

                if (Math.Abs(dirVals[i] - avg) > 0.1)
                {
                    Console.WriteLine("Curves have uniform slope but are not aligned.");
                    break;
                }
                else if (i == dirVals.Count - 1)
                {
                    inline = true;
                }
            }

            return inline;
        }

        public static bool SegmentsFormOneCurve(List<Curve> curves)
        {
            return false;
            //Seems to be consistently returning nothing. Not spending time on figuring this out.

            /*
            Curve[] joinOperation = Curve.JoinCurves(curves);

            Console.WriteLine(curves.Count + " -> " + joinOperation.Length);

            if (joinOperation.Length == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
            */
        }

        public static bool CurveRegionIntersection(Curve crv, Brep region)
        {
            Rhino.Geometry.Intersect.Intersection.CurveBrep(crv, region, 0.1, out Curve[] curves, out Point3d[] pts);

            Console.WriteLine(curves.Length + " curves & " + pts.Length + " points.");

            if (pts.Length > 0 || curves.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool RegionIsNotDonut(Brep region)
        {
            Point3d regionCenter = Utils.GetRegionCenter(region);

            double dist = regionCenter.DistanceTo(region.ClosestPoint(regionCenter));
            bool isNotDonut = dist < 10;

            return isNotDonut;
        }

        public static bool PointInRegion(Brep region, Point3d pt, double tolerance)
        {
            bool result = (pt.DistanceTo(region.ClosestPoint(pt)) < tolerance) ? true : false;

            return result;
        }

        public static bool PointIsCornerPoint(Point3d pt, Brep region)
        {
            var bounds = region.GetBoundingBox(Plane.WorldXY);

            if (Utils.EqualWithinTolerance(pt.X, bounds.Max.X, 0.1) || Utils.EqualWithinTolerance(pt.X, bounds.Min.X, 0.1))
            {
                return true;
            }

            if (Utils.EqualWithinTolerance(pt.Y, bounds.Max.Y, 0.1) || Utils.EqualWithinTolerance(pt.Y, bounds.Min.Y, 0.1))
            {
                return true;
            }

            return false;
        }

        public static bool RegionProportionIsVertical(Brep region)
        {
            var result = Confirm.VectorProportionIsVertical(new CurveBounds(Utils.GetBoundingBoxCurve(region)).Diagonal);

            return result;
        }

        public static bool PointInRegion(Brep region, Point3d pt)
        {
            var testCurveA = new LineCurve(pt, new Point3d(pt.X, double.MaxValue, 0));
            var testCurveB = new LineCurve(pt, new Point3d(pt.X, double.MaxValue * -1, 0));

            Intersection.CurveBrep(testCurveA, region, 0.1, out Curve[] crvA, out Point3d[] ptA);
            Intersection.CurveBrep(testCurveB, region, 0.1, out Curve[] crvB, out Point3d[] ptB);

            var result = crvA.Length + ptA.Length > 0 && crvB.Length + ptB.Length > 0;

            return result;
        }
    }
}
