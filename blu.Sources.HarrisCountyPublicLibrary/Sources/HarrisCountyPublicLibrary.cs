using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using blu.Common.Enums;
using blu.Common.Sources;
using HtmlAgilityPack;

namespace blu.Sources.HarrisCountyPublicLibrary.Sources
{
    [Export(typeof (ILibrary))]
    public class HarrisCountyPublicLibrary : Library
    {
        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.DownloadableAudiobook,
            Format.EBook
        };

        private string Url { get; } = "http://hcpl.ent.sirsi.net/client/webcat/search/results?qu=[QUERY]";

        protected override IEnumerable<string> SourceLookup(string title, string author, Format format)
        {
            string response;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

                var query = BuildQuery(title, author, format);

                var lookupUrl = Url.Replace("[QUERY]", query);

                response = wc.DownloadString(lookupUrl);
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes =
                doc.DocumentNode.SelectNodes(
                    "//div[contains(concat(' ', normalize-space(@class), ' '), ' results_cell ')]");

            if (childNodes == null)
            {
                yield break;
            }

            foreach (
                var firstOrDefault in
                    childNodes.Select(
                        element =>
                            element.SelectNodes(
                                "div[contains(concat(' ', normalize-space(@class), ' '), ' results_bio ')]"))
                        .TakeWhile(valid => valid != null && valid.Any())
                        .Select(valid => valid.FirstOrDefault())
                        .Where(firstOrDefault => firstOrDefault != null))
            {
                yield return firstOrDefault.InnerText;
            }
        }

        private string BuildQuery(string title, string author, Format format)
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