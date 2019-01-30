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
    public class Program
    {
        public static void Distribution(TestFitPackage tf)
        {
            List<ProgramPackage> programs = tf.ProgramPackages;

            int maximizedCount = 0;
            double nonMaximizedUse = 0;

            foreach (ProgramPackage program in programs)
            {
                if (program.Quota == 0)
                {
                    maximizedCount++;
                }
                else
                {
                    double quotaArea = program.OccupationArea * program.Quota;
                    double usage = quotaArea / tf.FloorPlanPackage.BaseArea;

                    nonMaximizedUse = nonMaximizedUse + usage;
                }
            }

            double maximizedUse = (maximizedCount != 0) ? (1 - nonMaximizedUse) / maximizedCount : 0;

            List<double> distributions = new List<double>();

            for (int i = 0; i < tf.ProgramPackages.Count; i++)
            {
                if (programs[i].Quota == 0)
                {
                    distributions.Add(maximizedUse * 100);
                }
                else
                {
                    double quotaArea = Brep.CreatePlanarBreps(programs[i].OccupationBoundary)[0].GetArea() * programs[i].Quota;
                    double usage = quotaArea / tf.FloorPlanPackage.BaseArea;

                    distributions.Add(usage * 100);
                }
            }

            for (int i = 0; i < programs.Count; i++)
            {
                programs[i].Distribution = distributions[i];
            }
        }

        public static void Target(TestFitPackage tf)
        {
            foreach (ProgramPackage program in tf.ProgramPackages)
            {
                if (program.Quota != 0)
                {
                    program.Target = program.Quota;
                }
                else
                {
                    double availableArea = (program.Distribution / 100) * tf.FloorPlanPackage.BaseArea;
                    double target = availableArea / program.OccupationArea;

                    program.Target = Convert.ToInt32(Math.Round(target));
                }

                program.Remaining = program.Target;
            }
        }

        #region preference functions
        public static void Affinity(TestFitPackage tf)
        {
            foreach (ProgramPackage program in tf.ProgramPackages)
            {
                int accessibility = 0;

                foreach (Char dir in program.AccessDirections)
                {
                    if (dir == '1')
                    {
                        accessibility++;
                    }
                }

                program.Affinity = new List<int>();

                if (program.IsPrivate && accessibility <= 1)
                {
                    program.Affinity.Add(1);
                }
                else if (!program.IsPrivate && accessibility == 1)
                {
                    program.Affinity.Add(2);
                    program.Affinity.Add(3);
                }
                else if (accessibility == 4)
                {
                    program.Affinity.Add(3);
                    program.Affinity.Add(2);
                }
            }
        }

        public static void Enmity(TestFitPackage tf)
        {
            foreach (ProgramPackage program in tf.ProgramPackages)
            {
                int accessibility = 0;

                foreach (Char dir in program.AccessDirections)
                {
                    if (dir == '1')
                    {
                        accessibility++;
                    }
                }

                program.Enmity = new List<int>();

                if (program.IsPrivate && accessibility <= 1)
                {
                    program.Enmity.Add(3);
                }
                else if (accessibility == 4)
                {
                    program.Enmity.Add(1);
                }
            }
        }
        #endregion

        public static void Priority(TestFitPackage tf)
        {
            for (int i = 0; i < tf.ProgramPackages.Count; i++)
            {
                tf.ProgramPackages[i].Priority = i;
            }
        }

        public static void ZonePreference(ProgramManifest pm, ZoneManifest zm)
        {
            foreach (ProgramPackage program in pm.ProgramPackages)
            {
                int zoneCount = zm.Zones.Count;

                List<int> zonePreference = new List<int>(zoneCount);

                //Add most preferred zones first.
                for (int i = 0; i < zoneCount; i++)
                {
                    if (program.Affinity.Count == 0)
                    {
                        break;
                    }

                    foreach (int preferred in program.Affinity)
                    {
                        if (zm.Zones[i].AffinityType == preferred)
                        {
                            zonePreference.Add(i);
                        }
                    }
                }

                //Then add neutral zones.
                for (int i = 0; i < zoneCount; i++)
                {
                    if (zonePreference.Contains(i) == false)
                    {
                        if (program.Enmity.Count == 0)
                        {
                            zonePreference.Add(i);
                        }
                        else
                        {
                            foreach (int dispreferred in program.Enmity)
                            {
                                if (zm.Zones[i].AffinityType != dispreferred)
                                {
                                    zonePreference.Add(i);
                                }
                            }
                        }
                    }
                }

                //Last, add dispreferred zones, making sure least preferred is last.
                for (int i = 0; i < zoneCount; i++)
                {
                    for (int j = program.Enmity.Count - 1; j >= 0; j--)
                    {
                        if (program.Enmity[j] == zm.Zones[i].AffinityType)
                        {
                            zonePreference.Add(i);
                        }
                    }
                }

                program.ZonePreference = zonePreference;
            }
        }
    }
}
