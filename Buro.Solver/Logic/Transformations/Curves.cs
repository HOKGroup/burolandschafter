using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Logic.Relationships;
using Rhino.Geometry;

namespace HOK.Buro.Logic.Transformations
{
    public static class Curves
    {
        public static Curve OffsetClosed(Curve inputCurve, double distance, bool bothSides)
        {
            //Curve.Offset() run in compute currently returns null. McNeel is revamping the command to not depend on an active document.
            //See: https://discourse.mcneel.com/t/curve-offset-and-mstest/66042/5

            Point3d firstStartPoint = inputCurve.PointAtStart;
            Point3d firstEndPoint = inputCurve.PointAtEnd;

            Plane defaultPlane = Plane.WorldXY;

            Curve[] offsetCurves = inputCurve.Offset(defaultPlane, distance, 0.1, CurveOffsetCornerStyle.Sharp);
            Curve[] mirroredOffsetCurves = inputCurve.Offset(defaultPlane, distance * -1, 0.1, CurveOffsetCornerStyle.Sharp);

            if (offsetCurves.Length > 1)
            {
                Console.WriteLine("Curve is not clean.");

                return null;
            }

            if (bothSides == true && mirroredOffsetCurves.Length > 1)
            {
                Console.WriteLine("Curve is not clean.");

                return null;
            }

            Curve offsetCurve = offsetCurves[0];
            Point3d newStartPoint = offsetCurve.PointAtStart;
            Point3d newEndPoint = offsetCurve.PointAtEnd;

            Curve mirroredOffsetCurve = mirroredOffsetCurves[0];
            Point3d mirroredNewStartPoint = mirroredOffsetCurve.PointAtStart;
            Point3d mirroredNewEndPoint = mirroredOffsetCurve.PointAtEnd;

            Curve startCapCurve = null;
            Curve endCapCurve = null;

            List<Curve> regionCurvePieces = new List<Curve>();

            if (bothSides == false)
            {
                startCapCurve = new LineCurve(firstStartPoint, newStartPoint);
                endCapCurve = new LineCurve(firstEndPoint, newEndPoint);

                regionCurvePieces.Add(inputCurve);
            }
            else if (bothSides)
            {
                startCapCurve = new LineCurve(mirroredNewStartPoint, newStartPoint);
                endCapCurve = new LineCurve(mirroredNewEndPoint, newEndPoint);

                regionCurvePieces.Add(mirroredOffsetCurve);
            }

            regionCurvePieces.Add(startCapCurve);
            regionCurvePieces.Add(offsetCurve);
            regionCurvePieces.Add(endCapCurve);

            Curve[] regionOffset = Curve.JoinCurves(regionCurvePieces);

            if (regionOffset.Length > 1)
            {
                Console.WriteLine("Curve was unsuccessful.");

                return null;
            }

            return regionOffset[0];
        }

        public static List<Curve> JoinOrthogonal(List<Curve> segments)
        {
            //Group curves by slope, then join.

            return null;
        }

        public static Curve RebuildPerpendicularTo(Curve baseCurve, Vector3d direction)
        {
            Point3d basePoint = Utils.GetCurveMidPoint(baseCurve);

            double scalingFactor = baseCurve.GetLength() / (direction.Length * 2);
            Vector3d leftDirVector = direction * scalingFactor;
            Vector3d rightDirVector = direction * scalingFactor;

            leftDirVector.Rotate((Math.PI / 2), Vector3d.ZAxis);
            rightDirVector.Rotate((Math.PI / -2), Vector3d.ZAxis);

            Point3d leftDirPoint = new Point3d(leftDirVector.X, leftDirVector.Y, 0);
            Point3d rightDirPoint = new Point3d(rightDirVector.X, rightDirVector.Y, 0);

            Point3d startPoint = leftDirPoint + basePoint;
            Point3d endPoint = rightDirPoint + basePoint;

            Curve rebuiltCurve = new LineCurve(startPoint, endPoint);

            return rebuiltCurve;
        }

        public static List<Curve> ShatterToSegments(List<Curve> curvesToShatter)
        {
            int numCurves = curvesToShatter.Count;

            List<Point3d> allIntersectionPoints = new List<Point3d>();

            for (int i = 0; i < numCurves; i++)
            {
                for (int j = 0; j < numCurves; j++)
                {
                    if (j != i)
                    {
                        Rhino.Geometry.Intersect.CurveIntersections ccx = Rhino.Geometry.Intersect.Intersection.CurveCurve(curvesToShatter[i], curvesToShatter[j], 0.1, 0.1);

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

            List<Point3d> uniqueIntersectionPoints = new List<Point3d>(Point3d.CullDuplicates(allIntersectionPoints, 0.1));

            //Console.WriteLine(uniqueIntersectionPoints.Count);

            List<Curve> allSegments = new List<Curve>();

            List<double> validParameters = new List<double>();

            foreach (Curve curve in curvesToShatter)
            {
                foreach (Point3d point in uniqueIntersectionPoints)
                {
                    curve.ClosestPoint(point, out double t);
                    Point3d curvePoint = curve.PointAt(t);

                    if (point.DistanceTo(curvePoint) < 0.1)
                    {
                        validParameters.Add(t);
                    }
                }

                if (validParameters.Count > 0)
                {
                    Curve[] segments = curve.Split(validParameters);

                    foreach (Curve segment in segments)
                    {
                        allSegments.Add(segment);
                    }
                }
                else
                {
                    allSegments.Add(curve);
                }

                validParameters.Clear();
            }

            return allSegments;
        }

        public static Curve TrimWithClosedCurve(Curve trimRegion, Curve curve)
        {
            //TODO: Confirm that persistent error with Curve.Trim is my fault and not Rhino Compute. Seems to always return null, like Offset.
            bool curvesIntersect = Confirm.CurvesIntersect(trimRegion, curve, false);

            if (curve.Degree > 1)
            {
                //Current stopgap to compensate for Curve.Trim rebuilds from intersection points. Output is incorrect for nonlinear curves.
                return curve;
            }

            Console.WriteLine("Curves intersect!");
            Console.WriteLine(curve.PointAtStart.ToString());

            if (!curvesIntersect)
            {
                Console.WriteLine("Trim region and curve do not intersect. Returning original curve.");

                return curve;
            }

            List<Point3d> trimPoints = new List<Point3d>();

            Rhino.Geometry.Intersect.CurveIntersections ccx = Rhino.Geometry.Intersect.Intersection.CurveCurve(trimRegion, curve, 0.1, 0.1);

            if (ccx.Count != 2)
            {
                Console.WriteLine("Not exactly two intersection events. Circulation axis placement is invalid. Returning original curve.");

                return curve;
            }

            for (int i = 0; i < ccx.Count; i++)
            {
                trimPoints.Add(ccx[i].PointB);
            }

            Curve trimmedCurve = new LineCurve(trimPoints[0], trimPoints[1]);

            /*
            if (trimmedCurve == null)
            {
                Console.WriteLine("Damn.");
                return curve;
            }
            */

            return trimmedCurve;
        }

        public static Curve ExtendToBounds(Curve bounds, Curve curve)
        {
            Curve oversizeCurve = curve.Extend(CurveEnd.Both, 200, CurveExtensionStyle.Line);

            Curve extendedCurve = Curves.TrimWithClosedCurve(bounds, oversizeCurve);

            return extendedCurve;
        }

        public static List<Curve> JoinColinear(List<Curve> curvesToJoin)
        {
            List<List<Curve>> groupedCurves = Curves.GroupBySlope(curvesToJoin);

            List<Curve> joinedCurves = new List<Curve>();

            foreach (List<Curve> curveGroup in groupedCurves)
            {
                var curveGroupCopy = new List<Curve>();

                foreach (Curve curve in curveGroup)
                {
                    curveGroupCopy.Add(curve.DuplicateCurve());
                }

                Curve[] joinOutput = Curve.JoinCurves(curveGroupCopy);

                foreach (Curve curve in joinOutput)
                {
                    joinedCurves.Add(curve);
                }
            }

            return joinedCurves;
        }

        public static List<List<Curve>> GroupBySlope(List<Curve> curvesToGroup)
        {
            //Current implementation rounds off values for an approximate grouping.
            List<double> slopes = new List<double>();

            foreach (Curve curve in curvesToGroup)
            {
                if (curve.Degree > 1)
                {
                    slopes.Add(double.NaN);
                    continue;
                }

                double slope = Utils.GetSlope(curve);

                slopes.Add(Math.Round(slope));
            }

            List<List<Curve>> groupedCurves = new List<List<Curve>>();

            for (int i = 0; i < curvesToGroup.Count; i++)
            {
                List<Curve> curveCache = new List<Curve>();

                double activeSlope = slopes[i];

                if (activeSlope == double.NaN)
                {
                    continue;
                }

                for (int j = 0; j < curvesToGroup.Count; j++)
                {
                    if (slopes[j] == activeSlope)
                    {
                        curveCache.Add(curvesToGroup[j]);
                        slopes[j] = double.NaN;
                    }
                }

                if (curveCache.Count > 0)
                {
                    groupedCurves.Add(curveCache);
                }
            }

            return groupedCurves;
        }
    }
}
