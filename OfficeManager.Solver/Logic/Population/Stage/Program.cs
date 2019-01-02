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
    public class Program
    {
        /// <summary>
        /// Stages program with its base plane location and orientation aligned to room's.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="program"></param>
        /// <returns></returns>
        public static PlacementPackage InRoom(RoomPackage room, ProgramPackage program)
        {
            var roomPlane = new Plane(room.OrientationPlane);

            var newBounds = new Rectangle3d(roomPlane, new Interval(0, program.Dims.Width), new Interval(0, program.Dims.Height * -1)).ToNurbsCurve();

            return new PlacementPackage(program, roomPlane, newBounds);
        }

        /// <summary>
        /// Stages program in center of room. Distorts plane alignment, so only useful for debugging.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="program"></param>
        /// <returns></returns>
        public static PlacementPackage CenteredInRoom(RoomPackage room, ProgramPackage program)
        {
            var roomPlane = room.OrientationPlane;

            var newBounds = new Rectangle3d(roomPlane, new Interval(program.Dims.Width / -2, program.Dims.Width / 2), new Interval(program.Dims.Height / -2, program.Dims.Height / 2)).ToNurbsCurve();

            return new PlacementPackage(program, roomPlane, newBounds);
        }

        public static PlacementPackage ForLeftLane(RoomPackage room, PlacementPackage program)
        {
            //Rotate item and move it to starting point.
            program.Bounds.Rotate(Math.PI / 2, Vector3d.ZAxis, room.OrientationPlane.Origin);

            var shiftDir = new Vector3d(room.OrientationPlane.YAxis);
            shiftDir.Reverse();
            shiftDir = shiftDir * (program.Program.Dims.Width / shiftDir.Length);

            Transform move = Transform.Translation(room.BaseAnchorLeft - room.OrientationPlane.Origin);
            program.Bounds.Transform(move);

            //Identify new orientation plane's origin.
            var roomIsVertical = Math.Abs(room.OrientationPlane.YAxis.Y) > Math.Abs(room.OrientationPlane.YAxis.X);
            Point3d origin;

            if (roomIsVertical)
            {
                origin = room.OrientationPlane.YAxis.Y > 0 ? new Point3d(room.BaseAnchorLeft.X, room.BaseAnchorLeft.Y - program.Program.Dims.Width, 0) : new Point3d(room.BaseAnchorLeft.X, room.BaseAnchorLeft.Y + program.Program.Dims.Width, 0);
            }
            else
            {
                origin = room.OrientationPlane.YAxis.X > 0 ? new Point3d(room.BaseAnchorLeft.X - program.Program.Dims.Width, room.BaseAnchorLeft.Y, 0) : new Point3d(room.BaseAnchorLeft.X + program.Program.Dims.Width, room.BaseAnchorLeft.Y, 0);
            }

            //Generate new base plane.
            var stagedPlane = new Plane(origin, shiftDir, new Vector3d(room.OrientationPlane.XAxis) * -1);

            //Confirm item is still in zone. If not, compensate.
            if (Confirm.PointInRegion(room.Region, new CurveBounds(program.Bounds).Center, 0.1))
            {
                return new PlacementPackage(program.Program, stagedPlane, program.Bounds);
            }

            //Generate mirror plane and reflect bounds.
            var mirrorPlane = new Plane(room.OrientationPlane);
            mirrorPlane.Rotate(Math.PI / 2, mirrorPlane.YAxis);
            mirrorPlane.Origin = room.BaseAnchorLeft;

            Transform mirror = Transform.Mirror(mirrorPlane);

            program.Bounds.Transform(mirror);

            return new PlacementPackage(program.Program, stagedPlane, program.Bounds);
        }

        public static PlacementPackage ForRightLane(RoomPackage room, PlacementPackage program)
        {
            //Rotate item and move it to starting point.
            program.Bounds.Rotate(Math.PI / -2, Vector3d.ZAxis, room.OrientationPlane.Origin);

            var shiftDir = new Vector3d(room.OrientationPlane.YAxis);
            shiftDir.Reverse();
            shiftDir = shiftDir * (program.Program.Dims.Width / shiftDir.Length);

            Transform shift = Transform.Translation(shiftDir);
            Transform move = Transform.Translation(room.BaseAnchorRight - room.OrientationPlane.Origin);

            program.Bounds.Transform(shift);
            program.Bounds.Transform(move);

            //Identify new orientation plane's origin.
            var roomIsVertical = Math.Abs(room.OrientationPlane.YAxis.Y) > Math.Abs(room.OrientationPlane.YAxis.X);
            Point3d origin;

            if (roomIsVertical)
            {
                origin = room.OrientationPlane.YAxis.Y > 0 ? new Point3d(room.BaseAnchorRight.X, room.BaseAnchorRight.Y - program.Program.Dims.Width, 0) : new Point3d(room.BaseAnchorRight.X, room.BaseAnchorRight.Y + program.Program.Dims.Width, 0);
            }
            else
            {
                origin = room.OrientationPlane.YAxis.X > 0 ? new Point3d(room.BaseAnchorRight.X - program.Program.Dims.Width, room.BaseAnchorRight.Y, 0) : new Point3d(room.BaseAnchorRight.X + program.Program.Dims.Width, room.BaseAnchorRight.Y, 0);
            }

            //Generate new base plane.
            var stagedPlane = new Plane(origin, shiftDir, new Vector3d(room.OrientationPlane.XAxis));

            //Confirm item is still in zone. If not, compensate.
            if (Confirm.PointInRegion(room.Region, new CurveBounds(program.Bounds).Center, 0.1))
            {
                return new PlacementPackage(program.Program, stagedPlane, program.Bounds);
            }

            //Generate mirror plane and reflect bounds.
            var mirrorPlane = new Plane(room.OrientationPlane);
            mirrorPlane.Rotate(Math.PI / 2, mirrorPlane.YAxis);
            mirrorPlane.Origin = room.BaseAnchorRight;

            Transform mirror = Transform.Mirror(mirrorPlane);

            program.Bounds.Transform(mirror);

            return new PlacementPackage(program.Program, stagedPlane, program.Bounds);
        }

        public static PlacementPackage ForFrontRow(RoomPackage room, PlacementPackage program)
        {
            Transform move = Transform.Translation(room.BaseAnchorLeft - room.OrientationPlane.Origin);
            program.Bounds.Transform(move);

            var stagedPlane = new Plane(room.BaseAnchorLeft, new Vector3d(room.OrientationPlane.XAxis), new Vector3d(room.OrientationPlane.YAxis));

            return new PlacementPackage(program.Program, stagedPlane, program.Bounds);
        }
    }
}
