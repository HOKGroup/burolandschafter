using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.OfficeManager.Logic.Translations
{
    public static class Data
    {
        public static string ListToString<T>(ref List<T> list, string delimiter)
        {
            if (list.Count == 0)
            {
                return "";
            }

            var output = "";
            var lastItem = list.Last();

            if (delimiter == null)
            {
                delimiter = " ";
            }

            list.ForEach(x => output = output + (x.Equals(lastItem) ? x.ToString() : x.ToString() + delimiter) );

            return output;
        }
    }
}
