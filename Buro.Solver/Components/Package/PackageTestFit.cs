using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Formats;
using HOK.Buro.Properties;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;

using HOK.Buro.Logic;
using HOK.Buro.Logic.Relationships;

namespace HOK.Buro.Components
{
    public class PackageTestFit : GH_Component
    {
        public PackageTestFit() : base("Package Test Fit", "Test Fit", "Collect packages and format into a test fit instance.", Resources.TabName, "Packages")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Floor Plan Package", "<F>", "Floor plan to use as test fit terrain.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Program Packages", "<P>", "Collection of program information and quotas.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Test Fit Package", "<T>", "Complete data package to be used by solver.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FloorPlanPackage floorPlan = null;
            DA.GetData(0, ref floorPlan);

            List<ProgramPackage> programInfo = new List<ProgramPackage>();
            DA.GetDataList(1, programInfo);

            TestFitPackage TestFit = new TestFitPackage(floorPlan, programInfo);

            //Perform tests for data fidelity.
            bool allTestsPassed = Confirm.TestFit.Fidelity(TestFit);

            if (!allTestsPassed)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Test Fit Package is not valid.");
            }

            DA.SetData(0, TestFit);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.buro_pkg_testfit;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("5f4592c8-d28e-42c0-935a-646281c446dc"); }
        }
    }
}
