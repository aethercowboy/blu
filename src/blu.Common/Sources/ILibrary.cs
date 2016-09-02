using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using blu.Common.Enums;
using HtmlAgilityPack;

namespace blu.Common.Sources
{
    public interface ILibrary
    {
        Task<IEnumerable<string>> Lookup(string title, string author, Format format);
    }

    public abstract class Library : ILibrary
    {
        protected abstract IList<Format> AllowedFormats { get; }

        public virtual async Task<IEnumerable<string>> Lookup(string title, string author, Format format)
        {
            var results = new List<string>();

            if (!AllowedFormats.Contains(format))
            {
                return results;
            }

            results.AddRange(await SourceLookup(title, author, format));

            return results;
        }

        protected abstract Task<IEnumerable<string>> SourceLookup(string title, string author, Format format);

        protected static bool HtmlNodeHasClass(HtmlNode x, string className)
        {
            if (x == null) return false;
            if (!x.HasAttributes) return false;

            var htmlClass = x?.Attributes["class"]?.Value;

            return htmlClass != null &&
                   htmlClass.Split(' ')
                       .Any(y => y.Equals(className));
        }
    }
}