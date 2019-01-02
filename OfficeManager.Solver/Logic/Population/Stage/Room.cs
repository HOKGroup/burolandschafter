using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Logic.Relationships;
using HOK.OfficeManager.Formats;
using Rhino.Geometry;

namespace HOK.OfficeManager.Logic.Population.Stage
{
    public class Room
    {
        #region anchors

        /// <summary>
        /// Container method for legibility in the staging method that references it.
        /// These methods determine the top left, right, and center points of a room based on its orientation.
        /// </summary>
        /// <param name="room"></param>
        public static void BaseAnchors(RoomPackage room)
        {
            room.BaseAnchorLeft = Stage.Room.FirstAnchorLeft(room);
            room.BaseAnchorCenter = Stage.Room.FirstAnchorCenter(room);
            room.BaseAnchorRight = Stage.Room.FirstAnchorRight(room);
        }

        public static Point3d FirstAnchorCenter(RoomPackage room)
        {
            var extentVal = 0.0;
            var placementVal = 0.0;

            var roomAxisY = room.OrientationPlane.YAxis;
            var roomAxisX = room.OrientationPlane.XAxis;

            if (Confirm.VectorProportionIsVertical(roomAxisY))
            {
                extentVal = roomAxisY.Y > 0 ? room.Dims.YMax : room.Dims.YMin;
                placementVal = room.OrientationPlane.OriginX;

                room.Median = placementVal;

                return new Point3d(placementVal, extentVal, 0);
            }
            else
            {
                extentVal = roomAxisY.X > 0 ? room.Dims.XMax : room.Dims.XMin;
                placementVal = room.OrientationPlane.OriginY;

                room.Median = placementVal;

                return new Point3d(extentVal, placementVal, 0);
            }
        }

        public static Point3d FirstAnchorLeft(RoomPackage room)
        {
            var extentVal = 0.0;
            var placementVal = 0.0;

            var roomAxisY = room.OrientationPlane.YAxis;
            var roomAxisX = room.OrientationPlane.XAxis;

            if (Confirm.VectorProportionIsVertical(roomAxisY))
            {
                extentVal = roomAxisY.Y > 0 ? room.Dims.YMax : room.Dims.YMin;
                placementVal = roomAxisX.X > 0 ? room.Dims.XMin : room.Dims.XMax;

                room.Lex = placementVal;

                return new Point3d(placementVal, extentVal, 0);
            }
            else
            {
                extentVal = roomAxisY.X > 0 ? room.Dims.XMax : room.Dims.XMin;
                placementVal = roomAxisX.Y > 0 ? room.Dims.YMin : room.Dims.YMax;

                room.Lex = placementVal;

                return new Point3d(extentVal, placementVal, 0);
            }
        }

        public static Point3d FirstAnchorRight(RoomPackage room)
        {
            var extentVal = 0.0;
            var placementVal = 0.0;

            var roomAxisY = room.OrientationPlane.YAxis;
            var roomAxisX = room.OrientationPlane.XAxis;

            if (Confirm.VectorProportionIsVertical(roomAxisY))
            {
                extentVal = roomAxisY.Y > 0 ? room.Dims.YMax : room.Dims.YMin;
                placementVal = roomAxisX.X > 0 ? room.Dims.XMax : room.Dims.XMin;

                room.Rex = placementVal;

                return new Point3d(placementVal, extentVal, 0);
            }
            else
            {
                extentVal = roomAxisY.X > 0 ? room.Dims.XMax : room.Dims.XMin;
                placementVal = roomAxisX.Y > 0 ? room.Dims.YMax : room.Dims.YMin;

                room.Rex = placementVal;

                return new Point3d(extentVal, placementVal, 0);
            }
        }

        public static Point3d NextAnchorRows(RoomPackage room, ProgramPackage program, Point3d anchor, double multiplier, double buffer)
        {
            var step = 0.0;

            if (Confirm.VectorProportionIsVertical(room.OrientationPlane.YAxis))
            {
                step = room.OrientationPlane.YAxis.Y > 0 ? program.Dims.Width * -1 : program.Dims.Width;
                step = (step * multiplier) + buffer;

                return new Point3d(anchor.X, anchor.Y + step, 0);
            }
            else
            {
                step = room.OrientationPlane.YAxis.X > 0 ? program.Dims.Width * -1 : program.Dims.Width;
                step = (step * multiplier) + buffer;

                return new Point3d(anchor.X + step, anchor.Y, 0);
            }
        }

        #endregion

        public static void ProgramFillOrder(RoomPackage room, ZonePackage zone, ProgramManifest pm)
        {
            var fillOrder = Enumerable.Range(0, zone.RemainingProgramTargets.Count)
                .Where(i => zone.RemainingProgramTargets[i] > 0 && pm.ProgramPackages[i].Quota != 0).ToList()
                .Select(x => pm.ProgramPackages[x])
                .OrderByDescending(x => x.Dims.Height).ToList()
                .Select(x => x.Priority).ToList();

            fillOrder.AddRange(pm.ProgramPackages
                .Where(x => x.Quota == 0).ToList()
                .Select(x => x.Priority));

            room.FillOrder = fillOrder;
        }

        public static void MaximumPlacements(RoomPackage room, ZonePackage zone, ProgramManifest pm)
        {
            var maxPlacement = room.FillOrder
                .Select(x => pm.ProgramPackages[x].Quota == 0 ? 999 : zone.RemainingProgramTargets[x]).ToList();

            room.MaxPlacement = maxPlacement;
        }
    }
}
