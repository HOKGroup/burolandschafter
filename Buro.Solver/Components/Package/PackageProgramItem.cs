using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Formats;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using HOK.Buro.Properties;

using HOK.Buro.Logic;


namespace HOK.Buro.Components
{
    public class PackageProgramItem : GH_Component
    {
        public PackageProgramItem() : base("Package Program", "Program", "Translate Rhino data into a program requirement for Office Manager.", Resources.TabName, "Packages")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Quota", "Q", "Desired number of items in final version. Set to 0 to request maximum amount.", GH_ParamAccess.item);
            pManager.AddTextParameter("Access", "A", "Type of accessibility. TODO: Find a way to not ask for numbers here.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Privacy", "P", "Set to true if private. (enclosed)", GH_ParamAccess.item);
            pManager.AddCurveParameter("Occupation Boundary", "B", "Extents of persistent occupation for this program item. NOTE: This does not include shared circulation.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Geometry", "g", "Collection of curves to be used for drawing the item once placed.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Program Item Package", "<P>", "Properly packaged program piece.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int quota = 0;
            DA.GetData(0, ref quota);

            string access = "0000";
            DA.GetData(1, ref access);

            bool isPrivate = false;
            DA.GetData(2, ref isPrivate);

            Curve boundary = null;
            DA.GetData(3, ref boundary);

            List<Curve> geometry = new List<Curve>();
            DA.GetDataList(4, geometry);
            
            ProgramPackage ProgramItem = new ProgramPackage(quota, access, isPrivate, boundary, geometry);

            DA.SetData(0, ProgramItem);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.buro_pkg_program;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("16b5a03a-bc6a-494b-888b-710ea1c81ffb"); }
        }
    }
}
