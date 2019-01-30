using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HOK.Buro.Logic.Relationships;
using HOK.Buro.Logic.Transformations;
using HOK.Buro.Formats;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Rhino.Geometry.Intersect;

namespace HOK.Buro.Logic.Update
{
    public class Room
    {
        /// <summary>
        /// Slices each zone into rectangular "lanes" based on dimensions of program targets.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="zone"></param>
        /// <param name="pm"></param>
        public static void LaneConfiguration(Brep region, ZonePackage zone, ProgramManifest pm)
        {
            var zoneTargets = new List<int>(zone.ProgramTargets);
            var totalProjectedFill = new List<int>();

            foreach (int target in zoneTargets)
            {
                totalProjectedFill.Add(0);
            }

            var regionInfo = new CurveBounds(Utils.GetBoundingBoxCurve(region));

            var splitterCurves = new List<Curve>();
            var sliceDirectionIsVertical = Confirm.RegionProportionIsVertical(region);

            var startVal = sliceDirectionIsVertical ? regionInfo.YMin : regionInfo.XMin;
            var maxVal = sliceDirectionIsVertical ? regionInfo.YMax : regionInfo.XMax;

            var activeSliceVal = startVal;

            var allLanePackages = new List<LanePackage>();

            //Preflight checks.
            var targets = Confirm.Zone.TargetFulfilled(totalProjectedFill, zone.ProgramTargets);
            var inbounds = activeSliceVal < maxVal;

            //RhinoApp.WriteLine("Targets fulfilled: {0} | In bounds: {1}", targets, inbounds);

            while (!Confirm.Zone.TargetFulfilled(totalProjectedFill, zone.ProgramTargets) && activeSliceVal < maxVal)
            {
                //Slice lane based on remaining program.
                LanePackage lp = Select.NextLanePayload(zone, pm, totalProjectedFill, activeSliceVal, maxVal);
                allLanePackages.Add(lp);

                //RhinoApp.WriteLine("{0} => {1}", activeSliceVal, lp.SlicePosition);

                activeSliceVal = lp.SlicePosition;

                var splitter = sliceDirectionIsVertical ? new LineCurve(new Point2d(regionInfo.XMin, activeSliceVal), new Point2d(regionInfo.XMax, activeSliceVal)) : new LineCurve(new Point2d(activeSliceVal, regionInfo.YMax), new Point2d(activeSliceVal, regionInfo.YMin));

                splitterCurves.Add(splitter);

                Update.Room.ProjectedFill(lp, region, splitterCurves, pm);

                for (int i = 0; i < lp.ProjectedFill.Count; i++)
                {
                    totalProjectedFill[i] = totalProjectedFill[i] + lp.ProjectedFill[i];
                }
            }

            zone.LanePackages = allLanePackages;

            var zoneSlices = Breps.SplitByCurves(region, splitterCurves);

            //RhinoApp.WriteLine("{0} splitter curves & {1} rooms.", splitterCurves.Count.ToString(), zoneSlices.Count);

            foreach (Brep lane in zoneSlices)
            {
                zone.Rooms.Add(new RoomPackage(lane, pm.ProgramPackages.Count));
            }
        }

        /// <summary>
        /// Method responsible for estimating the effectiveness of each lane.
        /// Plan is to use this in the future to segment lanes if needed.
        /// </summary>
        /// <param name="lp"></param>
        /// <param name="region">Region for entire zone.</param>
        /// <param name="splitters">All currently determined splitters.</param>
        /// <param name="pm"></param>
        public static void ProjectedFill(LanePackage lp, Brep region, List<Curve> splitters, ProgramManifest pm)
        {
            //Determine approximate depth of lane.
            var depth = 0.0;

            if (splitters.Count == 1)
            {
                depth = Curves.TrimWithClosedCurve(Utils.GetRegionPerimeter(region), splitters[0]).GetLength();
            }
            else
            {
                var depthL = Curves.TrimWithClosedCurve(Utils.GetRegionPerimeter(region), splitters[splitters.Count - 2]).GetLength();
                var depthR = Curves.TrimWithClosedCurve(Utils.GetRegionPerimeter(region), splitters[splitters.Count - 1]).GetLength();

                depth = (depthL + depthR) / 2;
            }

            lp.Depth = depth;

            lp.ProjectedFill = new List<int>();

            foreach (ProgramPackage program in pm.ProgramPackages)
            {
                lp.ProjectedFill.Add(0);
            }

            for (int i = 0; i < lp.Programs.Count; i++)
            {
                var selectedProgramIndex = lp.Programs[i];
                var selectedProgram = pm.ProgramPackages[selectedProgramIndex];
                var selectedProgramDims = selectedProgram.Dims;

                var estimatedFill = Convert.ToInt32(Math.Floor(depth / selectedProgramDims.Width));

                if (selectedProgram.AccessDirections == "1000")
                {
                    estimatedFill = estimatedFill * 2;
                }

                lp.ProjectedFill[selectedProgramIndex] = estimatedFill;
            }
        }

        /// <summary>
        /// Generates a base plane for each room with +Y pointing towards circulation.
        /// Necessary to normalize placement of program across highly varied room geometry.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="zone"></param>
        public static void Orientation(RoomPackage room, ZonePackage zone)
        {
            var circEdge = Select.PrimaryCirculationEdge(zone);
            var basePoint = Utils.GetRegionCenter(room.Region);

            circEdge.ClosestPoint(basePoint, out double t);
            var circEdgePoint = circEdge.PointAt(t);

            var yDirVector = new Vector3d(circEdgePoint - basePoint);

            var yTestVal = Confirm.VectorProportionIsVertical(yDirVector) ? yDirVector.Y : yDirVector.X;

            var yAxis = Confirm.VectorProportionIsVertical(yDirVector) ? new Vector3d(0, yTestVal, 0) : new Vector3d(yTestVal, 0, 0);
            var xAxis = new Vector3d(yAxis);
            xAxis.Rotate(Math.PI / 2, Vector3d.ZAxis);

            var roomOrientationPlane = new Plane(basePoint, xAxis, yAxis);

            room.OrientationPlane = roomOrientationPlane;
        }

        /// <summary>
        /// Extracts indices of programs each lane was "designed for" during Update.Room.LaneConfiguration.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="zone"></param>
        /// <param name="i"></param>
        public static void ProgramHint(RoomPackage room, ZonePackage zone, int i)
        {
            room.ProgramHint = i >= zone.LanePackages.Count ? new List<int>() : zone.LanePackages[i].Programs;
        }
    }
}
