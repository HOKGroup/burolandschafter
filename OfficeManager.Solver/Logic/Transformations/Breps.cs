using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Formats;
using HOK.Buro.Logic.Relationships;
using Rhino.Geometry;

namespace HOK.Buro.Logic.Transformations
{
    public static class Breps
    {
        public static List<Brep> SplitTwoBreps(Brep region, Brep cutter)
        {
            Brep[] AllRegionsArray = region.Split(cutter, .01);
            List<Brep> AllRegions = new List<Brep>(AllRegionsArray);

            return AllRegions;
        }

        public static List<Brep> TrimAllIntersections(List<Brep> regionsToTrim, Brep cutter)
        {
            List<Brep> trimmedRegions = new List<Brep>();

            foreach (Brep region in regionsToTrim)
            {
                //bool regionsIntersect = Confirm.RegionsIntersect(region, cutter);
                List<Brep> splitRegions = Breps.SplitTwoBreps(region, cutter);

                if (splitRegions.Count != 0)
                {
                    List<Brep> validRegions = Select.FloorFromCore(splitRegions, cutter);

                    foreach (Brep trimmedRegion in validRegions)
                    {
                        trimmedRegions.Add(trimmedRegion);
                    }
                }
                else
                {
                    trimmedRegions.Add(region);
                }
            }

            return trimmedRegions;
        }

        public static List<Brep> SplitByCurve(Brep brepToSplit, Curve splitter)
        {
            if (splitter == null)
            {
                List<Brep> unmodified = new List<Brep>();
                unmodified.Add(brepToSplit);
                return unmodified;
            }

            Brep splitterBrep = Extrusion.CreateExtrusion(splitter, Vector3d.ZAxis).ToBrep();
            Brep[] splitBreps = brepToSplit.Split(splitterBrep, 0.1);

            List<Brep> result = new List<Brep>(splitBreps);

            //MessageBox.Show(result.Count.ToString());

            return result;
        }

        public static List<Brep> SplitByCurves(Brep brepToSplit, List<Curve> splitters)
        {
            List<Brep> splitBreps = new List<Brep>();
            splitBreps.Add(brepToSplit);

            //RhinoApp.WriteLine("Region being split by {0} curves.", splitters.Count);

            foreach (Curve splitter in splitters)
            {
                List<Brep> splitCache = new List<Brep>();

                //RhinoApp.WriteLine("--About to split {0} regions.", splitBreps.Count);

                for (int i = 0; i < splitBreps.Count; i++)
                {
                    if (Confirm.CurveRegionIntersection(splitter, splitBreps[i]))
                    {
                        List<Brep> splitResult = SplitByCurve(splitBreps[i], splitter);

                        if (splitResult.Count < 2)
                        {
                            splitCache.Add(splitBreps[i]);
                        }
                        else
                        {
                            foreach (Brep result in splitResult)
                            {
                                splitCache.Add(result);
                            }
                        }
                    }
                    else
                    {
                        splitCache.Add(splitBreps[i]);
                    }
                }

                //RhinoApp.WriteLine("--Split into {0} regions.", splitCache.Count);

                if (splitCache.Count != 0)
                {
                    splitBreps.Clear();

                    foreach (Brep brep in splitCache)
                    {
                        splitBreps.Add(brep);
                    }

                    splitCache.Clear();
                }
            }

            return splitBreps;
        }

        public static List<Brep> Rectangularize(ZonePackage zone)
        {
            var rectangularizedZones = new List<Brep>();

            if (Utils.GetLongestCurve(new List<Curve>(Curve.JoinCurves(zone.Region.Curves3D))).SpanCount <= 4)
            {
                rectangularizedZones.Add(zone.Region);
                return rectangularizedZones;
            }

            var boundsRect = new CurveBounds(new Rectangle3d(Plane.WorldXY, zone.Region.GetBoundingBox(Plane.WorldXY).Max, zone.Region.GetBoundingBox(Plane.WorldXY).Min).ToNurbsCurve());
            var rectRangeX = boundsRect.RangeX;
            var rectRangeY = boundsRect.RangeY;

            var proportion = rectRangeX > rectRangeY ? rectRangeX / rectRangeY : rectRangeY / rectRangeX;

            if (boundsRect.Area < 2.5 * zone.BaseArea && proportion < 2)
            {
                rectangularizedZones.Add(zone.Region);
                return rectangularizedZones;
            }

            var allRegionPerimeters = new List<Curve>(Curve.JoinCurves(zone.Region.Curves3D));

            var regionBoundary = Utils.GetLongestCurve(allRegionPerimeters).Simplify(CurveSimplifyOptions.All, 0.1, 0.1);
            var otherPerimeters = new List<Curve>();

            foreach (Curve curve in allRegionPerimeters)
            {
                if (Utils.EqualWithinTolerance(regionBoundary.GetLength(), curve.GetLength(), 1) == false)
                {
                    otherPerimeters.Add(curve.Simplify(CurveSimplifyOptions.All, 0.1, 0.1));
                }
            }

            Curve primaryCirculationEdge = Select.PrimaryCirculationEdge(zone);

            var splitters = new List<Curve>();

            //Generate splitting curves based on discontinuities in exterior perimeter curve of zone.
            for (int i = 0; i <= regionBoundary.SpanCount; i++)
            {
                var activePoint = Point3d.Unset;

                if (i == regionBoundary.SpanCount)
                {
                    activePoint = regionBoundary.PointAt(regionBoundary.SpanDomain(regionBoundary.SpanCount - 1).Max);
                }
                else
                {
                    activePoint = regionBoundary.PointAt(regionBoundary.SpanDomain(i).Min);
                }

                if (Confirm.PointIsCornerPoint(activePoint, zone.Region))
                {
                    //Corner points generate undesirable splitting geometry.
                    continue;
                }

                primaryCirculationEdge.ClosestPoint(activePoint, out double t);
                var pointOnCirculationEdge = primaryCirculationEdge.PointAt(t);

                if (activePoint.DistanceTo(pointOnCirculationEdge) < 0.1)
                {
                    //Active point is coincident with circulation edge and is unusable.
                    continue;
                }

                splitters.Add(new LineCurve(activePoint, pointOnCirculationEdge));
            }

            //Generate splitting curves based on an interior exemptions, if they exist.
            if (otherPerimeters.Count > 0)
            {
                foreach (Curve perimeter in otherPerimeters)
                {
                    var perimeterRegion = Brep.CreatePlanarBreps(perimeter)[0];
                    var activePoint = Point3d.Unset;

                    for (int i = 0; i < perimeter.SpanCount; i++)
                    {
                        activePoint = perimeter.PointAt(perimeter.SpanDomain(i).Min);

                        if (!Confirm.PointIsCornerPoint(activePoint, perimeterRegion))
                        {
                            //For interior exemptions, corner points are desirable.
                            continue;
                        }

                        primaryCirculationEdge.ClosestPoint(activePoint, out double t);
                        var pointOnCirculationEdge = primaryCirculationEdge.PointAt(t);

                        if (activePoint.DistanceTo(pointOnCirculationEdge) < 0.1)
                        {
                            //Active point is coincident with circulation edge and is unusable.
                            continue;
                        }

                        splitters.Add(new LineCurve(activePoint, pointOnCirculationEdge));
                    }
                }
            }

            //Extend splitters to guarantee they traverse region boundary.
            for (int i = 0; i < splitters.Count; i++)
            {
                var extendedSplitter = Curves.ExtendToBounds(regionBoundary, splitters[i]);

                splitters[i] = extendedSplitter;
            }

            //Split region by all splitter curves.
            var roomRegions = Breps.SplitByCurves(zone.Region, splitters);

            return roomRegions;
        }
    }
}
