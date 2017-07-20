using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using blu.Common;
using blu.Common.Enums;
using blu.Common.Sources;
using HtmlAgilityPack;

namespace blu.Sources.HarrisCountyPublicLibrary.Sources
{
    public class HarrisCountyPublicLibrary : Library
    {
        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.DownloadableAudiobook,
            Format.EBook
        };

        private string Url { get; } = "http://hcpl.ent.sirsi.net/client/webcat/search/results?qu=[QUERY]";

        protected override async Task<IEnumerable<string>> SourceLookup(string title, string author, Format format)
        {
            string response;

            using (var wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.Add("User-Agent", UserAgent.GoogleChrome);

                var query = BuildQuery(title, author, format);

                var lookupUrl = Url.Replace("[QUERY]", query);

                response = await wc.GetStringAsync(lookupUrl);
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes =
                doc.DocumentNode.Descendants("div").Where(x => HtmlNodeHasClass(x, "results_cell")).ToList();

            var results = new List<string>();

            if (childNodes == null)
            {
                return results;
            }

            results.AddRange(
                childNodes.Select(element => element.Descendants("div").Where(x => HtmlNodeHasClass(x, "results_bio")))
                    .TakeWhile(valid => valid != null && valid.Any())
                    .Select(valid => valid.FirstOrDefault())
                    .Where(firstOrDefault => firstOrDefault != null)
                    .Select(firstOrDefault => firstOrDefault.InnerText));

            return results;
        }

        private static string BuildQuery(string title, string author, Format format)
        {
            var sb = new StringBuilder();

            sb.Append($"&qu=TITLE%3D{title}");

            sb.Append($"&qu=AUTHOR%3D{author} ");

            var fmt = string.Empty;

            switch (format)
            {
                case Format.DownloadableAudiobook:
                    fmt = "E_SOUNDREC\teAudiobook";
                    break;
                case Format.EBook:
                    fmt = "E_BOOK\teBook";
                    break;
            }

            sb.Append($"&qf=FORMAT\tFormat\t{fmt}");

            return sb.ToString()
                .Replace("\t", "%09")
                .Replace(" ", "%20")
                .Replace(":", "%3A")
                .Replace("(", "%28")
                .Replace(")", "%29");
        }
    }
}