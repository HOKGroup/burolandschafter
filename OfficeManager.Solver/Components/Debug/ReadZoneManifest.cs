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
    public class ReadZoneManifest : GH_Component
    {
        public ReadZoneManifest() : base("Read Zone Manifest", "zm", "Outputs zm manifest information.", "Office Manager", "Debug")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Zone Manifest", "<ZM>", "Zone geometry and metadata.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ZonePackage", "<Z>", "List of individual zone packages.", GH_ParamAccess.list);
            pManager.AddBrepParameter("Region", "r", "Flat region geometry.", GH_ParamAccess.list);
            pManager.AddTextParameter("Targets", "t", "Program targets.", GH_ParamAccess.list);
            pManager.AddTextParameter("Popularities", "p", "Zone popularity.", GH_ParamAccess.list);
            pManager.AddNumberParameter("PlacedItems", "#", "Actual number of items placed.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ZoneManifest zm = null;

            if (!DA.GetData(0, ref zm))
            {
                return;
            }

            List<ZonePackage> zones = new List<ZonePackage>();

            List<Brep> regions = new List<Brep>();
            List<string> targets = new List<string>();
            List<string> popularities = new List<string>();

            foreach (ZonePackage zone in zm.Zones)
            {
                zones.Add(zone);

                regions.Add(zone.Region);

                string targetsCache = null;

                foreach (int val in zone.ProgramTargets)
                {
                    targetsCache = targetsCache + val.ToString().PadLeft(3, '0') + " ";
                }

                targets.Add(targetsCache);

                string popularityCache = null;

                foreach (int val in zone.Popularity)
                {
                    popularityCache = popularityCache + val.ToString().PadLeft(2, '0') + " ";
                }

                popularities.Add(popularityCache);
            }

            DA.SetDataList(0, zones);
            DA.SetDataList(1, regions);
            DA.SetDataList(2, targets);
            DA.SetDataList(3, popularities);
            DA.SetDataList(4, zm.PlacementTotals);
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
            get { return new Guid("0eab6c37-49b4-4121-91c1-209a6f4ff941"); }
        }
    }
}
