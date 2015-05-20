using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Blu.Extensions
{
    public static class StringExtensions
    {
        public static string UnCamelCase(this String str)
        {
            var pattern = new Regex(@"([a-z])([A-Z])");

            string retval = pattern.Replace(str, "$1 $2");

            return retval;
        }
    }
}