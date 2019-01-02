using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Logic.Relationships;
using HOK.OfficeManager.Formats;
using Rhino;
using Rhino.Geometry;

namespace HOK.OfficeManager.Logic.Population
{
    public class Populate
    {
        /// <summary>
        /// Populates room in rows along its Y axis. Packs along the left (-X) edge until passing the room's midpoint, then packs along right edge until full.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="zone"></param>
        /// <param name="pm"></param>
        public static void ByMostRows(RoomPackage room, ZonePackage zone, ProgramManifest pm)
        {
            var lexFull = false;
            var rexFull = false;
            var openZones = !lexFull || !rexFull;

            var anchor = Point3d.Unset;

            //Parent population method.
            while (room.MaxPlacement.Select(x => x > 0).Any() && openZones)
            {
                //Iterate through programs as staged.
                // (Chuck) Important: room.MaxPlacement aligns to program indices in room.FillOrder.
                // These DO NOT align to pm.ProgramPackages, and often don't even contain all programs.
                // To get data for the active program from its ProgramPackage, or to use a list that aligns with it, reference with index at room.FillOrder[i].
                for (int i = 0; i < room.MaxPlacement.Count; i++)
                {
                    var activeProgram = pm.ProgramPackages[room.FillOrder[i]];
                    var activeProgramIndex = room.FillOrder[i];

                    //Begin packing program.
                    while (room.MaxPlacement[i] - room.NumProgramsPlaced[activeProgramIndex] > 0)
                    {
                        PlacementPackage candidate = null;

                        //Start in left half.
                        if (!lexFull)
                        {
                            candidate = Stage.Program.ForLeftLane(room, Stage.Program.InRoom(room, activeProgram));

                            //Move candidate to next position.
                            anchor = room.NextAnchor == Point3d.Unset ? room.BaseAnchorLeft : room.NextAnchor;
                            candidate.Bounds.Transform(Transform.Translation(anchor - room.BaseAnchorLeft));

                            //Verify that placement is valid and stage next anchor point accordingly.
                            if (Collision.PlacementIsValid(room, candidate, pm))
                            {
                                room.PlacedItems.Add(candidate);
                                room.NumProgramsPlaced[activeProgramIndex]++;

                                candidate.Dims = new CurveBounds(candidate.Bounds);

                                candidate.Orientation.Origin.Transform(Transform.Translation(anchor - room.BaseAnchorLeft));

                                var buffer = 0.0;

                                if (candidate.Program.AccessDirections == "1111")
                                {
                                    buffer = 3;
                                }

                                room.PrevAnchor = anchor;
                                room.NextAnchor = Stage.Room.NextAnchorRows(room, activeProgram, anchor, 1, buffer);
                            }
                            //Otherwise shift anchor slightly and retry.
                            else
                            {
                                room.PrevAnchor = anchor;
                                room.NextAnchor = Stage.Room.NextAnchorRows(room, activeProgram, anchor, 1, 0.1);
                            }

                            if (!Confirm.PointInRegion(room.Region, room.NextAnchor))
                            {
                                if (i == room.MaxPlacement.Count - 1)
                                {
                                    room.NextAnchor = Point3d.Unset;
                                    lexFull = true;
                                    break;
                                }

                                room.NextAnchor = Point3d.Unset;
                                break;
                            }

                            continue;
                        }

                        if (!rexFull)
                        {
                            candidate = Stage.Program.ForRightLane(room, Stage.Program.InRoom(room, activeProgram));

                            //Move candidate to next position.
                            anchor = room.NextAnchor == Point3d.Unset ? room.BaseAnchorRight : room.NextAnchor;
                            candidate.Bounds.Transform(Transform.Translation(anchor - room.BaseAnchorRight));

                            //Verify that placement is valid and stage next anchor point accordingly.
                            if (Collision.PlacementIsValid(room, candidate, pm))
                            {
                                room.PlacedItems.Add(candidate);
                                room.NumProgramsPlaced[activeProgramIndex]++;

                                candidate.Dims = new CurveBounds(candidate.Bounds);

                                candidate.Orientation.Origin.Transform(Transform.Translation(anchor - room.BaseAnchorRight));

                                var buffer = 0.0;

                                if (candidate.Program.AccessDirections == "1111")
                                {
                                    buffer = 3;
                                }

                                room.PrevAnchor = anchor;
                                room.NextAnchor = Stage.Room.NextAnchorRows(room, activeProgram, anchor, 1, buffer);
                            }
                            //Otherwise shift anchor slightly and retry.
                            else
                            {
                                room.PrevAnchor = anchor;
                                room.NextAnchor = Stage.Room.NextAnchorRows(room, activeProgram, anchor, 1, 0.1);
                            }

                            if (!Confirm.PointInRegion(room.Region, room.NextAnchor))
                            {
                                if (i == room.MaxPlacement.Count - 1)
                                {
                                    room.NextAnchor = Point3d.Unset;
                                    rexFull = true;
                                    break;
                                }

                                room.NextAnchor = Point3d.Unset;
                                break;
                            }

                            continue;
                        }

                        openZones = false;
                        break;
                    }

                    //Room has been populated.
                }
            }
        }

        /// <summary>
        /// Populate room by marching along perimeter. Generate interior islands as necessary.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="zone"></param>
        /// <param name="pm"></param>
        public static void ByPerimeter(RoomPackage room, ZonePackage zone, ProgramManifest pm)
        {
            //TODO: Write this routine. Must solve lane generation problems for curvilinear zones first.
        }

        /// <summary>
        /// Population routine for debugging. Places one instance of highest priority item in room.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="zone"></param>
        /// <param name="pm"></param>
        public static void PlaceOne(RoomPackage room, ZonePackage zone, ProgramManifest pm)
        {
            var firstWithTarget = pm.ProgramPackages[zone.ProgramTargets.IndexOf(zone.ProgramTargets.First(x => x > 0))];

            var candidate = Stage.Program.InRoom(room, firstWithTarget);

            if (Collision.PlacementIsValid(room, candidate, pm))
            {
                room.PlacedItems.Add(candidate);
                room.NumProgramsPlaced[firstWithTarget.Priority] = room.NumProgramsPlaced[firstWithTarget.Priority] + 1;
            }
        }
    }
}
