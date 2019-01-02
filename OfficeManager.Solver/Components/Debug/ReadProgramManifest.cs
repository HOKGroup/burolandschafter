using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Formats;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;

using HOK.OfficeManager.Logic;

namespace HOK.OfficeManager.Components.Debug
{
    public class ReadProgramManifest : GH_Component
    {
        public ReadProgramManifest() : base("Read Program Manifest", "pm", "Outputs program manifest information.", "Office Manager", "Debug")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Program Manifest", "<PM>", "Program geometry and metadata.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Program Packages", "<P>", "List of program packages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Advice Packages", "<A>", "All advice considered in previous version.", GH_ParamAccess.list);
            pManager.AddTextParameter("Zone Preference", "z", "Zone indices in order of preference.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Distribution", "d", "Percent of floor plan allocated to this program.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Priority", "p", "Program priority.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ProgramManifest pm = null;

            if (!DA.GetData(0, ref pm))
            {
                return;
            }

            DA.SetDataList(0, pm.ProgramPackages);
            DA.SetDataList(1, pm.AllAdvice);

            List<string> zonePreferences = new List<string>();
            List<double> distributions = new List<double>();
            List<int> priorities = new List<int>();

            foreach (ProgramPackage program in pm.ProgramPackages)
            {
                string preferenceCache = null;

                foreach (int val in program.ZonePreference)
                {
                    preferenceCache = preferenceCache + val + " ";
                }

                zonePreferences.Add(preferenceCache);

                distributions.Add(program.Distribution);

                priorities.Add(program.Priority);
            }

            DA.SetDataList(2, zonePreferences);
            DA.SetDataList(3, distributions);
            DA.SetDataList(4, priorities);
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
            get { return new Guid("30af6c36-e8dd-4ffa-9d34-8857f8b04045"); }
        }
    }
}
