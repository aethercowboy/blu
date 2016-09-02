using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blu.Common.Extensions
{
    public static class StringExtensions
    {
        public static string UrlEscape(this string str)
        {
            return str.Replace(" ", "%20").Replace(":", "%3A").Replace("(", "%28").Replace(")", "%29");
        }
    }
}
