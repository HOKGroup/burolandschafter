using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.Buro.Factory;
using HOK.Buro.Formats;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using HOK.Buro.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace HOK.Buro.Tests.Formats
{
    [TestClass]
    public class ProgramPackageTests
    {
        [TestMethod]
        public void NullInput_IsValid()
        {
            int quota = 0;
            string access = "1000";
            bool priv = false;
            Curve boundary = CurvesFactory.RectangleCWH(Point3d.Origin, 1, 1);
            List<Curve> geometry = new List<Curve>();
            
            ProgramPackage TestProgramItem = new ProgramPackage(quota, access, priv, boundary, geometry);

            Assert.IsNotNull(TestProgramItem);
        }
    }
}