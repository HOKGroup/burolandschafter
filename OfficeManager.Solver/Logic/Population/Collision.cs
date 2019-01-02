using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Logic.Relationships;
using HOK.OfficeManager.Formats;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace HOK.OfficeManager.Logic.Population
{
    public class Collision
    {
        /// <summary>
        /// Basic collision detection routine for single item placement.
        /// </summary>
        /// <param name="room">Room currently being populated.</param>
        /// <param name="candidate">PlacementPackaged being checked for fidelity.</param>
        /// <param name="pm"></param>
        /// <returns></returns>
        public static bool PlacementIsValid(RoomPackage room, PlacementPackage candidate, ProgramManifest pm)
        {
            //Verify placed item does not clash with previously placed items.
            foreach (PlacementPackage placedItem in room.PlacedItems)
            {
                if (Confirm.CurvesIntersect(placedItem.Bounds, candidate.Bounds, true))
                {
                    if (Confirm.CurvesIntersect(placedItem.Bounds, candidate.Bounds, false))
                    {
                        return false;
                    }

                    CurveIntersections ccx = Intersection.CurveCurve(placedItem.Bounds, candidate.Bounds, 0.1, 0.1);

                    if (ccx.Count(x => x.IsOverlap) > 1)
                    {
                        return false;
                    }
                }

                PointContainment insideCheck = placedItem.Bounds.Contains(candidate.Bounds.GetBoundingBox(Plane.WorldXY).Center);

                if (insideCheck == PointContainment.Inside || insideCheck == PointContainment.Coincident)
                {
                    return false;
                }
            }

            //Verify item is completely within boundary of room.
            foreach (Curve edgeCurve in room.AllEdgeCurves)
            {
                if (Confirm.CurvesIntersect(candidate.Bounds, edgeCurve, false))
                {
                    return false;
                }

                /*
                if (!Confirm.PointInRegion(room.Region, new CurveBounds(candidate.Bounds).Center))
                {
                    return false;
                }
                */
            }

            //Verify program area is not larger than total room area.
            if (room.Region.GetArea() < pm.ProgramPackages[candidate.ProgramIndex].OccupationArea)
            {
                return false;
            }

            return true;
        }
    }
}
