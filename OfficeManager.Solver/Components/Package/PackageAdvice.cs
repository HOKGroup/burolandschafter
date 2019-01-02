using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Formats;
using HOK.OfficeManager.Logic;
using Rhino;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace HOK.OfficeManager.Components
{
    public class PackageAdvice : GH_Component
    {
        public PackageAdvice() : base("Package Advice", "Advice", "Translate Rhino data into a advice for Office Manager.", "Office Manager", "Packages")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type", "T", "Type of advice to offer.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Bounds", "B", "Region advice is being applied to.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Program Indices", "i", "Indices of programs to apply advice to.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Program Adjustments", "a", "Adjustment to be made to program.", GH_ParamAccess.list);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Advice Package", "<A>", "Properly packaged advice.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var type = "";
            DA.GetData(0, ref type);

            Curve bounds = null;
            DA.GetData(1, ref bounds);

            var indices = new List<int>();
            DA.GetDataList(2, indices);

            var adjustments = new List<double>();
            DA.GetDataList(3, adjustments);

            var advice = new AdvicePackage(type, bounds, indices, adjustments);

            DA.SetData(0, advice);
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
            get { return new Guid("a87190f8-6ae5-4513-8094-f645b97b6ea3"); }
        }
    }
}



