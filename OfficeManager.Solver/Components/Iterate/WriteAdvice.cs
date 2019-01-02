using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Formats;
using HOK.OfficeManager.Logic;
using HOK.OfficeManager.Logic.Translations;
using HOK.OfficeManager.Properties;
using Rhino;
using Grasshopper.Kernel;

namespace HOK.OfficeManager.Components
{
    public class WriteAdvice : GH_Component
    {
        public WriteAdvice() : base(Resources.WriteAdvice_Name, Resources.WriteAdvice_Label, Resources.WriteAdvice_Desc, Resources.TabName, Resources.Category_Dispatch)
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Advice Package", "<A>", "Advice to format for interpretation.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Formatted Advice", "(A)", "Formatted advice for interpretation.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var allAdvice = new List<AdvicePackage>();

            if (!DA.GetDataList(0, allAdvice))
            {
                return;
            }

            var data = new List<string>();

            allAdvice.ForEach(x => data.Add(Advice.ParseAdvice(x)));

            DA.SetDataList(0, data);
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
            get { return new Guid("7bd66c8f-e66e-428c-9076-74de167293e5"); }
        }
    }
}



