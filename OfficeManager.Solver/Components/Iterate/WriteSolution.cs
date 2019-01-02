using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Properties;

using Rhino;
using Grasshopper.Kernel;

namespace HOK.OfficeManager.Components
{
    public class WriteSolution : GH_Component
    {
        public WriteSolution() : base(Resources.WriteSolution_Name, Resources.WriteSolution_Label, Resources.WriteSolution_Desc, Resources.TabName, Resources.Category_Dispatch)
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Zone Manifest", "<ZM>", "Zone manifest from solution.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Program Manifest", "<PM>", "Program manifest from solution.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Formatted Solution", "(S)", "Solution data formatted for interpretation.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

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
            get { return new Guid("500f61a0-c86f-4517-afed-896f7f0e5e44"); }
        }
    }
}



