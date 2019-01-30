using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Formats;
//using System.ValueTuple;

using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;

namespace HOK.Buro.Logic
{
    public static class Utils
    {
        public class Debug
        {
            public static void PrintList<T>(ref List<T> list)
            {
                #if VERBOSE

                var lastItem = list.Last();

                list.ForEach(x => RhinoApp.Write(x.Equals(lastItem) ? x.ToString() : x.ToString() + " "));
                RhinoApp.WriteLine();
                
                #else

                StackTrace stackTrace = new StackTrace(true);
                StackFrame frame = stackTrace.GetFrame(1);

                RhinoApp.WriteLine($"Stagnant log! {Utils.Debug.StackTraceInfo(frame)}");

                #endif
            }

            public static void PrintList<T>(ref List<T> list, string delimiter)
            {
                #if VERBOSE

                var lastItem = list.Last();

                list.ForEach(x => RhinoApp.Write(x.Equals(lastItem) ? x.ToString() : x.ToString() + delimiter));
                RhinoApp.WriteLine(); 

                #else

                StackTrace stackTrace = new StackTrace(true);
                StackFrame frame = stackTrace.GetFrame(1);

                RhinoApp.WriteLine($"Stagnant log! {Utils.Debug.StackTraceInfo(frame)}");

                #endif
            }

            public static string StackTraceInfo(StackFrame frame)
            {
                var fileName = frame.GetFileName() == null ? new []{"Inaccessible"} : frame.GetFileName().Split('\\');
                var className = fileName[fileName.Length - 1];
                var methodName = frame.GetMethod().Name;
                var line = frame.GetFileLineNumber();

                return $"[ {className}: {methodName} Line {line} ]";
            }
        }

        public static Plane GetDefaultPlane()
        {
            return new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
        }

        public static Point3d GetRegionCenter(Brep geo)
        {
            return geo.GetBoundingBox(Utils.GetDefaultPlane()).Center;
        }

        public static Point3d GetCurveMidPoint(Curve curve)
        {
            Point3d midPoint = curve.PointAtNormalizedLength(0.5);

            return midPoint;
        }

        public static double GetAverageLength(List<Curve> curves)
        {
            List<double> lengths = new List<double>();

            foreach (Curve curve in curves)
            {
                lengths.Add(curve.GetLength());
            }

            return lengths.Average();
        }

        public static Interval GetLengthRange(List<Curve> curves)
        {
            List<double> lengths = new List<double>();

            foreach (Curve curve in curves)
            {
                lengths.Add(curve.GetLength());
            }

            Interval range = new Interval(lengths.Min(), lengths.Max());

            return range;
        }

        public static double GetSlope(Curve curve)
        {
            Point3d startPoint = curve.PointAtStart;
            Point3d endPoint = curve.PointAtEnd;

            Console.WriteLine(startPoint.ToString() + " to " + endPoint.ToString());

            Point3d refPoint = (endPoint.X < startPoint.X) ? endPoint : startPoint;
            Point3d slopePoint = (endPoint.X < startPoint.X) ? startPoint : endPoint;

            Point3d normalizedSlopePoint = new Point3d(slopePoint.X - refPoint.X, slopePoint.Y - refPoint.Y, slopePoint.Z - refPoint.Z);

            Console.WriteLine(normalizedSlopePoint.ToString());

            if (normalizedSlopePoint.X != 0)
            {
                double slope = normalizedSlopePoint.Y / normalizedSlopePoint.X;

                return slope;
            }
            else
            {
                return -1;
            }
        }

        public static double GetAverageArea(List<Brep> regions)
        {
            List<double> areas = new List<double>();

            foreach (Brep region in regions)
            {
                areas.Add(region.GetArea());
            }

            return areas.Average();
        }

        public static double GetTotalArea(List<Brep> regions)
        {
            double totalArea = 0;

            foreach (Brep region in regions)
            {
                totalArea = totalArea + region.GetArea();
            }

            return totalArea;
        }

        public static CurveBounds DeconstructRectangle(Curve rect)
        {
            //Assumes, and does not confirm, that input is a rectangle!

            return new CurveBounds(rect);
        }

        public static List<Point3d> GetAllCurveIntersections(List<Curve> curves, bool unique)
        {
            int numCurves = curves.Count;
            List<Point3d> allIntersectionPoints = new List<Point3d>();

            for (int i = 0; i < numCurves; i++)
            {
                for (int j = 0; j < numCurves; j++)
                {
                    if (j != i)
                    {
                        Rhino.Geometry.Intersect.CurveIntersections ccx = Rhino.Geometry.Intersect.Intersection.CurveCurve(curves[i], curves[j], 0.1, 0.1);

                        for (int k = 0; k < ccx.Count; k++)
                        {
                            if (ccx[k].IsPoint)
                            {
                                allIntersectionPoints.Add(ccx[k].PointA);
                            }
                        }
                    }
                }
            }

            return unique ? new List<Point3d>(Point3d.CullDuplicates(allIntersectionPoints, 0.1)) : allIntersectionPoints;
        }

        public static List<Point3d> GetAllCurveIntersections(Curve referenceCurve, List<Curve> otherCurves, bool unique)
        {
            //Console.WriteLine("Function called!");

            List<Point3d> allIntersectionPoints = new List<Point3d>();
            
            foreach (Curve curve in otherCurves)
            {
                Rhino.Geometry.Intersect.CurveIntersections ccx = Rhino.Geometry.Intersect.Intersection.CurveCurve(referenceCurve, curve, 0.1, 0.1);

                //Console.WriteLine("CCX Run!");

                for (int i = 0; i < ccx.Count; i++)
                {
                    if (ccx[i].IsPoint)
                    {
                        allIntersectionPoints.Add(ccx[i].PointA);
                    }
                }

                //Console.WriteLine("Curve");
            }

            if (allIntersectionPoints.Count == 0)
            {
                return allIntersectionPoints;
            }

            if (!unique)
            {
                return allIntersectionPoints;
            }
            else
            {
                return new List<Point3d>(Point3d.CullDuplicates(allIntersectionPoints, 0.1));
            }
        }

        public static Point3d GetClosestPoint(Point3d refPoint, List<Point3d> pointsToSort)
        {
            List<double> distances = new List<double>();

            for (int i = 0; i < pointsToSort.Count; i++)
            {
                distances.Add(refPoint.DistanceTo(pointsToSort[i]));
            }

            return pointsToSort[distances.IndexOf(distances.Min())];
        }

        /// <summary>
        /// Takes a series of non-integer values and rounds them, up or down, to maintain a predefined total.
        /// </summary>
        /// <param name="valuesToRound"></param>
        /// <param name="totalToMaintain"></param>
        /// <returns></returns>
        public static List<int> RoundWithinTotal(List<double> valuesToRound, int totalToMaintain)
        {
            List<int> roundedVals = new List<int>();
            List<double> delta = new List<double>();
            List<int> indexInts = new List<int>();

            for (int i = 0; i < valuesToRound.Count; i++)
            {
                int newVal = Convert.ToInt32(Math.Floor(valuesToRound[i]));

                indexInts.Add(i);
                delta.Add(valuesToRound[i] - newVal);
                roundedVals.Add(newVal);
            }

            int floorTotal = roundedVals.Sum();
            int remainder = totalToMaintain - floorTotal;

            double[] deltaArray = delta.ToArray();
            int[] indexIntsArray = indexInts.ToArray();

            Array.Sort(indexIntsArray, deltaArray);

            indexIntsArray.Reverse();

            for (int i = 0; i < remainder; i++)
            {
                roundedVals[indexIntsArray[i]] = roundedVals[indexIntsArray[i]] + 1;
            }

            return roundedVals;
        }

        public static Vector2d FlattenVector(Vector3d vector)
        {
            return new Vector2d(vector.X, vector.Y);
        }

        public static Curve GetLongestCurve(List<Curve> curves)
        {
            return curves.Count == 1 ? curves[0] : curves.OrderBy(x => x.GetLength()).ToList()[curves.Count - 1];
        }

        public static bool EqualWithinTolerance(double valueA, double valueB, double tolerance)
        {
            double diff = Math.Abs(valueB - valueA);

            if (diff <= tolerance)
            {
                return true;
            }

            return false;
        }

        public static Curve GetBoundingBoxCurve(Brep region)
        {
            var curveBounds = region.GetBoundingBox(Plane.WorldXY);
            var rect = new Rectangle3d(Plane.WorldXY, curveBounds.Min, curveBounds.Max).ToNurbsCurve();

            return rect;
        }

        public static int InstancesInList<T>(ref List<T> list, ref T key)
        {
            var count = 0;

            foreach (T item in list)
            {
                if (key.ToString() == item.ToString())
                {
                    count++;
                }
            }

            return count;
        }

        public static int NestedListCount<T>(ref List<List<T>> list)
        {
            var count = 0;

            foreach (List<T> sublist in list)
            {
                foreach (T item in sublist)
                {
                    count++;
                }
            }

            return count;
        }

        public static Curve GetRegionPerimeter(Brep region)
        {
            return region.Curves3D.Count > 1 ? Utils.GetLongestCurve(new List<Curve>(Curve.JoinCurves(region.Curves3D))) : region.Curves3D.Count == 0 ? null : region.Curves3D[0];
        }

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
