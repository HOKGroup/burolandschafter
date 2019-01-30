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
using HOK.Buro.Logic.Translations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace HOK.Buro.Tests.Translations.Advice
{
    [TestClass]
    public class ParseAdviceTests
    {
        [TestMethod]
        public void LockRoom_CallsCorrectMethod()
        {
            var testAdvice = new AdvicePackage(
                "Lock",
                CurvesFactory.UnitXCurve(),
                new List<int>(),
                new List<double>()
                );

            var result = HOK.Buro.Logic.Translations.Advice.ParseAdvice(testAdvice);

            Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

            Assert.AreEqual("Lock", data["Type"]);
        }

        [TestMethod]
        public void EncourageProgram_CallsCorrectMethod()
        {
            var testAdvice = new AdvicePackage(
                "Encourage",
                CurvesFactory.UnitXCurve(),
                new List<int>(),
                new List<double>()
            );

            var result = HOK.Buro.Logic.Translations.Advice.ParseAdvice(testAdvice);

            Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string,string>>(result);

            Assert.AreEqual("Encourage", data["Type"]);
        }

        [TestMethod]
        public void DiscourageProgram_CallsCorrectMethod()
        {
            var testAdvice = new AdvicePackage(
                "Discourage",
                CurvesFactory.UnitXCurve(),
                new List<int>(),
                new List<double>()
            );

            var result = HOK.Buro.Logic.Translations.Advice.ParseAdvice(testAdvice);

            Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

            Assert.AreEqual("Discourage", data["Type"]);
        }

        [TestMethod]
        public void ForbidProgram_CallsCorrectMethod()
        {
            var testAdvice = new AdvicePackage(
                "Forbid",
                CurvesFactory.UnitXCurve(),
                new List<int>(),
                new List<double>()
            );

            var result = HOK.Buro.Logic.Translations.Advice.ParseAdvice(testAdvice);

            Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

            Assert.AreEqual("Forbid", data["Type"]);
        }
    }
}
