using blu.Common.Enums;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace blu.Common.Sources
{
    public interface ILibrary
    {
        HttpClient HttpClient { get; set; }
        Task<IEnumerable<string>> Lookup(string title, string author, Format format);
    }

    public abstract class Library : ILibrary
    {
        public HttpClient HttpClient { get; set; }
        protected IBluConsole Console { get; }

        protected Library() : this(new BluConsole())
        {
        }

        private Library(IBluConsole console)
        {
            Console = console;
        }

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