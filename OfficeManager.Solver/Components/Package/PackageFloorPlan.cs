using System;
using System.Collections.Generic;
using HOK.OfficeManager.Formats;
using Rhino.Geometry;
using Grasshopper.Kernel;
using HOK.OfficeManager.Logic;

namespace HOK.OfficeManager.Components
{
    public class PackageFloorPlan : GH_Component
    {
        public PackageFloorPlan() : base("Package Floor Plan", "Floor Plan", Properties.Resources.PackageFloorPlan_Desc, Properties.Resources.TabName, "Packages")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Floor Profile", "P", "Single closed curve representing interior edge of floor plate geometry.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Core Profile", "P'", "Single closed curve representing extents of building's core.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Other Exemptions", "X", "Closed curve representations of other regions to leave unpopulated by program.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Structure", "S", "Closed curve representations of columns and other structural obstructions.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Desired Circulation", "C", "Axial representation of desired circulation paths through the floor.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Core Access", "C'", "Regions of the core profile that must be left unobstructed to allow proper egress and access.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Floor Plan Package", "<F>", "Properly packaged plan pieces.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve mainProfile = null;
            DA.GetData(0, ref mainProfile);

            Curve coreProfile = null;
            DA.GetData(1, ref coreProfile);

            var exemptions = new List<Curve>();
            DA.GetDataList(2, exemptions);

            var structure = new List<Curve>();
            DA.GetDataList(3, structure);

            var circulation = new List<Curve>();
            DA.GetDataList(4, circulation);

            var coreAccess = new List<Curve>();
            DA.GetDataList(5, coreAccess);

            var FloorPlan = new FloorPlanPackage(mainProfile, coreProfile, exemptions, structure, circulation, coreAccess);

            DA.SetData(0, FloorPlan);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //return Resources.Icon
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("3c2e789b-e179-4751-af5e-032d485d7a3d"); }
        }
    }
}
