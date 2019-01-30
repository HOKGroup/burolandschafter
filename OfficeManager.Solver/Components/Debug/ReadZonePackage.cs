using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Formats;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;

using HOK.Buro.Logic;

namespace HOK.Buro.Components.Debug
{
    public class ReadZonePackage : GH_Component
    {
        public ReadZonePackage() : base("Read Zone Package", "ReadZone", "Outputs zone-level information from each package.", "Office Manager", "Debug")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Zone Package", "<Z>", "Zone geometry and metadata.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Region", "r", "Base region.", GH_ParamAccess.item);

            pManager.AddGenericParameter("br", "--", "break", GH_ParamAccess.item);

            pManager.AddCurveParameter("Perimeter Edges", "E.P", "Perimeter adjacent edges.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Circulation Edges", "E.Ci", "Circulatino adjacent edges.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Core Edges", "E.Co", "Core adjacent edges.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Exemption Edges", "E.E", "Exemption adjacent edges.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Structure Edges", "E.S", "Structure adjacent edges.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Zone Edges", "E.Z", "Zone adjacent edges.", GH_ParamAccess.list);

            pManager.AddGenericParameter("br", "--", "break", GH_ParamAccess.item);

            pManager.AddNumberParameter("Targets", "T", "Targets for each program.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ZonePackage zp = null;

            if (!DA.GetData(0, ref zp))
            {
                return;
            }

            DA.SetData(0, zp.Region);

            EdgeCurves edges = zp.EdgeCurves;

            DA.SetDataList(2, edges.PerimeterAdjacent);
            DA.SetDataList(3, edges.CirculationAdjacent);
            DA.SetDataList(4, edges.CoreAdjacent);
            DA.SetDataList(5, edges.ExemptionAdjacent);
            DA.SetDataList(6, edges.StructureAdjacent);
            DA.SetDataList(7, edges.ZoneAdjacent);

            DA.SetDataList(9, zp.ProgramTargets);
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
            get { return new Guid("d7ad90c7-5aa8-4feb-a261-9ea066d15adc"); }
        }
    }
}
