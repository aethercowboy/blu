using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using blu.Common.Enums;
using blu.Common.Sources;
using HtmlAgilityPack;
using System.Linq;
using blu.Common;
using blu.Common.Extensions;

namespace blu.Sources.Clevnet.Sources
{
    public class Clevnet : Library
    {

        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.AudiobookCd,
            Format.DownloadableAudiobook,
            Format.EBook,
            Format.Print
        };

        private string Url { get; } = "https://search.clevnet.org/client/en_US/clevnet/search/results?[QUERY]";

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
                doc.DocumentNode.Descendants("div").Where(x => x.Id == "results_wrapper").ToList();

            var results = new List<string>();

            if (!childNodes.Any())
            {
                childNodes = doc.DocumentNode.Descendants("div").Where(x => HtmlNodeHasClass(x, "detail_main")).ToList();

                if (!childNodes.Any())
                {
                    return results;
                }

                foreach (var element in childNodes)
                {
                    var valid =
                        element.Descendants("div")
                            .Where(x => HtmlNodeHasClass(x, "displayElementText") && HtmlNodeHasClass(x, "TITLE"))
                            .ToList();

                    if (valid == null || !valid.Any())
                    {
                        return results;
                    }

                    var firstOrDefault = valid.FirstOrDefault();
                    if (firstOrDefault != null) results.Add(firstOrDefault.InnerText);
                }
            }

            foreach (var element in childNodes)
            {
                var valid =
                    element.Descendants("div").Where(x => HtmlNodeHasClass(x, "results_cell")).ToList();

                if (!valid.Any())
                {
                    return results;
                }

                var firstOrDefault = valid.FirstOrDefault();
                if (firstOrDefault != null) results.Add(firstOrDefault.InnerText);
            }

            return results;
        }

        private string BuildQuery(string title, string author, Format format)
        {
            var parts = new List<string> {$"qu=TITLE%3D{title}", $"qu=AUTHOR%3D{author}"};

            var fmt = string.Empty;

            switch (format)
            {
                case Format.DownloadableAudiobook:
                    fmt = "EAUDIOBOOKS";
                    break;
                case Format.EBook:
                    fmt = "E-BOOKS";
                    break;
                case Format.AudiobookCd:
                    fmt = "AUDIOBOOKS";
                    break;
                case Format.Print:
                    fmt = "BOOK";
                    break;
                case Format.EComic:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            parts.Add($"lm={fmt}");

            return string.Join("&", parts).UrlEscape();
        }
    }
}