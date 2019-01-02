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
using HOK.OfficeManager.Logic.Relationships;
using HOK.OfficeManager.Logic.Population.Stage;
using HOK.OfficeManager.Properties;

namespace HOK.OfficeManager.Components
{
    public class Solver : GH_Component
    {
        public Solver() : base(Resources.Solver_Name, Resources.Solver_Label, Resources.Solver_Desc, Resources.TabName, Resources.Category_Main)
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Test Fit Package", "<T>", "Test fit instance to solve.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Solve", "S>", "Set to true to solve.", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Program Manifest", "<PM>", "Program information.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Zone Manifest", "<ZM>", "Zone information.", GH_ParamAccess.item);
            pManager.AddTextParameter("Debug", "d", "Debug text.", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            TestFitPackage tf = null;
            DA.GetData(0, ref tf);

            bool active = false;
            DA.GetData(1, ref active);

            if (!active)
            {
                return;
            }

            //Parse FloorPlanPackage for main working area(s).
            List<Brep> baseFloorRegions = Identify.FloorPlateRegions(tf);

            //Categorize circulation segments into "main" and "option" types.
            CirculationPackage circ = Identify.CirculationTypes(tf);

            //Sanitization: generate and remove all obstacles from workable space. (Circulation, Structure, etc.)
            List<Brep> validFloorRegions = Identify.AvailableFloorSpace(baseFloorRegions, tf, circ);

            //First-pass subdivision: coarse division based on proximity and proportion.
            List<Brep> optimizedFloorRegions = Identify.OptimalFloorSpaceConfiguration(validFloorRegions, tf);

            //Parse ProgramPackage(s) and format information/relationships into manifest.
            ProgramManifest pm = Identify.ProgramManifest(tf);

            //Assign program targets to each zone, based on priority + affinity, and subdivide to rooms.
            ZoneManifest zm = Identify.ZoneManifest(optimizedFloorRegions, pm, tf);

            //Populate zones and solve test fit.
            Terrain.Solution(zm, pm);

            List<string> debugText = new List<string>();

            foreach (ZonePackage zone in zm.Zones)
            {
                string output = null;

                foreach (int target in zone.ProgramTargets)
                {
                    output = output + target + " ";
                }

                debugText.Add(output);
            }

            DA.SetData(0, pm);
            DA.SetData(1, zm);
            DA.SetDataList(2, debugText);
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
            get { return new Guid("dec96e5d-03d8-4007-916a-9a3b39e1bbdf"); }
        }
    }
}
