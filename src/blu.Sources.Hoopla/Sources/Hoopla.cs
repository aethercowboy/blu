using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using blu.Common.Enums;
using blu.Common.Sources;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        private string Url { get; } = "https://hoopla-ws.hoopladigital.com/search?q=[QUERY]&kind=[KIND]";

        protected override async Task<IEnumerable<string>> SourceLookup(string title, string author, Format format)
        {
            JObject result;

            using (var wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.Add("User-Agent", UserAgent.GoogleChrome);

                var query = BuildQuery(title, author);
                var kind = GetKind(format);

                var lookupUrl = Url.Replace("[QUERY]", query)
                    .Replace("[KIND]", kind);

                var json = await wc.GetStringAsync(lookupUrl);
                result = JObject.Parse(json);
            }

            var results = new List<string>();

            if (result == null || !result["titles"].Any()) return results;

            results.AddRange(result["titles"].Select(doc => doc["title"].ToString()));

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