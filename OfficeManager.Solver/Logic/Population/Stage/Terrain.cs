using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Formats;

namespace HOK.OfficeManager.Logic.Population.Stage
{
    public class Terrain
    {
        /// <summary>
        /// Parent method for all population methods.
        /// </summary>
        /// <param name="zm"></param>
        /// <param name="pm"></param>
        public static void Solution(ZoneManifest zm, ProgramManifest pm)
        {
            for (int i = 0; i < zm.Zones.Count; i++)
            {
                //Prepare zone for population.
                var activeZone = zm.Zones[i];
                activeZone.RemainingProgramTargets = new List<int>(activeZone.ProgramTargets);

                //TODO: Figure out why population order is running opposite to slicing order so I don't have to do this.
                activeZone.Rooms.Reverse();

                //Loop through and populate every room in active zone.
                for (int j = 0; j < zm.Zones[i].Rooms.Count; j++)
                {
                    var activeRoom = activeZone.Rooms[j];
                    Stage.Terrain.Room(activeRoom, activeZone, pm);
                }

                //If zone fails to meet its predefined program targets, pass remainder on to next zone.
                if (i != zm.Zones.Count - 1)
                {
                    Update.Zone.PassUnfulfilledTargets(activeZone, zm.Zones[i + 1], pm);
                }

                Draw.InputGeometry(activeZone);
            }

            Evaluate.QuotasMet(zm, pm);
        }

        /// <summary>
        /// Selects best population strategy for room based on current conditions.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="zone"></param>
        /// <param name="pm"></param>
        public static void Room(RoomPackage room, ZonePackage zone, ProgramManifest pm)
        {
            //Prepare room for population routine based on current fill progress.
            room.PlacedItems = new List<PlacementPackage>();

            Stage.Room.BaseAnchors(room);
            Stage.Room.ProgramFillOrder(room, zone, pm);
            Stage.Room.MaximumPlacements(room, zone, pm);

            //Select best population strategy based on room geometry and quotas.
            //TODO: Automate selection process and write base suite of strategies.
            Populate.ByMostRows(room, zone, pm);

            //After room is filled, adjust remaining quota for zone.
            Update.Zone.RemainingProgramTargets(zone, room);
        }
    }
}
