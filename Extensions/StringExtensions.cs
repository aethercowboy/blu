using System.Text.RegularExpressions;

namespace blu.Extensions
{
    public static class StringExtensions
    {
        public static string UnCamelCase(this string str)
        {
            if (str == null)
            {
                return null;
            }

            var pattern = new Regex(@"([a-z])([A-Z])");

            var retval = pattern.Replace(str, "$1 $2");

            return retval;
        }
    }
}