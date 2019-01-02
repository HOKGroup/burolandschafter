using System;
using System.Collections.Generic;
using System.Drawing.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Formats;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;

using HOK.OfficeManager.Logic;
using HOK.OfficeManager.Properties;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace HOK.OfficeManager.Components.Debug
{
    public class ReadRoomPackage : GH_Component
    {
        public ReadRoomPackage() : base(Resources.GetRooms_Name, Resources.GetRooms_Label, Resources.GetRooms_Desc, Resources.TabName, Resources.Category_Debug)
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Zone Package", "<Z>", "Zone geometry and metadata.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Room Regions", "R", "Region geometry for rooms.", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Program placement.", "P", "Placed items.", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Orientation Plane", "Pl", "Room orientation plane. +Y should face towards circulation.", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Anchor", "A", "Centered anchor point.", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Drawing geometry", "D", "Input geometry for program representation.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Drawing geometry count.", "Dn", "Number of curves used to draw geometry.", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Center", "C", "Center based on item.Dims", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ZonePackage zp = null;

            if (!DA.GetData(0, ref zp))
            {
                return;
            }

            var roomRegions = new List<Brep>();
            var placedBounds = new List<Curve>();
            var roomPlanes = new List<Plane>();
            var anchorPoints = new List<Point3d>();
            var drawingGeometry = new List<Curve>();
            var drawingGeometryCount = new List<int>();
            var roomCenter = new List<Point3d>();

            foreach (RoomPackage room in zp.Rooms)
            {
                roomRegions.Add(room.Region);
                roomPlanes.Add(room.OrientationPlane);
                anchorPoints.Add(room.PrevAnchor);

                foreach (PlacementPackage placedItem in room.PlacedItems)
                {
                    placedBounds.Add(placedItem.Bounds);
                    roomCenter.Add(placedItem.Dims.Center);

                    drawingGeometryCount.Add(placedItem.DrawingGeometry.Count);

                    foreach (Curve geo in placedItem.DrawingGeometry)
                    {
                        drawingGeometry.Add(geo);
                    }
                }
            }

            DA.SetDataList(0, roomRegions);
            DA.SetDataList(1, placedBounds);
            DA.SetDataList(2, roomPlanes);
            DA.SetDataList(3, anchorPoints);
            DA.SetDataList(4, drawingGeometry);
            DA.SetDataList(5, drawingGeometryCount);
            DA.SetDataList(6, roomCenter);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("2ff20b39-4e6c-4a24-b10e-bdf7782aab75"); }
        }
    }
}
