using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Logic.Transformations;
using HOK.Buro.Formats;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace HOK.Buro.Logic.Relationships
{
    public static class Select
    {
        public static List<Brep> FloorFromCore(List<Brep> set, Brep core)
        {
            //Cull the result with a center closest to the core's center.

            Point3d coreCenterPoint = core.GetBoundingBox(Utils.GetDefaultPlane()).Center;
            List<Brep> validRegions = new List<Brep>();

            List<double> distances = new List<double>();

            for (int i = 0; i < set.Count; i++)
            {
                Point3d activeCenter = set[i].GetBoundingBox(Utils.GetDefaultPlane()).Center;

                double distance = activeCenter.DistanceTo(coreCenterPoint);

                distances.Add(distance);
            }

            int indexOfInvalidRegion = distances.IndexOf(distances.Min());

            for (int i = 0; i < set.Count; i++)
            {
                if (i != indexOfInvalidRegion)
                {
                    validRegions.Add(set[i]);
                }
            }

            return validRegions;
        }

        public static List<Brep> NotExemptRegions(List<Brep> baseRegions, List<Brep> exemptRegions)
        {
            List<Brep> validRegions = baseRegions;

            foreach (Brep exemption in exemptRegions)
            {
                List<Brep> latestTrimmedRegions = Breps.TrimAllIntersections(validRegions, exemption);

                validRegions = latestTrimmedRegions;
            }

            return validRegions;
        }

        public static List<Brep> NotCirculationRegions(List<Brep> baseRegions, List<Brep> exemptRegions)
        {
            List<Brep> validRegions = baseRegions;
            List<Brep> validRegionsCache = new List<Brep>();

            foreach (Brep exemption in exemptRegions)
            {
                for (int i = validRegions.Count - 1; i >= 0; i--)
                {
                    List<Brep> splitOperation = Breps.SplitTwoBreps(validRegions[i], exemption);

                    if (splitOperation.Count < 1)
                    {
                        continue;
                    }

                    foreach (Brep splitBrep in splitOperation)
                    {
                        //Remove any regions coincident with circulation. (Can be more than one.)
                        Point3d regionCenter = Utils.GetRegionCenter(splitBrep);

                        if (regionCenter.DistanceTo(exemption.ClosestPoint(regionCenter)) < 0.1)
                        {
                            //Do nothing.
                        }
                        else
                        {
                            validRegionsCache.Add(splitBrep);
                        }
                    }
                }
                validRegions = validRegionsCache;
                validRegionsCache.Clear();
            }
            return validRegions;
        }

        public static List<Curve> CoreCurvesWithoutEgress(List<Curve> coreAccessCurves, List<Curve> circulationAxisCurves)
        {
            List<Curve> curvesWithoutEgress = new List<Curve>();

            foreach (Curve accessCurve in coreAccessCurves)
            {
                for (int i = 0; i < circulationAxisCurves.Count; i++)
                {
                    bool curvesIntersect = Confirm.CurvesIntersect(accessCurve, circulationAxisCurves[i], true);

                    if (curvesIntersect)
                    {
                        break;
                    }
                    else if (i == circulationAxisCurves.Count - 1)
                    {
                        curvesWithoutEgress.Add(accessCurve);
                    }
                }
            }

            return curvesWithoutEgress;
        }

        public static List<Curve> BestSourceOfEgress(List<Curve> coreAccessCurves, List<Curve> circulationAxisCurves)
        {
            //Select closest circulation axis based on core curve's midpoint.
            List<Curve> orderedSelectedAxis = new List<Curve>();

            //MessageBox.Show(coreAccessCurves.Count.ToString() + " / " + circulationAxisCurves.Count.ToString());

            foreach (Curve coreCurve in coreAccessCurves)
            {
                Point3d midPoint = Utils.GetCurveMidPoint(coreCurve);

                List<double> midPointDistancesToCirculation = new List<double>();

                for (int i = 0; i < circulationAxisCurves.Count; i++)
                {
                    circulationAxisCurves[i].ClosestPoint(midPoint, out double closestPointParameter);
                    Point3d axisClosestPoint = circulationAxisCurves[i].PointAt(closestPointParameter);

                    midPointDistancesToCirculation.Add(midPoint.DistanceTo(axisClosestPoint));
                }

                int bestSourceIndex = midPointDistancesToCirculation.IndexOf(midPointDistancesToCirculation.Min());
                orderedSelectedAxis.Add(circulationAxisCurves[bestSourceIndex]);
            }

            return orderedSelectedAxis;
        }

        public static List<Brep> OrthogonalEgress(List<Curve> coreCurves, List<Curve> egressAxis)
        {
            List<Brep> egressExemptionRegions = new List<Brep>();

            for (int i = 0; i < egressAxis.Count; i++)
            {
                Point3d coreCurveMidpoint = Utils.GetCurveMidPoint(coreCurves[i]);
                egressAxis[i].ClosestPoint(coreCurveMidpoint, out double circulationParameter);
                Point3d circulationPoint = egressAxis[i].PointAt(circulationParameter);

                Vector3d connectionVector = new Vector3d(circulationPoint - coreCurveMidpoint);
                bool proportionIsVertical = Confirm.VectorProportionIsVertical(connectionVector);

                Vector3d egressVector = (proportionIsVertical == true) ? new Vector3d(0, connectionVector.Y, 0) : new Vector3d(connectionVector.X, 0, 0);

                Curve egressRegionProfile = Curves.RebuildPerpendicularTo(coreCurves[i], egressVector);

                //MessageBox.Show(connectionVector.ToString() + " => " + egressVector.ToString());

                Brep egressRegion = Brep.CreateFromSurface(Extrusion.CreateExtrusion(egressRegionProfile, egressVector));

                egressExemptionRegions.Add(egressRegion);
            }

            return egressExemptionRegions;
        }

        public static CirculationPackage CirculationConfig(TestFitPackage tf, List<Curve> axisSegments)
        {
            //Classifications.
            List<bool> TouchesEdge = new List<bool>();
            List<bool> TouchesCore = new List<bool>();
            List<bool> IsLong = new List<bool>();

            double avgLength = Utils.GetAverageLength(axisSegments);
            Interval lengthRange = Utils.GetLengthRange(axisSegments);

            for (int i = 0; i < axisSegments.Count; i++)
            {
                bool floorProfileEdgeTest = Confirm.CurvesIntersect(axisSegments[i], tf.FloorPlanPackage.FloorProfile, true);
                TouchesEdge.Add(floorProfileEdgeTest);

                bool coreEdgeTest = Confirm.CurvesIntersect(axisSegments[i], tf.FloorPlanPackage.CoreProfile, true);
                TouchesCore.Add(coreEdgeTest);

                bool isLong = (axisSegments[i].GetLength() > avgLength) ? true : false;
                IsLong.Add(isLong);
            }

            List<Curve> MainCirculationCurves = new List<Curve>();
            List<Curve> OptionalCirculationCurves = new List<Curve>();

            for (int i = 0; i < axisSegments.Count; i++)
            {
                if (TouchesCore[i] == false && TouchesEdge[i] == false)
                {
                    MainCirculationCurves.Add(axisSegments[i]);
                }
                else if (TouchesCore[i] == true)
                {
                    MainCirculationCurves.Add(axisSegments[i]);
                }
                else if (TouchesEdge[i] == true && IsLong[i] == true)
                {
                    MainCirculationCurves.Add(axisSegments[i]);
                }
                else
                {
                    OptionalCirculationCurves.Add(axisSegments[i]);
                }
            }

            bool hasOneAxis = Confirm.AllAxisColinear(MainCirculationCurves);

            if (hasOneAxis)
            {
                Curve singleAxisCurve = Curves.ExtendToBounds(tf.FloorPlanPackage.FloorProfile, MainCirculationCurves[0]);

                MainCirculationCurves.Clear();
                MainCirculationCurves.Add(singleAxisCurve);
            }

            CirculationPackage ClassifiedCirculation = new CirculationPackage(MainCirculationCurves, OptionalCirculationCurves);

            return ClassifiedCirculation;
        }

        public static List<Curve> BestSplitterCurves(List<Brep> zones, List<Curve> circ, Curve core)
        {
            int numZones = zones.Count;
            int numCircCurves = circ.Count;

            List<Curve> splitterCurves = new List<Curve>();

            for (int i = 0; i < numZones; i++)
            {
                int validCircCurves = 0;

                List<bool> intersects = new List<bool>();
                List<double> intersectionInterval = new List<double>();

                //RhinoApp.WriteLine("---");

                for (int j = 0; j < numCircCurves; j++)
                {
                    if (Confirm.CurveRegionIntersection(circ[j], zones[i]) && circ[j].Degree == 1)
                    {
                        //RhinoApp.WriteLine("Region intersection exists.");

                        validCircCurves++;
                        intersects.Add(true);

                        CurveIntersections csx = Intersection.CurveSurface(circ[j], zones[i].Surfaces[0], 0.1, 0.1);

                        //RhinoApp.WriteLine("--{0} csx event(s). (Overlap: {1})", csx.Count, csx[0].OverlapA.ToString());

                        intersectionInterval.Add(csx[0].OverlapA.T1 - csx[0].OverlapB.T0);
                    }
                    else
                    {
                        //RhinoApp.WriteLine("Region intersection does not exist.");

                        intersects.Add(false);

                        intersectionInterval.Add(0);
                    }
                }

                if (validCircCurves == 0)
                {
                    //RhinoApp.WriteLine("No circulation options.");

                    Curve newSplitCurve = Select.GenerateSplitCurve(zones[i], core);

                    splitterCurves.Add(newSplitCurve);
                }
                if (validCircCurves == 1)
                {
                    //RhinoApp.WriteLine("Only one option.");
                    splitterCurves.Add(circ[intersects.IndexOf(true)]);
                }
                else if (validCircCurves > 1)
                {
                    //RhinoApp.WriteLine(validCircCurves.ToString() + " options.");
                    splitterCurves.Add(circ[intersectionInterval.IndexOf(intersectionInterval.Max())]);
                }

                intersects.Clear();
                intersectionInterval.Clear();
            }

            return splitterCurves;
        }

        public static Curve GenerateSplitCurve(Brep zone, Curve core)
        {
            Point3d zoneCenter = Utils.GetRegionCenter(zone);
            Point3d coreCenter = Utils.GetRegionCenter(Brep.CreatePlanarBreps(core)[0]);
            Vector3d vectorToCore = new Vector3d(coreCenter - zoneCenter);

            bool isVerticalProportion = Confirm.VectorProportionIsVertical(vectorToCore);
            double componentToCheck = (isVerticalProportion) ? vectorToCore.Y : vectorToCore.X;
            bool isPositive = (componentToCheck > 0) ? true : false;

            Point3d coreMin = Brep.CreatePlanarBreps(core)[0].GetBoundingBox(Plane.WorldXY).Min;
            Point3d coreMax = Brep.CreatePlanarBreps(core)[0].GetBoundingBox(Plane.WorldXY).Max;

            Point3d coreExtent = (isPositive) ? coreMin : coreMax;
            double coreExtentValue = (isVerticalProportion) ? coreExtent.Y : coreExtent.X;

            Point3d zoneMin = zone.GetBoundingBox(Plane.WorldXY).Min;
            Point3d zoneMax = zone.GetBoundingBox(Plane.WorldXY).Max;

            Point3d zoneExtent = (isPositive) ? zoneMin : zoneMax;
            double zoneExtentValue = (isVerticalProportion) ? zoneExtent.Y : zoneExtent.X;

            double curveOffset = (zoneExtentValue - coreExtentValue) * 0.20; //Make this a function of zone proportionality.

            double curveStart = coreExtentValue + curveOffset;

            Curve splitCurve = (isVerticalProportion) ? new LineCurve(new Point2d(zoneMin.X, curveStart), new Point2d(zoneMax.X, curveStart)) : new LineCurve(new Point2d(curveStart, zoneMin.Y), new Point2d(curveStart, zoneMax.Y));

            return splitCurve;
        }

        public static List<Curve> DonutSplittingCurves(Brep region)
        {
            BoundingBox regionBounds = region.GetBoundingBox(Plane.WorldXY);
            Point3d regionCenter = Utils.GetRegionCenter(region);

            Curve bisectorA = new LineCurve(regionBounds.Min, regionBounds.Max);
            Curve bisectorB = new LineCurve(new Point2d(regionBounds.Min.X, regionBounds.Max.Y), new Point2d(regionBounds.Max.X, regionBounds.Min.Y));

            Intersection.CurveBrepFace(bisectorA, region.Faces[0], 0.1, out Curve[] curvesA, out Point3d[] ptsA);
            Intersection.CurveBrepFace(bisectorB, region.Faces[0], 0.1, out Curve[] curvesB, out Point3d[] ptsB);

            //RhinoApp.WriteLine((curvesA.Length + curvesB.Length).ToString());

            List<Curve> diagonals = new List<Curve>(curvesA);

            foreach (Curve crv in curvesB)
            {
                diagonals.Add(crv);
            }

            List<Point3d> splitterReferencePoints = new List<Point3d>();

            foreach (Curve diag in diagonals)
            {
                if (diag.GetLength() > 5)
                {
                    List<Point3d> endpoints = new List<Point3d>();
                    endpoints.Add(diag.PointAtStart);
                    endpoints.Add(diag.PointAtEnd);

                    splitterReferencePoints.Add(Utils.GetClosestPoint(regionCenter, endpoints));
                }
            }

            List<double> xVals = new List<double>();
            List<double> yVals = new List<double>();

            foreach (Point3d refPt in splitterReferencePoints)
            {
                xVals.Add(refPt.X);
                yVals.Add(refPt.Y);
            }

            bool isVerticallyProportioned = Confirm.VectorProportionIsVertical(regionBounds.Diagonal);

            List<double> neededVals = (isVerticallyProportioned) ? yVals : xVals;

            List<double> splitterVals = new List<double>();
            splitterVals.Add(neededVals.Min());
            splitterVals.Add(neededVals.Max());

            List<Curve> splitters = new List<Curve>();

            foreach (double val in splitterVals)
            {
                Curve newSplitter = (isVerticallyProportioned) ? new LineCurve(new Point2d(regionBounds.Min.X, val), new Point2d(regionBounds.Max.X, val)) : new LineCurve(new Point2d(val, regionBounds.Min.Y), new Point2d(val, regionBounds.Max.Y));
                splitters.Add(newSplitter);
            }

            return splitters;
        }

        public static EdgeCurves ZoneEdgeCurves(Brep zone, TestFitPackage tf, List<Brep> allZones)
        {
            List<Curve> edgeCurves = new List<Curve>(zone.Curves3D);
            List<Curve> allEdgeCurves = new List<Curve>();

            foreach (Curve curve in edgeCurves)
            {
                if (curve.SpanCount != 1)
                {
                    for (int i = 0; i < curve.SpanCount; i++)
                    {
                        allEdgeCurves.Add(curve.Trim(curve.SpanDomain(i)));
                    }
                }
                else
                {
                    allEdgeCurves.Add(curve);
                }
            }

            //RhinoApp.WriteLine(edgeCurves.Count.ToString() + " => " + allEdgeCurves.Count);

            List<Curve> coreEdges = new List<Curve>();
            List<Curve> perimeterEdges = new List<Curve>();
            List<Curve> circulationEdges = new List<Curve>();
            List<Curve> exemptEdges = new List<Curve>();
            List<Curve> zoneEdges = new List<Curve>();
            List<Curve> structureEdges = new List<Curve>();

            foreach (Curve curve in allEdgeCurves)
            {
                //Nested loops, so goto is used here to kill the search when a match is found. Saves time/processing. Please forgive.
                Point3d testPt = curve.PointAt(curve.Domain.Mid);

                //Check if curve is core adjacent.
                PointContainment coreRelationship = tf.FloorPlanPackage.CoreProfile.Contains(testPt, Plane.WorldXY, 0.1);

                if (coreRelationship == PointContainment.Inside || coreRelationship == PointContainment.Coincident)
                {
                    coreEdges.Add(curve);
                    goto End;
                }

                //Check if curve is perimeter adjacent.
                PointContainment perimeterRelationship = tf.FloorPlanPackage.FloorProfile.Contains(testPt, Plane.WorldXY, 0.1);

                if (perimeterRelationship == PointContainment.Coincident)
                {
                    perimeterEdges.Add(curve);
                    goto End;
                }

                //Check if curve is exemption adjacent.
                for (int i = 0; i < tf.FloorPlanPackage.ExemptionProfiles.Count; i++)
                {
                    PointContainment exemptRelationship = tf.FloorPlanPackage.ExemptionProfiles[i].Contains(testPt, Plane.WorldXY, 0.1);

                    if (exemptRelationship == PointContainment.Coincident || exemptRelationship == PointContainment.Inside)
                    {
                        exemptEdges.Add(curve);
                        goto End;
                    }
                }

                //Check if curve is structure adjacent.
                for (int i = 0; i < tf.FloorPlanPackage.StructureProfiles.Count; i++)
                {
                    PointContainment strRelationship = tf.FloorPlanPackage.StructureProfiles[i].Contains(testPt, Plane.WorldXY, 0.1);

                    if (strRelationship == PointContainment.Inside || strRelationship == PointContainment.Coincident)
                    {
                        structureEdges.Add(curve);
                        goto End;
                    }
                }

                //Check if curve is zone adjacent.
                int adjCount = 0;

                for (int i = 0; i < allZones.Count; i++)
                {
                    if (Confirm.PointInRegion(allZones[i], testPt, 0.1))
                    {
                        adjCount++;
                    }

                    if (adjCount > 1)
                    {
                        //RhinoApp.WriteLine("Zone edge!");
                        zoneEdges.Add(curve);
                        goto End;
                    }
                }

                //Otherwise, add to circulation adjacent.
                circulationEdges.Add(curve);

                End:;
            }

            EdgeCurves edgeCurvePackage = new EdgeCurves(coreEdges, perimeterEdges, circulationEdges, exemptEdges, zoneEdges, structureEdges);

            return edgeCurvePackage;
        }

        public static Point3d EdgeCurveTestPoint(Curve edge, Brep zone)
        {
            //Depreciated: edge midpoint can be used to check for incidence.
            //Faster & more reliable than generating a "correctly" offset point and checking interiority.

            Point3d edgeMidPoint = Utils.GetCurveMidPoint(edge);
            double edgeDomainRange = edge.Domain.Max - edge.Domain.Min;

            edge.FrameAt(edge.Domain.Mid, out Plane plane);

            double targetDist = 0.25;

            Vector3d planeYAxis = plane.YAxis * (targetDist / plane.YAxis.Length);
            Point3d adjustY = new Point3d(planeYAxis);
            Vector3d negPlaneYAxis = (plane.YAxis * -1) * (targetDist / plane.YAxis.Length);
            Point3d adjustNegY = new Point3d(negPlaneYAxis);

            Point3d pointA = edgeMidPoint + adjustY;
            Point3d pointB = edgeMidPoint + adjustNegY;

            zone.Faces[0].ClosestPoint(pointA, out double uA, out double vA);
            PointFaceRelation relA = zone.Faces[0].IsPointOnFace(uA, vA);

            zone.Faces[0].ClosestPoint(pointB, out double uB, out double vB);
            PointFaceRelation relB = zone.Faces[0].IsPointOnFace(uB, vB);

            bool testA = (relA == PointFaceRelation.Interior || relA == PointFaceRelation.Boundary) ? true : false;
            bool testB = (relB == PointFaceRelation.Interior || relB == PointFaceRelation.Boundary) ? true : false;

            //if (testA && testB) { RhinoApp.WriteLine("Double correct instance!"); }
            //if (!testA && !testB) { RhinoApp.WriteLine("No correct instance!"); }

            Point3d testPoint = (testA) ? pointB : pointA;

            if (!testA && !testB)
            {
                RhinoApp.WriteLine("No correct points.");

                return edgeMidPoint;
            }

            return testPoint;
        }

        /// <summary>
        /// Select longest circulation adjacent edge that follows zone proportion.
        /// If no edges follow zone proportion, return longest circulation adjacent edge.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static Curve PrimaryCirculationEdge(ZonePackage zone)
        {
            var zoneProportionVector = zone.Region.GetBoundingBox(Utils.GetDefaultPlane()).Diagonal;
            var zoneProportion = Confirm.VectorProportionIsVertical(zoneProportionVector);

            var circulationEdges = zone.EdgeCurves.CirculationAdjacent;
            var candidateEdgeSegments = new List<Curve>();

            if (circulationEdges.Count == 0)
            {
                return null;
            }

            foreach (Curve edge in circulationEdges)
            {
                var edgeVector = new Vector3d(edge.PointAtEnd - edge.PointAtStart);

                if (Confirm.VectorProportionIsVertical(edgeVector) == zoneProportion)
                {
                    candidateEdgeSegments.Add(edge);
                }
            }

            var candidateEdges = Curves.JoinColinear(candidateEdgeSegments);

            if (candidateEdges.Count == 0)
            {
                return Utils.GetLongestCurve(Curves.JoinColinear(circulationEdges)).Simplify(CurveSimplifyOptions.All, 0.1, 0.1);
            }
            else if (candidateEdges.Count == 1)
            {
                return candidateEdges[0];
            }

            var bestChoiceCurve = Utils.GetLongestCurve(Curves.JoinColinear(candidateEdges)).Simplify(CurveSimplifyOptions.All, 0.1, 0.1);

            zone.PrimaryCirculationEdge = bestChoiceCurve;

            return bestChoiceCurve;
        }

        public static LanePackage NextLanePayload(ZonePackage zone, ProgramManifest pm, List<int> currentFill, double activeVal, double maxVal)
        {
            var selectedPrograms = new List<int>();
            var selectedProgramTypes = new List<string>();
            var laneIsPrivate = false;
            var programWidths = new List<double>();
            var programHeights = new List<double>();

            var maximizedPrograms = new List<int>();

            var minCirculationClearance = 3;

            //Select programs to use in lane size determination.
            for (int i = 0; i < zone.ProgramTargets.Count; i++)
            {
                //Maximum three programs per lane.
                if (selectedPrograms.Count >= 3)
                {
                    break;
                }

                //If program is maximized, note for later.
                if (pm.ProgramPackages[i].Quota == 0)
                {
                    maximizedPrograms.Add(i);
                }

                //If zone quota has already been met, move on to next program.
                if (zone.ProgramTargets[i] - currentFill[i] <= 0)
                {
                    continue;
                }

                var activeProgram = pm.ProgramPackages[i];

                //Lane privacy types cannot mix. Will investigate if this causes problems.
                if (i == 0)
                {
                    laneIsPrivate = activeProgram.IsPrivate;
                }

                //Limit repeat types in each payload.
                if (selectedProgramTypes.Contains(activeProgram.AccessDirections))
                {
                    //Only one type of meeting space per lane.
                    if (activeProgram.AccessDirections == "1111")
                    {
                        continue;
                    }

                    //Only two types of working space per lane.
                    if (activeProgram.AccessDirections == "1000")
                    {
                        if (activeProgram.IsPrivate != laneIsPrivate)
                        {
                            continue;
                        }

                        var prevCount = Utils.InstancesInList(ref selectedProgramTypes, ref activeProgram.AccessDirections);

                        if (prevCount >= 2)
                        {
                            continue;
                        }
                    }
                }

                var activeProgramDims = new CurveBounds(activeProgram.OccupationBoundary);

                selectedPrograms.Add(i);
                programWidths.Add(activeProgramDims.Width);
                programHeights.Add(activeProgramDims.Height);
            }

            //If all targets are projected to be filled, pull from maximized programs.
            if (selectedPrograms.Count == 0)
            {
                var chosenProgramIndex = -1;

                for (int i = 0; i < maximizedPrograms.Count; i++)
                {
                    if (zone.ProgramTargets[maximizedPrograms[i]] - currentFill[i] > 0)
                    {
                        chosenProgramIndex = maximizedPrograms[i];
                    }
                }

                if (chosenProgramIndex >= 0)
                {
                    var activeProgram = pm.ProgramPackages[chosenProgramIndex];
                    var activeProgramDims = new CurveBounds(activeProgram.OccupationBoundary);

                    var sliceVal = activeVal + (2 * activeProgramDims.Height) + minCirculationClearance;

                    return new LanePackage(sliceVal, new List<int>(new[] { chosenProgramIndex }), sliceVal - activeVal);
                }
                else
                {
                    //If there are still no programs to pull from, assume quotas in zone is fulfilled.
                    return new LanePackage(maxVal + 10, new List<int>(), maxVal - activeVal);
                }
            }

            //If only one program is selected, use its dimensions.
            if (selectedPrograms.Count == 1)
            {
                var activeProgram = pm.ProgramPackages[selectedPrograms[0]];
                var activeProgramDims = new CurveBounds(activeProgram.OccupationBoundary);

                //If selection is one private office, split lanes for tight packing.
                if (activeProgram.IsPrivate)
                {
                    var sliceVal = activeVal + activeProgramDims.Width;

                    return new LanePackage(sliceVal, selectedPrograms, sliceVal - activeVal);
                }

                //If selection is one desk/working area, split lanes for two rows of placement + circulation.
                if (activeProgram.AccessDirections == "1000")
                {
                    var sliceVal = activeVal + ((2 * activeProgramDims.Height) + minCirculationClearance);

                    return new LanePackage(sliceVal, selectedPrograms, sliceVal - activeVal);
                }

                //Otherwise, split lanes for single row placement.
                var deltaVal = activeVal + activeProgramDims.Height + minCirculationClearance;

                return new LanePackage(deltaVal, selectedPrograms, deltaVal - activeVal);
            }

            //If two programs are selected, check what the pair's relationship is before designating dimension.
            if (selectedPrograms.Count == 2)
            {
                //Ordinal names used to make priority clear.
                var firstProgram = pm.ProgramPackages[selectedPrograms[0]];
                var firstProgramDims = new CurveBounds(firstProgram.OccupationBoundary);
                var secondProgram = pm.ProgramPackages[selectedPrograms[1]];
                var secondProgramDims = new CurveBounds(secondProgram.OccupationBoundary);

                //If selection is two desks, use dimensions of deeper desk.
                if (firstProgram.AccessDirections == "1000" && secondProgram.AccessDirections == "1000")
                {
                    var deltaVal = firstProgramDims.Height > secondProgramDims.Height ? firstProgramDims.Height : secondProgramDims.Height;

                    var sliceVal = activeVal + ((2 * deltaVal) + minCirculationClearance);

                    return new LanePackage(sliceVal, selectedPrograms, sliceVal - activeVal);
                }

                //If selection is desk and something else, make room for two rows of desks and a middle row of the other program.
                if (firstProgram.AccessDirections == "1000" || secondProgram.AccessDirections == "1000")
                {
                    var deskProgramDims = firstProgram.AccessDirections == "1000" ? firstProgramDims : secondProgramDims;
                    var otherProgramDims = firstProgram.AccessDirections == "1000" ? secondProgramDims : firstProgramDims;

                    var sliceVal = activeVal + ((2 * deskProgramDims.Height) + (2 * minCirculationClearance) + minCirculationClearance);

                    return new LanePackage(sliceVal, selectedPrograms, sliceVal - activeVal);
                }

                //If neither program is a desk, split for one row of higher priority program.
                var newVal = activeVal + firstProgramDims.Height + minCirculationClearance;

                return new LanePackage(newVal, selectedPrograms, newVal - activeVal);
            }

            //If three programs are selected, select a configuration based on priority.
            if (selectedPrograms.Count == 3)
            {
                //Ordinal names used to make priority clear.
                var firstProgram = pm.ProgramPackages[selectedPrograms[0]];
                var firstProgramDims = new CurveBounds(firstProgram.OccupationBoundary);
                var secondProgram = pm.ProgramPackages[selectedPrograms[1]];
                var secondProgramDims = new CurveBounds(secondProgram.OccupationBoundary);
                var thirdProgram = pm.ProgramPackages[selectedPrograms[2]];
                var thirdProgramDims = new CurveBounds(thirdProgram.OccupationBoundary);

                var allDims = new List<CurveBounds>(new[] { firstProgramDims, secondProgramDims, thirdProgramDims });

                //If lane is private, slice for highest priority office.
                if (laneIsPrivate)
                {
                    var sliceVal = activeVal + firstProgramDims.Width;

                    return new LanePackage(sliceVal, selectedPrograms, sliceVal - activeVal);
                }

                //Identify which programs are desks.
                var deskCount = 0;
                var deskPrograms = new List<int>();

                for (int i = 0; i < selectedPrograms.Count; i++)
                {
                    if (pm.ProgramPackages[selectedPrograms[i]].AccessDirections == "1000")
                    {
                        deskCount++;
                        deskPrograms.Add(i);
                    }
                }

                //If two of the programs are desks, configure for two rows of higher priority desk.
                if (deskCount >= 2)
                {
                    var deskProgramDepths = new List<double>();
                    var otherProgramDepth = 0.0;

                    for (int i = 0; i < 3; i++)
                    {
                        if (deskPrograms.Contains(i))
                        {
                            deskProgramDepths.Add(allDims[i].Height);
                        }
                        else
                        {
                            otherProgramDepth = allDims[i].Height;
                        }
                    }

                    var sliceVal = activeVal + ((2 * deskProgramDepths[0]) + (2 * minCirculationClearance) + otherProgramDepth);

                    return new LanePackage(sliceVal, selectedPrograms, sliceVal - activeVal);
                }

                //If only one program is a desk, configure for two rows of the desk and a middle row of the higher priority program.
                if (deskCount == 1)
                {
                    var deskProgramDepth = allDims[deskPrograms[0]].Height;
                    var otherProgramDepths = new List<double>();

                    for (int i = 0; i < 3; i++)
                    {
                        if (!deskPrograms.Contains(i))
                        {
                            otherProgramDepths.Add(allDims[i].Height);
                        }
                    }

                    var sliceVal = activeVal + ((2 * deskProgramDepth) + (2 * minCirculationClearance) + otherProgramDepths[0]);

                    return new LanePackage(sliceVal, selectedPrograms, sliceVal - activeVal);
                }

                //If none of the programs are a desk, configure for one row of highest priority program.
                var deltaVal = activeVal + firstProgramDims.Height + minCirculationClearance;

                return new LanePackage(deltaVal, selectedPrograms, deltaVal - activeVal);
            }

            //Check for overfill due to error.
            if (selectedPrograms.Count > 3)
            {
                RhinoApp.WriteLine("LanePackage has been overfilled.");

                return null;
            }

            return null;
        }
    }

}
