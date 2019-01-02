using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HOK.OfficeManager.Logic.Transformations;
using HOK.OfficeManager.Formats;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;

namespace HOK.OfficeManager.Logic
{
    public class Draw
    {
        public static void InputGeometry(ZonePackage zone)
        {
            foreach (RoomPackage room in zone.Rooms)
            {
                foreach (PlacementPackage item in room.PlacedItems)
                {
                    var activeProgram = item.Program;
                    var drawingGeometry = new List<Curve>();

                    foreach (Curve curve in activeProgram.DrawingGeometry)
                    {
                        drawingGeometry.Add(new NurbsCurve(curve.ToNurbsCurve()));
                    }

                    foreach (Curve geo in drawingGeometry)
                    {
                        geo.Transform(Transform.Translation(new Vector3d(item.Dims.Center - activeProgram.Dims.Center)));

                        geo.Transform(Transform.Rotation(Vector3d.YAxis, room.OrientationPlane.XAxis, item.Dims.Center));

                        //var testVector = new Vector3d(item.Dims.Center - room.Dims.Center);

                        if (room.Dims.Height > room.Dims.Width ? room.OrientationPlane.XAxis.X > 0 == item.Orientation.YAxis.X > 0 : room.OrientationPlane.XAxis.Y > 0 == item.Orientation.YAxis.Y > 0)
                        {
                            geo.Transform(Transform.Mirror(new Plane(item.Dims.Center, room.OrientationPlane.YAxis, Vector3d.ZAxis)));
                        }
                        else
                        {
                            
                        }
                    }

                    item.DrawingGeometry = drawingGeometry;
                }
            }
        }

        public static void OrientForRoom(Curve crv, PlacementPackage item, RoomPackage room)
        {
            crv.Transform(Transform.Rotation(Vector3d.YAxis, room.OrientationPlane.XAxis, item.Dims.Center));
        }

        //For speed, population logic and testing is done with boundaries. These methods transfer/generate curve information to make the solutions clear.
        public static List<Curve> StandardOffice(Curve bounds)
        {
            return DrawGeneric.CrossThroughSpace(bounds);
        }

        public static List<Curve> StandardSingleDesk(Curve bounds)
        {
            return DrawGeneric.CrossThroughSpace(bounds);
        }

        public static List<Curve> StandardPartyDesk(Curve bounds, int numSeats)
        {
            return DrawGeneric.CrossThroughSpace(bounds);
        }

        public static List<Curve> StandardPrivateMeetingSpace(Curve bounds, int numSeats)
        {
            return DrawGeneric.CrossThroughSpace(bounds);
        }

        public static List<Curve> StandardOpenMeetingSpace(Curve bounds, int numSeats)
        {
            return DrawGeneric.CrossThroughSpace(bounds);
        }
    }

    public class DrawGeneric
    {
        public static List<Curve> CrossThroughSpace(Curve bounds)
        {
            List<double> boundsPointsX = new List<double>();
            List<double> boundsPointsY = new List<double>();
            int spanCount = bounds.SpanCount;

            boundsPointsX.Add(bounds.PointAtStart.X);
            boundsPointsY.Add(bounds.PointAtStart.Y);

            for (int i = 0; i < spanCount; i++)
            {
                Interval spanDomain = bounds.SpanDomain(i);
                boundsPointsX.Add(bounds.PointAt(spanDomain.Max).X);
                boundsPointsY.Add(bounds.PointAt(spanDomain.Max).Y);
            }

            Point3d topRight = new Point3d(boundsPointsX.Max(), boundsPointsY.Max(), 0);
            Point3d topLeft = new Point3d(boundsPointsX.Min(), boundsPointsY.Max(), 0);
            Point3d bottomLeft = new Point3d(boundsPointsX.Min(), boundsPointsY.Min(), 0);
            Point3d bottomRight = new Point3d(boundsPointsX.Max(), boundsPointsY.Max(), 0);

            Curve crossA = Curves.TrimWithClosedCurve(bounds, new LineCurve(topRight, bottomLeft));
            Curve crossB = Curves.TrimWithClosedCurve(bounds, new LineCurve(topLeft, bottomRight));

            List<Curve> crossCurves = new List<Curve>();
            crossCurves.Add(crossA);
            crossCurves.Add(crossB);

            return crossCurves;
        }
    }
}
