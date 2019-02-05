using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Formats;
using HOK.Buro.Logic;
using HOK.Buro.Properties;
using Rhino;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace HOK.Buro.Components.Debug
{
    public class ReadAdvicePackage : GH_Component
    {
        public ReadAdvicePackage() : base(Resources.ReadAdvice_Name, Resources.ReadAdvice_Label, Resources.ReadAdvice_Desc, Resources.TabName, Resources.Category_Debug)
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Advice Package", "<A>", "Advice to read.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Placed Items", "P", "Curve data for placed programs.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var allAdvice = new List<AdvicePackage>();
            DA.GetDataList(0, allAdvice);

            var drawingGeometry = new List<Curve>();

            foreach (AdvicePackage advice in allAdvice)
            {
                if (advice.CapturedProgram == null)
                {
                    continue;
                }

                foreach (PlacementPackage program in advice.CapturedProgram)
                {
                    foreach (Curve crv in program.DrawingGeometry)
                    {
                        drawingGeometry.Add(crv);
                    }
                }
            }

            DA.SetDataList(0, drawingGeometry);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.buro_read_advice;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("60084edd-9d24-4086-a74d-db43aa287c78"); }
        }
    }
}



