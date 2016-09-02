using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using blu.Common.Enums;
using blu.Common.Sources;
using HtmlAgilityPack;

namespace blu.Sources.Hoopla.Sources
{
    internal class Hoopla : Library
    {
        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.DownloadableAudiobook,
            Format.EBook,
            Format.EComic,
            Format.EMusic
        };

        private string Url { get; } = "https://www.hoopladigital.com/search?results=&q=[QUERY]&kind=[KIND]";

        protected override async Task<IEnumerable<string>> SourceLookup(string title, string author, Format format)
        {
            string response;

            using (var wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.Add("User-Agent", UserAgent.GoogleChrome);

                var query = BuildQuery(title, author);
                var kind = GetKind(format);

                var lookupUrl = Url.Replace("[QUERY]", query)
                    .Replace("[KIND]", kind);

                response = await wc.GetStringAsync(lookupUrl);
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes =
                doc.DocumentNode.Descendants("div").Where(x => HtmlNodeHasClass(x, "row")).ToList();

            var results = new List<string>();

            if (childNodes == null)
            {
                return results;
            }

            foreach (var element in childNodes)
            {
                var valid = element.Descendants("div").Where(x => HtmlNodeHasClass(x, "columns")).ToList();

                if (valid == null || !valid.Any())
                {
                    return results;
                }

                var firstOrDefault = valid.FirstOrDefault();
                if (firstOrDefault != null) results.Add(firstOrDefault.InnerText);
            }

            return results;
        }

        private static string BuildQuery(string title, string author)
        {
            var retval = string.Join("+", title.Replace(" ", "+"), author);

            return retval;
        }

        private static string GetKind(Format format)
        {
            switch (format)
            {
                case Format.DownloadableAudiobook:
                    return "AUDIOBOOK";
                case Format.EBook:
                    return "EBOOK";
                case Format.EComic:
                    return "COMIC";
                case Format.EMusic:
                    return "MUSIC";
                default:
                    return string.Empty;
            }
        }
    }
}