using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Logic.Transformations;
using HOK.Buro.Formats;
using Rhino.Geometry;

namespace HOK.Buro.Logic.Relationships
{
    public static class Identify
    {
        /// <summary>
        /// Identify class methods parse data from TestFitPackage to prepare for calculations.
        /// Geometry is sent to Select class methods for processing. Each select method is solving a limited relationship problem.
        /// While Select methods may be over-specific and overlap with each other sometimes, they're just different groupings of all other general methods.
        /// Structure allows for quick readability and for Select methods to be directly tested without having to set up a whole test fit.
        /// </summary>
        /// <param name="tf"></param>
        /// <returns></returns>
        public static List<Brep> FloorPlateRegions(TestFitPackage tf)
        {
            //Parse floor and core profiles from TestFitPackage.
            Curve baseCurve = tf.FloorPlanPackage.FloorProfile;
            Curve coreCurve = tf.FloorPlanPackage.CoreProfile;

            Brep baseSurface = Brep.CreatePlanarBreps(baseCurve)[0];
            Brep coreSurface = Brep.CreatePlanarBreps(coreCurve)[0];

            List<Brep> AllRegions = Breps.SplitTwoBreps(baseSurface, coreSurface);

            //Determine base floor plate region.
            List<Brep> ValidFloorRegions = Select.FloorFromCore(AllRegions, coreSurface);

            return ValidFloorRegions;
        }

        public static List<Brep> AvailableFloorSpace(List<Brep> floorRegions, TestFitPackage tf, CirculationPackage circ)
        {
            //Parse exemption profiles.
            List<Curve> exemptionCurves = tf.FloorPlanPackage.ExemptionProfiles;
            List<Brep> exemptionRegions = new List<Brep>();

            foreach (Curve exemption in exemptionCurves)
            {
                Brep exemptionSurface = Brep.CreatePlanarBreps(exemption)[0];
                exemptionRegions.Add(exemptionSurface);
            }

            //Remove spaces designated by exemptions.
            List<Brep> nonExemptRegions = Select.NotExemptRegions(floorRegions, exemptionRegions);


            //Parse regions of core that need egress and circulation axis.
            List<Curve> coreAccessCurves = tf.FloorPlanPackage.CoreAccessCurves;
            List<Curve> circulationCurves = tf.FloorPlanPackage.CirculationAxisCurves;

            //Remove space for core access and egress requirements. Only if circulation axis do not already connect all egress requirements.
            List<Curve> coreCurvesToGiveEgress = Select.CoreCurvesWithoutEgress(coreAccessCurves, circulationCurves);

            if (coreCurvesToGiveEgress.Count != 0)
            {
                List<Curve> bestSourceOfEgress = Select.BestSourceOfEgress(coreCurvesToGiveEgress, circulationCurves);
                List<Brep> newExemptionsForEgress = Select.OrthogonalEgress(coreCurvesToGiveEgress, bestSourceOfEgress);

                //nonExemptRegions = newExemptionsForEgress;
                nonExemptRegions = Select.NotExemptRegions(nonExemptRegions, newExemptionsForEgress);
            }

            //Remove circulation zones of main circulation items.
            List<Curve> mainCirculationSegments = circ.MainCurves;
            List<Brep> circulationExemptions = new List<Brep>();

            foreach (Curve segment in mainCirculationSegments)
            {
                int minDist = 4;
                Curve extendedSegment = segment.Extend(CurveEnd.Both, minDist / 2, CurveExtensionStyle.Line);
                Brep circRegion = Brep.CreatePlanarBreps(Curves.OffsetClosed(extendedSegment, minDist / 2, true))[0];
                circulationExemptions.Add(circRegion);
            }

            //nonExemptRegions = Select.NotCirculationRegions(nonExemptRegions, circulationExemptions);
            nonExemptRegions = Select.NotExemptRegions(nonExemptRegions, circulationExemptions);

            //Remove columns. Still unsure if this is the best move.
            //Disabled for now, for performance reasons. Maybe check collision on population?
            List<Curve> structureObstacles = tf.FloorPlanPackage.StructureProfiles;
            List<Brep> structureExemptions = new List<Brep>();

            foreach (Curve str in structureObstacles)
            {
                structureExemptions.Add(Brep.CreatePlanarBreps(str)[0]);
            }

            //nonExemptRegions = Select.NotExemptRegions(nonExemptRegions, structureExemptions);

            return nonExemptRegions;
        }

        public static CirculationPackage CirculationTypes(TestFitPackage tf)
        {
            //Trim circulation axis to floor plate region, shatter, and characterize.
            List<Curve> circulationAxis = tf.FloorPlanPackage.CirculationAxisCurves;
            Curve floorProfile = tf.FloorPlanPackage.FloorProfile;

            List<Curve> trimmedCirculationAxis = new List<Curve>();

            foreach (Curve axis in circulationAxis)
            {
                Curve trimmedAxis = Curves.TrimWithClosedCurve(floorProfile, axis);
                trimmedCirculationAxis.Add(trimmedAxis);
            }

            //Shatter trimmed axis.
            List<Curve> circulationSegments = Curves.ShatterToSegments(trimmedCirculationAxis);

            //Characterize segments and format as CirculationPackage.
            CirculationPackage classifiedAxisSegments = Select.CirculationConfig(tf, circulationSegments);

            //Unify main curves before offset.
            List<Curve> simplifiedCurves = Curves.JoinColinear(classifiedAxisSegments.MainCurves);
            classifiedAxisSegments.MainCurves = simplifiedCurves;

            return classifiedAxisSegments;
        }

        public static ProgramManifest ProgramManifest(TestFitPackage tf)
        {
            //Parse base statistics from floor areas.
            var approximateTotalArea = tf.FloorPlanPackage.BaseArea;

            //Perform measurements and confirmations.
            var isPossible = Confirm.Program.RequestIsPossible(tf.ProgramPackages, approximateTotalArea);
            var maximizedCount = Confirm.Program.MaximizedCount(tf.ProgramPackages);

            Update.Program.Distribution(tf);
            Update.Program.Target(tf);
            Update.Program.Affinity(tf);
            Update.Program.Enmity(tf);
            Update.Program.Priority(tf);

            ProgramManifest pm = new ProgramManifest(tf.ProgramPackages, isPossible, maximizedCount);

            pm.AllAdvice = tf.AllAdvice;

            return pm;
        }

        public static List<Brep> OptimalFloorSpaceConfiguration(List<Brep> validFloorRegions, TestFitPackage tf)
        {
            Curve perimeterCurve = tf.FloorPlanPackage.FloorProfile;
            Curve coreCurve = tf.FloorPlanPackage.CoreProfile;

            List<Brep> zonesToSlice = new List<Brep>();
            List<Brep> optimizedZones = new List<Brep>();

            //Identify zones with irregular proximity or shape.
            foreach (Brep region in validFloorRegions)
            {
                bool intersectsPerimeter = Confirm.CurveRegionIntersection(perimeterCurve, region);
                bool intersectsCore = Confirm.CurveRegionIntersection(coreCurve, region);

                if (intersectsPerimeter && intersectsCore)
                {
                    zonesToSlice.Add(region);
                }
                else
                {
                    optimizedZones.Add(region);
                }
            }

            //Cut them into more manageable pieces.
            List<Curve> splitterCurves = Select.BestSplitterCurves(zonesToSlice, tf.FloorPlanPackage.CirculationAxisCurves, tf.FloorPlanPackage.CoreProfile);

            for (int i = 0; i < zonesToSlice.Count; i++)
            {
                List<Brep> splitBreps = Breps.SplitByCurve(zonesToSlice[i], splitterCurves[i]);

                foreach (Brep zone in splitBreps)
                {
                    optimizedZones.Add(zone);
                }
            }

            //Identify donut zones (zones with a hole in the middle).
            for (int i = optimizedZones.Count - 1; i >= 0; i--)
            {
                bool isNotDonut = Confirm.RegionIsNotDonut(optimizedZones[i]);

                //Cut up the donuts.
                if (!isNotDonut)
                {
                    List<Curve> donutSplitCurves = Select.DonutSplittingCurves(optimizedZones[i]);

                    foreach (Curve crv in donutSplitCurves)
                    {
                        //RhinoDoc.ActiveDoc.Objects.AddCurve(crv);
                    }

                    List<Brep> unDonutZones = Breps.SplitByCurves(optimizedZones[i], donutSplitCurves);

                    optimizedZones.RemoveAt(i);

                    foreach (Brep newZone in unDonutZones)
                    {
                        optimizedZones.Add(newZone);
                    }
                }
            }

            return optimizedZones;
        }

        public static ZoneManifest ZoneManifest(List<Brep> zonesToParse, ProgramManifest pm, TestFitPackage tf)
        {
            List<ZonePackage> zpList = new List<ZonePackage>();
            double totalArea = 0;

            foreach (Brep zone in zonesToParse)
            {
                ZonePackage zp = new ZonePackage(zone);

                if (zp.BaseArea < 10)
                {
                    //Stopgap solution for micro-zones generated by weird circulation corners.

                    continue;
                }

                totalArea = totalArea + zp.BaseArea;

                EdgeCurves edgeCurvePackage = Select.ZoneEdgeCurves(zone, tf, zonesToParse);

                Update.Zone.EdgeClassification(zp, edgeCurvePackage);
                Update.Zone.Adjacencies(zp);
                Update.Zone.AffinityType(zp);

                zpList.Add(zp);
            }

            ZoneManifest zm = new ZoneManifest(zpList, totalArea)
            {
                FloorPlan = tf.FloorPlanPackage
            };

            Update.Program.ZonePreference(pm, zm);

            Update.Zone.Popularity(zm, pm);
            Update.Zone.ProgramPriority(zm, pm);
            Update.Zone.ReservedArea(zm, pm);

            Update.Zone.ProgramTargets(zm, pm);
            Update.Zone.RoomConfiguration(zm, pm);

            return zm;
        }
    }
}
