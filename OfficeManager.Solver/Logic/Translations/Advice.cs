using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Formats;
using HOK.OfficeManager.Logic;
using Rhino.Geometry;
using Newtonsoft.Json;

namespace HOK.OfficeManager.Logic.Translations
{
    public static class Advice
    {
        public static string ParseAdvice(AdvicePackage advice)
        {
            var adviceDictionary = new Dictionary<string, Func<Curve, List<int>, List<double>, string>>
            {
                { "Lock", LockRoom },
                { "Encourage", EncourageProgram },
                { "Discourage", DiscourageProgram },
                { "Forbid", ForbidProgram }
            };

            return adviceDictionary[advice.Type](advice.Bounds, advice.ProgramIndices, advice.ProgramAdjustments);
        }

        #region rooms

        public static string LockRoom(Curve bounds, List<int> programs, List<double> adjustments)
        {
            //Store initial values.
            var lockAdviceData = new Dictionary<string, string>
            {
                { "Type", "Lock" },
                { "Bounds", Curves.ToSvg(bounds, null, false) }
            };

            //Interpret data and append additional information.

            //Serialize and return values.
            string jsonData = JsonConvert.SerializeObject(lockAdviceData, Formatting.Indented);

            return jsonData;
        }

        #endregion

        #region programs

        public static string EncourageProgram(Curve bounds, List<int> programs, List<double> adjustments)
        {
            //Store initial values.
            var encourageAdviceData = new Dictionary<string, string>
            {
                { "Type", "Encourage" },
                { "Bounds", Curves.ToSvg(bounds, null, false) },
                { "Programs", Data.ListToString(ref programs, null) },
                { "Adjustments", Data.ListToString(ref adjustments, null) }
            };

            //Interpret data and append additional information.

            //Serialize and return values.
            string jsonData = JsonConvert.SerializeObject(encourageAdviceData, Formatting.Indented);

            return jsonData;
        }

        public static string DiscourageProgram(Curve bounds, List<int> programs, List<double> adjustments)
        {
            //Store initial values.
            var discourageAdviceData = new Dictionary<string, string>
            {
                { "Type", "Discourage" },
                { "Bounds", Curves.ToSvg(bounds, null, false) },
                { "Programs", Data.ListToString(ref programs, null) },
                { "Adjustments", Data.ListToString(ref adjustments, null) }
            };

            //Interpret data and append additional information.

            //Serialize and return values.
            string jsonData = JsonConvert.SerializeObject(discourageAdviceData, Formatting.Indented);

            return jsonData;
        }

        public static string ForbidProgram(Curve bounds, List<int> programs, List<double> adjustments)
        {
            //Store initial values.
            var forbidAdviceData = new Dictionary<string, string>
            {
                { "Type", "Forbid" },
                { "Bounds", Curves.ToSvg(bounds, null, false) },
                { "Programs", Data.ListToString(ref programs, null) },
            };

            //Interpret data and append additional information.

            //Serialize and return values.
            string jsonData = JsonConvert.SerializeObject(forbidAdviceData, Formatting.Indented);

            return jsonData;
        }

        #endregion
    }
}
