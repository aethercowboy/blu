using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace blu.Extensions
{
    public static class StringExtensions
    {
        public static string UnCamelCase(this String str)
        {
            Regex pattern = new Regex(@"([a-z])([A-Z])");

            string retval = pattern.Replace(str, "$1 $2");

            return retval;
        }
    }
}
