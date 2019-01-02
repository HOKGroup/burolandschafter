using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HOK.OfficeManager.Tests.Translations.Data
{
    [TestClass]
    public class ListToStringTests
    {
        [TestMethod]
        public void IntegerSeries_NoDelimiter_StringIsCorrect()
        {
            var testList = new List<int>(new[] {1, 2, 3});

            var result = HOK.OfficeManager.Logic.Translations.Data.ListToString(ref testList, null);

            Assert.AreEqual("1 2 3", result);
        }
    }
}
