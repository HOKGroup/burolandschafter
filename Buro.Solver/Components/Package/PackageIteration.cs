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

namespace HOK.Buro.Components
{
    public class PackageIteration : GH_Component
    {
        public PackageIteration() : base(Resources.PackageIteration_Name, Resources.PackageIteration_Label, Resources.PackageIteration_Desc, Resources.TabName, Resources.Category_Packages)
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Solution Program Manifest", "<PM>", "Program manifest from previous solution.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Solution Zone Manifest", "<ZM>", "Zone manifest from previous solution.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Advice", "<A>", "List of advice to consider for next iteration.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Test Fit Package", "<T'>", "Test fit package formatted for next iteration. Can be run directly into solver.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ProgramManifest pmOld = null;
            if (!DA.GetData(0, ref pmOld))
            {
                return;
            }

            var pm = Utils.DeepClone(pmOld);

            ZoneManifest zmOld = null;
            if (!DA.GetData(1, ref zmOld))
            {
                return;
            }

            var zm = Utils.DeepClone(zmOld);

            var allAdvice = new List<AdvicePackage>();
            DA.GetDataList(2, allAdvice);

            if (pm.AllAdvice != null)
            {
                allAdvice.AddRange(pm.AllAdvice);
            }

            foreach (AdvicePackage advice in allAdvice)
            {
                Logic.Update.Iteration.ApplyAdvice(advice, zm, pm);
            }

            var tf = new TestFitPackage(zm.FloorPlan, pm.ProgramPackages, allAdvice);

            DA.SetData(0, tf);
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
            get { return new Guid("65a0d7e2-0b9b-4c49-9684-ae101d68b1d6"); }
        }
    }
}



