using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Logic;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects.Tables;

namespace HOK.OfficeManager.Formats
{
    /// <summary>
    /// Top-level collection of user input data.
    /// </summary>
    public class TestFitPackage
    {
        public FloorPlanPackage FloorPlanPackage;
        public List<ProgramPackage> ProgramPackages;
        public List<AdvicePackage> AllAdvice;

        public TestFitPackage(FloorPlanPackage planInfo, List<ProgramPackage> programInfo)
        {
            FloorPlanPackage = planInfo;
            ProgramPackages = programInfo;
        }

        public TestFitPackage(FloorPlanPackage planInfo, List<ProgramPackage> programInfo, List<AdvicePackage> allAdvice)
        {
            FloorPlanPackage = planInfo;
            ProgramPackages = programInfo;
            AllAdvice = allAdvice;
        }
    }

    #region pm

    /// <summary>
    /// Collection of ProgramPackages with general metadata.
    /// </summary>
    [Serializable]
    public class ProgramManifest
    {
        public List<ProgramPackage> ProgramPackages;
        public bool IsPossible;
        public int MaximizedCount;

        public List<AdvicePackage> AllAdvice;

        public ProgramManifest(List<ProgramPackage> programs, bool isPossible, int maxCount)
        {
            ProgramPackages = programs;
            IsPossible = isPossible;
            MaximizedCount = maxCount;
        }

        public ProgramManifest Duplicate()
        {
            var pmBase = new ProgramManifest(new List<ProgramPackage>(ProgramPackages), IsPossible, MaximizedCount);

            return pmBase;
        }
    }

    /// <summary>
    /// Class containing user input program information and statistics recorded by the program.
    /// </summary>
    [Serializable]
    public class ProgramPackage
    {
        //User input values.
        public int Quota;
        public string AccessDirections;
        public bool IsPrivate;
        public Curve OccupationBoundary;
        public double OccupationArea;
        public List<Curve> DrawingGeometry;

        //Values calculated for program manifest.
        public int Target;
        public int Remaining;
        public double Distribution;
        public int Priority;

        // (Chuck) Ordinal list of most preferred, and most dispreferred, types of space for a program. Corresponds to ZonePackage "AffinityType" value.
        //General infrastructure for more parametric implementations. (Like departments, sound, etc.) Used to generate ZonePreference.
        //Current implementation hard-codes tests for the following:
        //1 - core adjacency
        //2 - perimeter adjacency
        //3 - island

        public List<int> Affinity;
        public List<int> Enmity;
        public List<int> ZonePreference;

        public CurveBounds Dims;
        public Plane BasePlane;

        /// <summary>
        /// Constructor for user input programs.
        /// </summary>
        /// <param name="quota">User input quota. A value of 0 means program will be maximized.</param>
        /// <param name="access">Direction program is accessed from. (NESW)</param>
        /// <param name="isPrivate">Is the program enclosed?</param>
        /// <param name="extents">Rectangular bounds of program occupation. Does not include circulation for access.</param>
        /// <param name="geo">2D curve information used for drawing.</param>
        public ProgramPackage(int quota, string access, bool isPrivate, Curve extents, List<Curve> geo)
        {
            var rectInfo = new CurveBounds(extents);

            Quota = quota;
            AccessDirections = access;
            IsPrivate = isPrivate;
            OccupationBoundary = extents;
            OccupationArea = rectInfo.Area;
            DrawingGeometry = geo;

            Dims = rectInfo;
            BasePlane = new Plane(new Point3d(rectInfo.XMin, rectInfo.YMax, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
        }
    }
    #endregion

    #region zm

    /// <summary>
    /// Collection of ZonePackages with general metadata.
    /// </summary>
    [Serializable]
    public class ZoneManifest
    {
        public FloorPlanPackage FloorPlan;
        public List<ZonePackage> Zones;
        public double TotalArea;
        public List<int> PlacementTotals;

        public ZoneManifest(List<ZonePackage> zonePackages, double totalArea)
        {
            Zones = zonePackages;
            TotalArea = totalArea;
        }

        public ZoneManifest Duplicate()
        {
            ZoneManifest zmBase = new ZoneManifest(new List<ZonePackage>(Zones), TotalArea);

            if (FloorPlan != null)
            {
                zmBase.FloorPlan = FloorPlan;
            }

            if (PlacementTotals != null)
            {
                zmBase.PlacementTotals = new List<int>(PlacementTotals);
            }

            return zmBase;
        }
    }

    /// <summary>
    /// Class containing data and geometry related to an individual zone.
    /// </summary>
    [Serializable]
    public class ZonePackage
    {
        //Geometric information.
        public Brep Region;
        public List<RoomPackage> Rooms; //Pseudo-rooms.
        public EdgeCurves EdgeCurves;
        public double BaseArea;

        //Zone adjacencies & tests.
        public int AffinityType;
        public bool IsCoreAdjacent;
        public bool IsPerimeterAdjacent;
        public bool IsIsland;

        public Curve PrimaryCirculationEdge;
        public List<LanePackage> LanePackages;

        //Statistics.
        public List<int> ProgramTargets;
        public List<int> RemainingProgramTargets;
        public List<double> ProportionalTargets;

        public List<int> Popularity;
        public List<int> ProgramPriority;

        public List<List<double>> ReservedArea; //[affinity priority level][area for program]

        public ZonePackage(Brep region)
        {
            Region = region;
            BaseArea = region.GetArea();
        }

        public ZonePackage(Brep region, EdgeCurves edgeCurves, bool coreAdj, bool perimeterAdj, bool island)
        {
            Region = region;
            EdgeCurves = edgeCurves;

            BaseArea = region.GetArea();

            IsCoreAdjacent = coreAdj;
            IsPerimeterAdjacent = perimeterAdj;
            IsIsland = island;
        }

        public override string ToString()
        {
            var output = $"{Math.Round(BaseArea)} sqft type {AffinityType.ToString()} zone. | ";

            for (var i = 0; i < ProportionalTargets.Count; i++)
            {
                var addendum = ProgramTargets[i].ToString();

                if (i != ProportionalTargets.Count - 1)
                {
                    addendum = addendum + " / ";
                }

                output = output + addendum;
            }

            return output;
        }
    }

    /// <summary>
    /// Package for "rooms" generated from zone subdivision. Includes base geometry and networking information.
    /// This is the object directly referenced in population methods.
    /// </summary>
    [Serializable]
    public class RoomPackage
    {
        //Geometry.
        public Brep Region;
        public Curve RegionPerimeter;
        public List<Curve> AllEdgeCurves;
        public Plane OrientationPlane;
        public bool IsVertical;
        public CurveBounds Dims;

        //Circulation network information.
        public RoomPackage NextRoom;
        public List<Point2d> AccessPoints;

        //Population information.
        public List<int> ProgramHint;

        public Point3d PrevAnchor = Point3d.Unset;
        public Point3d NextAnchor = Point3d.Unset;
        public Point3d BaseAnchorLeft;
        public Point3d BaseAnchorCenter;
        public Point3d BaseAnchorRight;

        public double Lex;
        public double Rex;
        public double Median;

        public List<int> FillOrder;
        public List<int> MaxPlacement;
        public List<int> NumProgramsPlaced;

        public List<PlacementPackage> PlacedItems;
        public List<List<PlacementPackage>> Solution;


        /// <summary>
        /// Basic constructor for instances where other parameters are added using Update.Room methods.
        /// </summary>
        /// <param name="region">Planar 2D Brep representation of floor area for room.</param>
        /// <param name="numPrograms">Number of programs in manifest.</param>
        public RoomPackage(Brep region, int numPrograms)
        {
            Region = region;
            RegionPerimeter = Utils.GetRegionPerimeter(region);
            AllEdgeCurves = new List<Curve>(Curve.JoinCurves(region.Curves3D));

            NumProgramsPlaced = new List<int>();

            for (int i = 0; i < numPrograms; i++)
            {
                NumProgramsPlaced.Add(0);
            }

            Dims = new CurveBounds(Utils.GetBoundingBoxCurve(region));
        }
    }

    /// <summary>
    /// Dimension information used in room slicing procedure.
    /// </summary>
    [Serializable]
    public class LanePackage
    {
        //Vertical or Horizontal position of slicing line. Direction determined before package creation.
        public double SlicePosition;

        //Dimensions.
        public double Width;
        public double Depth;

        //Program statistics.
        public List<int> Programs;
        public List<int> ProjectedFill;

        /// <summary>
        /// Basic constructor for lane package.
        /// </summary>
        /// <param name="sliceVal">Vertical or horizontal position of slicing line.</param>
        /// <param name="programs">Indices of program candidates for lane.</param>
        /// <param name="width">Approximate width of lane.</param>
        public LanePackage(double sliceVal, List<int> programs, double width)
        {
            SlicePosition = sliceVal;
            Programs = programs;
            Width = width;
        }
    }

    /// <summary>
    /// Instance of a program item placed in a room.
    /// </summary>
    [Serializable]
    public class PlacementPackage
    {
        //Geometric information.
        public Plane Orientation;
        public Curve Bounds;
        public List<Curve> DrawingGeometry;

        //Statistics.
        public int ProgramIndex;
        public CurveBounds Dims;

        //Program information.
        public ProgramPackage Program;

        public PlacementPackage(ProgramPackage program, Plane orientation, Curve bounds)
        {
            Program = program;
            Orientation = orientation;
            Bounds = bounds;

            DrawingGeometry = new List<Curve>();
        }
    }

    [Serializable]
    public class EdgeCurves
    {
        //Edge classifications.
        public List<Curve> CoreAdjacent;
        public List<Curve> PerimeterAdjacent;
        public List<Curve> CirculationAdjacent;
        public List<Curve> ExemptionAdjacent;
        public List<Curve> ZoneAdjacent;
        public List<Curve> StructureAdjacent;

        public EdgeCurves(List<Curve> core, List<Curve> perimeter, List<Curve> circulation, List<Curve> exemption, List<Curve> zone, List<Curve> structure)
        {
            CoreAdjacent = core;
            PerimeterAdjacent = perimeter;
            CirculationAdjacent = circulation;
            ExemptionAdjacent = exemption;
            ZoneAdjacent = zone;
            StructureAdjacent = structure;
        }

        public override string ToString()
        {
            var core = CoreAdjacent?.Count ?? 0;
            var perimeter = PerimeterAdjacent?.Count ?? 0;
            var circ = CirculationAdjacent?.Count ?? 0;
            var exempt = ExemptionAdjacent?.Count ?? 0;
            var zone = ZoneAdjacent?.Count ?? 0;
            var str = StructureAdjacent?.Count ?? 0;

            var output = $"Core: {core} / Perimeter: {perimeter} / Circ: {circ} / Exempt: {exempt} / Zone: {zone} / Str: {str}";

            return output;
        }
    }
    #endregion

    #region advice

    [Serializable]
    public class AdvicePackage
    {
        //User input parameters.
        public string Type;
        public Curve Bounds;
        public List<int> ProgramIndices;
        public List<double> ProgramAdjustments;

        //Stored data.
        public List<PlacementPackage> CapturedProgram;

        public AdvicePackage(string type, Curve bounds, List<int> indices, List<double> adjustments)
        {
            Type = type;
            Bounds = bounds;
            ProgramIndices = indices;
            ProgramAdjustments = adjustments;
        }
    }

    #endregion

    #region primitive

    [Serializable]
    public class FloorPlanPackage
    {
        //Primitive input data on floor plan.

        //User input values.
        public Curve FloorProfile;
        public Curve CoreProfile;
        public List<Curve> ExemptionProfiles;
        public List<Curve> StructureProfiles;

        public List<Curve> CirculationAxisCurves;
        public List<Curve> CoreAccessCurves;

        public double BaseArea;

        public FloorPlanPackage(Curve mainProfile, Curve coreProfile, List<Curve> exemptProfiles, List<Curve> obstacleProfiles, List<Curve> circulationAxis, List<Curve> coreAccess)
        {
            FloorProfile = mainProfile;
            CoreProfile = coreProfile;
            ExemptionProfiles = exemptProfiles;
            StructureProfiles = obstacleProfiles;
            CirculationAxisCurves = circulationAxis;
            CoreAccessCurves = coreAccess;

            double exemptArea = 0;

            if (ExemptionProfiles != null)
            {
                foreach (var area in ExemptionProfiles)
                {
                    exemptArea = exemptArea + Brep.CreatePlanarBreps(area)[0].GetArea();
                }
            }

            BaseArea = Brep.CreatePlanarBreps(FloorProfile)[0].GetArea() - (Brep.CreatePlanarBreps(CoreProfile)[0].GetArea() + exemptArea);
        }
    }

    [Serializable]
    public class CirculationPackage
    {
        public List<Curve> MainCurves;
        public List<Curve> OptionCurves;

        public CirculationPackage(List<Curve> main, List<Curve> opt)
        {
            MainCurves = main;
            OptionCurves = opt;
        }
    }
    #endregion

    #region geometry
    /// <summary>
    /// Class for easily accessible measurements of the bounding rectangle for a 2D curve.
    /// Most useful for rectangles, but not restricted to them.
    /// </summary>
    [Serializable]
    public class CurveBounds
    {
        public double Width;
        public double Height;
        public double Area;
        public double RangeX;
        public double RangeY;
        public double XMin;
        public double XMax;
        public double YMin;
        public double YMax;

        public Curve BaseCurve;
        public Vector3d Diagonal;
        public Point3d Center;

        public CurveBounds(Curve rect)
        {
            BaseCurve = rect;

            List<double> cornerPointsX = new List<double>();
            List<double> cornerPointsY = new List<double>();
            int spanCount = rect.SpanCount;

            for (int i = 0; i < spanCount; i++)
            {
                Point3d cornerPoint = rect.PointAt(rect.SpanDomain(i).Min);
                cornerPointsX.Add(cornerPoint.X);
                cornerPointsY.Add(cornerPoint.Y);
            }

            double width = cornerPointsX.Max() - cornerPointsX.Min();
            double height = cornerPointsY.Max() - cornerPointsY.Min();

            double xMax = cornerPointsX.Max();
            double xMin = cornerPointsX.Min();
            var yMax = cornerPointsY.Max();
            double yMin = cornerPointsY.Min();

            Width = width;
            Height = height;
            Area = width * height;

            RangeX = Math.Abs(xMax - xMin);
            RangeY = Math.Abs(yMax - yMin);

            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;

            Diagonal = new Vector3d(new Point3d(xMax, yMax, 0) - new Point3d(xMin, yMin, 0));
            Center = new Point3d((XMax + XMin) / 2, (YMax + YMin) / 2, 0);
        }
    }
    #endregion


}
