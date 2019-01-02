using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Formats;

namespace HOK.OfficeManager.Logic.Population
{
    public class Evaluate
    {
        public static void QuotasMet(ZoneManifest zm, ProgramManifest pm)
        {
            var programsPlaced = pm.ProgramPackages.Select(x => 0).ToList();

            foreach (ZonePackage zone in zm.Zones)
            {
                foreach (RoomPackage room in zone.Rooms)
                {
                    for (int i = 0; i < room.NumProgramsPlaced.Count; i++)
                    {
                        programsPlaced[i] = programsPlaced[i] + room.NumProgramsPlaced[i];
                    }
                }
            }

            zm.PlacementTotals = programsPlaced;

            //Utils.Debug.PrintList(ref programsPlaced);
        }
    }
}
