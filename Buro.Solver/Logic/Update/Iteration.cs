using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Formats;
using Rhino.Geometry;

namespace HOK.Buro.Logic.Update
{
    public class Iteration
    {
        public static void ApplyAdvice(AdvicePackage advice, ZoneManifest zm, ProgramManifest pm)
        {
            var adviceDictionary = new Dictionary<string, Func<AdvicePackage, ZoneManifest, ProgramManifest, bool>>
            {
                { "Lock", ApplyLockRoom }
            };

            adviceDictionary[advice.Type](advice, zm, pm);
        }

        public static bool ApplyLockRoom(AdvicePackage advice, ZoneManifest zm, ProgramManifest pm)
        {
            var bounds = advice.Bounds;

            foreach (ZonePackage zone in zm.Zones)
            {
                foreach (RoomPackage room in zone.Rooms)
                {
                    foreach (PlacementPackage item in room.PlacedItems)
                    {
                        PointContainment pc = bounds.Contains(item.Dims.Center);

                        if (pc == PointContainment.Inside)
                        {
                            if (advice.CapturedProgram == null)
                            {
                                advice.CapturedProgram = new List<PlacementPackage>();
                            }

                            advice.CapturedProgram.Add(item);

                            if (pm.ProgramPackages[item.Program.Priority].Quota > 0)
                            {
                                pm.ProgramPackages[item.Program.Priority].Quota--;
                            }
                        }
                    }
                }
            }

            zm.FloorPlan.ExemptionProfiles.Add(advice.Bounds);

            return true;
        }
    }
}
