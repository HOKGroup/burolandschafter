using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HOK.Buro.Logic.Relationships;
using HOK.Buro.Logic.Transformations;
using HOK.Buro.Formats;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Rhino.Geometry.Intersect;

namespace HOK.Buro.Logic.Update
{
    public class Zone
    {
        public static void EdgeClassification(ZonePackage zp, EdgeCurves ec)
        {
            zp.EdgeCurves = ec;
        }

        public static void Adjacencies(ZonePackage zp)
        {
            zp.IsCoreAdjacent = (zp.EdgeCurves.CoreAdjacent.Count > 0) ? true : false;
            zp.IsPerimeterAdjacent = (zp.EdgeCurves.PerimeterAdjacent.Count > 0) ? true : false;
            zp.IsIsland = (!zp.IsCoreAdjacent && !zp.IsPerimeterAdjacent) ? true : false;
        }

        public static void AffinityType(ZonePackage zp)
        {
            if (zp.IsCoreAdjacent)
            {
                zp.AffinityType = 1;
            }
            else if (zp.IsPerimeterAdjacent)
            {
                zp.AffinityType = 2;
            }
            else
            {
                zp.AffinityType = 3;
            }
        }

        public static void Popularity(ZoneManifest zm, ProgramManifest pm)
        {
            int zoneCount = zm.Zones.Count;

            for (int i = 0; i < zoneCount; i++)
            {
                List<int> popularity = new List<int>(zoneCount);

                for (int j = 0; j < zoneCount; j++)
                {
                    int wantedCounter = 0;

                    foreach (ProgramPackage program in pm.ProgramPackages)
                    {
                        if (program.ZonePreference[j] == i)
                        {
                            wantedCounter++;
                        }
                    }

                    popularity.Add(wantedCounter);
                }

                zm.Zones[i].Popularity = popularity;
            }
        }

        public static void ProgramPriority(ZoneManifest zm, ProgramManifest pm)
        {
            int zoneCount = zm.Zones.Count;
            int programCount = pm.ProgramPackages.Count;

            for (int i = 0; i < zoneCount; i++)
            {
                List<int> programPriority = new List<int>();

                for (int j = 0; j < zoneCount; j++)
                {
                    //Skip if we know zone is unloved at this priority.
                    if (zm.Zones[i].Popularity[j] == 0)
                    {
                        continue;
                    }

                    List<ProgramPackage> suitors = new List<ProgramPackage>();
                    List<int> programIndex = new List<int>();

                    //Grab programs that want to be with this zone, in order of preference.
                    for (int k = 0; k < programCount; k++)
                    {
                        if (pm.ProgramPackages[k].ZonePreference[j] == i)
                        {
                            suitors.Add(pm.ProgramPackages[k]);
                            programIndex.Add(k);
                        }
                    }

                    //In the case of multiple suitors, tiebreak based on program priority.
                    if (suitors.Count == 1)
                    {
                        programPriority.Add(programIndex[0]);
                    }
                    else
                    {
                        for (int k = 0; k < programCount; k++)
                        {
                            for (int t = 0; t < suitors.Count; t++)
                            {
                                if (suitors[t].Priority == k)
                                {
                                    programPriority.Add(programIndex[t]);
                                }
                            }
                        }
                    }
                }

                zm.Zones[i].ProgramPriority = programPriority;
            }
        }

        public static void ReservedArea(ZoneManifest zm, ProgramManifest pm)
        {
            int zoneCount = zm.Zones.Count;
            int programCount = pm.ProgramPackages.Count;

            for (int i = 0; i < zoneCount; i++)
            {
                List<List<double>> reservedArea = new List<List<double>>();

                for (int j = 0; j < zoneCount; j++)
                {
                    List<double> reservedAtThisPriority = new List<double>();

                    for (int k = 0; k < programCount; k++)
                    {
                        if (zm.Zones[i].Popularity[j] == 0)
                        {
                            reservedAtThisPriority.Add(0);
                        }
                        else
                        {
                            double portion = zm.Zones[i].BaseArea / zm.Zones[i].Popularity[j];

                            if (pm.ProgramPackages[k].ZonePreference[j] == i)
                            {
                                reservedAtThisPriority.Add(portion);
                            }
                            else
                            {
                                reservedAtThisPriority.Add(0);
                            }
                        }
                    }

                    reservedArea.Add(reservedAtThisPriority);
                }

                zm.Zones[i].ReservedArea = reservedArea;
            }
        }

        /// <summary>
        /// Negotiates program quotas, affinities, and priority to assign each zone a "sub-quota" to meet.
        /// </summary>
        /// <param name="zm"></param>
        /// <param name="pm"></param>
        public static void ProgramTargets(ZoneManifest zm, ProgramManifest pm)
        {
            int numZones = zm.Zones.Count;
            int numPrograms = pm.ProgramPackages.Count;

            #region area based distribution
            //Distribute targets between zones based on their proportion of the total area.
            foreach (ZonePackage zone in zm.Zones)
            {
                List<double> proportionalTargets = new List<double>();

                foreach (ProgramPackage program in pm.ProgramPackages)
                {
                    proportionalTargets.Add(program.Target * (zone.BaseArea / zm.TotalArea));
                }

                zone.ProportionalTargets = proportionalTargets;
            }
            #endregion

            #region safe rounding to integers
            //Round these proportional targets while maintaining overall quota total.
            List<int> newTargets = new List<int>();

            for (int i = 0; i < numPrograms; i++)
            {
                int activeTarget = pm.ProgramPackages[i].Target;
                List<double> distForActiveProgram = new List<double>();

                foreach (ZonePackage zone in zm.Zones)
                {
                    distForActiveProgram.Add(zone.ProportionalTargets[i]);
                }

                newTargets = Utils.RoundWithinTotal(distForActiveProgram, activeTarget);

                for (int j = 0; j < zm.Zones.Count; j++)
                {
                    if (i == 0)
                    {
                        zm.Zones[j].ProgramTargets = new List<int>();
                    }
                    //RhinoApp.WriteLine(zm.Zones[j].ProgramTargets.Count.ToString());

                    zm.Zones[j].ProgramTargets.Add(newTargets[j]);
                }
            }
            #endregion

            #region horse trading
            //In reverse-prioroity order, maximize program target in zone.
            for (int i = numPrograms - 1; i >= 0; i--)
            {
                ProgramPackage activeProgram = pm.ProgramPackages[i];

                //Decrement by zone target, on move into new zone, and by one for each trade.
                int remainingToMove = activeProgram.Target;

                //Increment when activeZone runs out of ejectionProgram quota to send. Cannot be >= numPrograms.
                int ejectionIndexModifier = 0;

                //Increment when sourceZone runs out of activeProgram quota to pull. Cannot be >= numZones.
                int sourceZoneIndexModifier = 0;

                //RhinoApp.WriteLine("---Beginning horse trade for program {0}.", i.ToString());

                for (int j = 0; j < numZones; j++)
                {
                    //Prepare base information for start of trading procedure in each zone.
                    ZonePackage activeZone = zm.Zones[activeProgram.ZonePreference[j]];

                    ejectionIndexModifier = 0;
                    sourceZoneIndexModifier = 0;

                    double baseReservedArea = activeZone.ReservedArea[j][i];
                    double reservedAreaRemaining = baseReservedArea - (activeZone.ProgramTargets[i] * activeProgram.OccupationArea);

                    //RhinoApp.WriteLine(String.Format("*Maximizing zone {0} (#{1} - {3}) ({2} sqft available.)", activeProgram.ZonePreference[j].ToString(), j.ToString(), reservedAreaRemaining.ToString(), activeZone.ToString()));

                    if (reservedAreaRemaining < 0)
                    {
                        //Often for maximized program, and sometimes for others, the default distribution will be larger than the reserved area.
                        //This is most common when the zone is desired by several programs.
                        //Since the current setup solves from lowest priority to highest, the higher priority programs will kick out the others.

                        //RhinoApp.WriteLine("...negative remaining area. Skip!");

                        continue;
                    }

                    //Pull from zone that activeProgram least prefers.
                    while (zm.Zones[activeProgram.ZonePreference[numZones - (sourceZoneIndexModifier + 1)]].ProgramTargets[i] <= 0)
                    {
                        //RhinoApp.WriteLine(">No program in zone {0} available to pull ({1}), incrementing zone.", activeProgram.ZonePreference[(numZones - (sourceZoneIndexModifier + 1))].ToString(), zm.Zones[activeProgram.ZonePreference[numZones - (sourceZoneIndexModifier + 1)]].ToString());
                        sourceZoneIndexModifier++;

                        if (sourceZoneIndexModifier >= numZones)
                        {
                            sourceZoneIndexModifier--;
                            break;
                        }
                    }

                    ZonePackage sourceZone = zm.Zones[activeProgram.ZonePreference[numZones - (sourceZoneIndexModifier + 1)]];

                    //RhinoApp.WriteLine("First source zone: {0}", sourceZone.ToString());

                    //Eject program from activeZone that least wants to be there.
                    //Its preference for the sourceZone is not considered. It should be thankful to be able to leave the place it hates so much.
                    while (activeZone.ProgramTargets[activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)]] <= 0)
                    {
                        int activeIndex = activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)];
                        //RhinoApp.WriteLine(">No program {0} available to eject, incrementing program.", activeIndex.ToString());
                        ejectionIndexModifier++;

                        if (ejectionIndexModifier >= numPrograms)
                        {
                            ejectionIndexModifier--;
                            break;
                        }
                    }

                    int programToEjectIndex = activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)];

                    //RhinoApp.WriteLine("First program to eject: {0}", programToEjectIndex.ToString());

                    double ejectedProgramSizeCache = 0;

                    int currentLoopEjectedProgramDelta = 0;

                    //Preflight tests:
                    bool remaining = (remainingToMove != 0);
                    bool availableToEject = (activeZone.ProgramTargets[programToEjectIndex] > 0);
                    bool reservedAvailable = (reservedAreaRemaining > 0);
                    bool availableToPull = (sourceZone.ProgramTargets[i] > 0);

                    //RhinoApp.WriteLine(String.Format("Preflight tests: {0} | {1} | {2} | {3}", remaining, availableToEject, reservedAvailable, availableToPull));

                    //While there is still activeProgram to optimize, other program to send from activeZone, reserved area to fill in activeZone, and program to receive from sourceZone...
                    //Proceed with horse trading.
                    while (remainingToMove != 0 && activeZone.ProgramTargets[programToEjectIndex] > 0 && reservedAreaRemaining > 0 && sourceZone.ProgramTargets[i] > 0)
                    {
                        //Skip loop if trying to eject self.
                        if (programToEjectIndex == i)
                        {
                            ejectionIndexModifier++;

                            if (ejectionIndexModifier >= numPrograms)
                            {
                                break;
                            }

                            programToEjectIndex = activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)];

                            //RhinoApp.WriteLine(">Self-ejection detected, moving on to program {0}.", programToEjectIndex);
                            continue;
                        }

                        //Make necessary measurements for potential horse trade.
                        int currentLoopActiveProgramDelta = 0;
                        currentLoopEjectedProgramDelta++;

                        double sizeOfProgramToEject = (pm.ProgramPackages[programToEjectIndex].OccupationArea * currentLoopEjectedProgramDelta) + ejectedProgramSizeCache;

                        //RhinoApp.WriteLine("Blip.");
                        //RhinoApp.WriteLine("Testing swap of {0}x program {1}. (Area {2}% filled.)", currentLoopEjectedProgramDelta.ToString(), programToEjectIndex.ToString(), ((sizeOfProgramToEject / activeProgram.OccupationArea) * 100).ToString());

                        //Because the area of one staged program may be multiple factors larger than the area of the activeProgram, we can't just increment by one.
                        if (sizeOfProgramToEject > activeProgram.OccupationArea)
                        {
                            int potentialSwap = Convert.ToInt32(Math.Floor(sizeOfProgramToEject / activeProgram.OccupationArea));
                            int remainingAtSource = sourceZone.ProgramTargets[i];

                            while (remainingToMove > 0 && potentialSwap > 0)
                            {
                                potentialSwap--;
                                remainingToMove--;

                                currentLoopActiveProgramDelta++;
                            }

                            //RhinoApp.WriteLine(String.Format("Trade initiated! {0} ejected ({1} still available) | {2} pulled ({3} remaining at source)", currentLoopEjectedProgramDelta.ToString(), activeZone.ProgramTargets[programToEjectIndex].ToString(), currentLoopActiveProgramDelta.ToString(), sourceZone.ProgramTargets[i].ToString()));

                            reservedAreaRemaining = reservedAreaRemaining - sizeOfProgramToEject;

                            //RhinoApp.WriteLine("{0} reserved area left to fill.", reservedAreaRemaining.ToString());

                            ejectedProgramSizeCache = 0;
                        }

                        //If a trade has been identified, complete it and check for necessary changes before next loop.
                        if (currentLoopActiveProgramDelta > 0)
                        {
                            sourceZone.ProgramTargets[i] = sourceZone.ProgramTargets[i] - currentLoopActiveProgramDelta;
                            activeZone.ProgramTargets[i] = activeZone.ProgramTargets[i] + currentLoopActiveProgramDelta;

                            activeZone.ProgramTargets[programToEjectIndex] = activeZone.ProgramTargets[programToEjectIndex] - currentLoopEjectedProgramDelta;
                            sourceZone.ProgramTargets[programToEjectIndex] = sourceZone.ProgramTargets[programToEjectIndex] + currentLoopEjectedProgramDelta;

                            remainingToMove = remainingToMove - currentLoopActiveProgramDelta;

                            currentLoopEjectedProgramDelta = 0;
                            currentLoopActiveProgramDelta = 0;

                            if (remainingToMove == 0)
                            {
                                //Operation is complete, move to next program. (There is a second break outside of the while loop for this.)
                                activeZone.ReservedArea[j][i] = reservedAreaRemaining;

                                if (sourceZone.ProgramTargets[i] < 0)
                                {
                                    int delta = Math.Abs(sourceZone.ProgramTargets[i]);

                                    sourceZone.ProgramTargets[i] = sourceZone.ProgramTargets[i] + delta;
                                    activeZone.ProgramTargets[i] = activeZone.ProgramTargets[i] - delta;
                                }
                                if (activeZone.ProgramTargets[programToEjectIndex] < 0)
                                {
                                    int delta = Math.Abs(activeZone.ProgramTargets[programToEjectIndex]);

                                    activeZone.ProgramTargets[programToEjectIndex] = activeZone.ProgramTargets[programToEjectIndex] + delta;
                                    sourceZone.ProgramTargets[programToEjectIndex] = sourceZone.ProgramTargets[programToEjectIndex] - delta;
                                }

                                break;
                            }
                            if (reservedAreaRemaining < activeProgram.OccupationArea)
                            {
                                //activeZone has run out of available space, reset values for next zone.

                                ejectionIndexModifier = 0;
                                sourceZoneIndexModifier = 0;

                                break;
                            }
                            if (sourceZone.ProgramTargets[i] <= 0)
                            {
                                //sourceZone has run out of activeProgram to pull. Increment sourceZoneIndexModifier.

                                if (sourceZone.ProgramTargets[i] < 0)
                                {
                                    int delta = Math.Abs(sourceZone.ProgramTargets[i]);

                                    sourceZone.ProgramTargets[i] = sourceZone.ProgramTargets[i] + delta;
                                    activeZone.ProgramTargets[i] = activeZone.ProgramTargets[i] - delta;
                                }

                                currentLoopEjectedProgramDelta = 0;

                                ejectionIndexModifier = 0;
                                programToEjectIndex = activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)];

                                sourceZoneIndexModifier++;
                                sourceZone = zm.Zones[activeProgram.ZonePreference[numZones - (sourceZoneIndexModifier + 1)]];

                                //continue;
                            }
                            if (activeZone.ProgramTargets[programToEjectIndex] <= 0)
                            {
                                //activeZone has run out of currentProgram to eject. Increment ejectionIndexModifier.

                                if (activeZone.ProgramTargets[programToEjectIndex] < 0)
                                {
                                    int delta = Math.Abs(activeZone.ProgramTargets[programToEjectIndex]);

                                    activeZone.ProgramTargets[programToEjectIndex] = activeZone.ProgramTargets[programToEjectIndex] + delta;
                                    sourceZone.ProgramTargets[programToEjectIndex] = sourceZone.ProgramTargets[programToEjectIndex] - delta;
                                }

                                currentLoopEjectedProgramDelta = 0;

                                ejectionIndexModifier++;

                                programToEjectIndex = ejectionIndexModifier >= numPrograms ? numPrograms : activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)];

                                //continue;
                            }
                            if (programToEjectIndex >= numPrograms)
                            {
                                //No more program available to pull in zone. Reset board and increment sourceZone.

                                currentLoopEjectedProgramDelta = 0;
                                ejectedProgramSizeCache = 0;

                                ejectionIndexModifier = 0;
                                programToEjectIndex = activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)];

                                sourceZoneIndexModifier++;
                                sourceZone = zm.Zones[activeProgram.ZonePreference[numZones - (sourceZoneIndexModifier + 1)]];
                            }
                            if (sourceZoneIndexModifier >= numZones)
                            {
                                //No more zones to pull from. Program is as good as it's going to get.

                                remainingToMove = 0;
                                break;
                            }

                            //If none of those tests trigger, things are fine and the next loop in this zone will begin.

                        }

                        //Endchecks for when a trade is not completed. If any while-condition is about to fail, increment necessary modifier, prepare for next loop, or complete process.
                        if (currentLoopActiveProgramDelta == 0)
                        {
                            if (remainingToMove == 0)
                            {
                                //Commit any outstanding deltas, move to next program. (There is a second break outside of the while loop for this.)
                                activeZone.ReservedArea[j][i] = reservedAreaRemaining;

                                break;
                            }
                            if (reservedAreaRemaining < activeProgram.OccupationArea)
                            {
                                //activeZone has run out of available space, reset values for next zone.

                                programToEjectIndex = 0;
                                sourceZoneIndexModifier = 0;

                                break;
                            }
                            if (sourceZone.ProgramTargets[i] <= 0)
                            {
                                //sourceZone has run out of activeProgram to pull. Increment sourceZoneIndexModifier.

                                if (sourceZone.ProgramTargets[i] < 0)
                                {
                                    int delta = Math.Abs(sourceZone.ProgramTargets[i]);

                                    sourceZone.ProgramTargets[i] = sourceZone.ProgramTargets[i] + delta;
                                    activeZone.ProgramTargets[i] = activeZone.ProgramTargets[i] - delta;
                                }

                                currentLoopEjectedProgramDelta = 0;

                                ejectionIndexModifier = 0;
                                programToEjectIndex = activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)];

                                sourceZoneIndexModifier++;
                                sourceZone = zm.Zones[activeProgram.ZonePreference[numZones - (sourceZoneIndexModifier + 1)]];

                                //continue;
                            }
                            if (activeZone.ProgramTargets[programToEjectIndex] <= 0)
                            {
                                //activeZone has run out of currentProgram to eject. Increment ejectionIndexModifier.

                                if (activeZone.ProgramTargets[programToEjectIndex] < 0)
                                {
                                    int delta = Math.Abs(activeZone.ProgramTargets[programToEjectIndex]);

                                    activeZone.ProgramTargets[programToEjectIndex] = activeZone.ProgramTargets[programToEjectIndex] + delta;
                                    sourceZone.ProgramTargets[programToEjectIndex] = sourceZone.ProgramTargets[programToEjectIndex] - delta;
                                }

                                currentLoopEjectedProgramDelta = 0;

                                ejectionIndexModifier++;

                                programToEjectIndex = ejectionIndexModifier >= numPrograms ? numPrograms : activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)];

                                ejectedProgramSizeCache = programToEjectIndex >= numPrograms ? 0 : currentLoopEjectedProgramDelta * pm.ProgramPackages[programToEjectIndex].OccupationArea;

                                //ejectedProgramCountCache = 0;

                                //continue;
                            }
                            if (ejectionIndexModifier >= numPrograms)
                            {
                                //No more program available to pull in zone. Reset board and increment sourceZone.

                                currentLoopEjectedProgramDelta = 0;
                                ejectedProgramSizeCache = 0;

                                ejectionIndexModifier = 0;
                                programToEjectIndex = activeZone.ProgramPriority[numPrograms - (ejectionIndexModifier + 1)];

                                sourceZoneIndexModifier++;
                                sourceZone = zm.Zones[activeProgram.ZonePreference[numZones - (sourceZoneIndexModifier + 1)]];
                            }
                            if (sourceZoneIndexModifier >= numZones)
                            {
                                //No more zones to pull from. Program is as good as it's going to get.

                                remainingToMove = 0;
                                break;
                            }

                            //If none of these tests trigger, the next loop should be able to run. Cache any contributions and begin next loop in this zone.

                            //ejectedProgramCountCache = currentLoopEjectedProgramDelta;
                        }
                    }

                    if (remainingToMove == 0)
                    {
                        break;
                    }

                    //Program has been optimized in zone. Clear all holdings.
                    activeZone.ReservedArea[j][i] = reservedAreaRemaining;
                }

                #region forfeit procedure
                //Program has been optimized. Forfeit any remaining reserved area.

                //RhinoApp.WriteLine("---Procedure complete, forfeiting...");

                int zoneCount = 0;

                foreach (ZonePackage zone in zm.Zones)
                {
                    //RhinoApp.WriteLine("Zone number {0}: ", zoneCount.ToString());
                    zoneCount++;

                    int priorityCount = 0;

                    foreach (List<double> reservedAreas in zone.ReservedArea)
                    {
                        //RhinoApp.WriteLine("Priority level {0}: ", priorityCount.ToString());
                        priorityCount++;

                        int remainingSuitors = 0;
                        List<int> remainingSuitorIndices = new List<int>();

                        double availableToRedistribute = reservedAreas[i];

                        if (availableToRedistribute < 0)
                        {
                            reservedAreas[i] = 0;

                            //RhinoApp.WriteLine("None available.");

                            continue;
                        }

                        for (int c = 0; c < numPrograms; c++)
                        {
                            if (c != i && reservedAreas[c] != 0)
                            {
                                remainingSuitors++;
                                remainingSuitorIndices.Add(c);
                            }
                        }

                        if (remainingSuitors == 0)
                        {
                            reservedAreas[i] = 0;

                            //RhinoApp.WriteLine("No one to distribute to.");

                            continue;
                        }

                        double areaToDistribute = availableToRedistribute / remainingSuitors;

                        //RhinoApp.WriteLine("Distributing {0} to {1} others.", areaToDistribute.ToString(), remainingSuitors.ToString());

                        foreach (int index in remainingSuitorIndices)
                        {
                            reservedAreas[index] = reservedAreas[index] + areaToDistribute;
                        }

                        reservedAreas[i] = 0;
                    }
                }

                #endregion
            }
            #endregion

        }

        /// <summary>
        /// Breaks initial zones into generally rectangular pieces.
        /// Subdivides those pieces into rooms based on program sizes and quotas.
        /// </summary>
        /// <param name="zm"></param>
        /// <param name="pm"></param>
        public static void RoomConfiguration(ZoneManifest zm, ProgramManifest pm)
        {
            foreach (ZonePackage zone in zm.Zones)
            {
                zone.Rooms = new List<RoomPackage>();

                List<Brep> rectangularizedZoneRegions = Breps.Rectangularize(zone);

                //Slice rectangularized zone regions into room lanes.
                foreach (Brep region in rectangularizedZoneRegions)
                {
                    Update.Room.LaneConfiguration(region, zone, pm);
                }

                //Perform final measurements and prepare rooms for population.
                for (int i = 0; i < zone.Rooms.Count; i++)
                {
                    var room = zone.Rooms[i];

                    Update.Room.Orientation(room, zone);
                    Update.Room.ProgramHint(room, zone, i);
                }
            }
        }

        public static void RemainingProgramTargets(ZonePackage zone, RoomPackage room)
        {
            for (int i = 0; i < room.NumProgramsPlaced.Count; i++)
            {
                zone.RemainingProgramTargets[i] = zone.RemainingProgramTargets[i] - room.NumProgramsPlaced[i];
            }
        }

        /// <summary>
        /// Since the zone-specific targets are estimates, if a zone is unable to meet its quotas, it passes them along to the next.
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="nextZone"></param>
        /// <param name="pm"></param>
        public static void PassUnfulfilledTargets(ZonePackage zone, ZonePackage nextZone, ProgramManifest pm)
        {
            for (int i = 0; i < zone.RemainingProgramTargets.Count; i++)
            {
                var activeRemainder = zone.RemainingProgramTargets[i];

                if (activeRemainder > 0 && pm.ProgramPackages[i].Quota != 0)
                {
                    nextZone.ProgramTargets[i] = nextZone.ProgramTargets[i] + activeRemainder;
                }
            }
        }
    }
}
