using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using blu.Common.Enums;
using blu.Common.Extensions;
using blu.Common.Sources;
using HtmlAgilityPack;

namespace blu.Sources.Clevnet.Sources
{
    [Export(typeof(ILibrary))]
    public class Clevnet : Library
    {
        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.AudiobookCd,
            Format.DownloadableAudiobook,
            Format.EBook,
            Format.Print
        };

        private string Url { get; } =
            //"https://clevnet.bibliocommons.com/search?custom_query=[QUERY]&suppress=true&custom_edit=true";
            "https://search.clevnet.org/client/en_US/clevnet/search/results?[QUERY]";

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
                doc.DocumentNode.SelectNodes("//*[@id='results_wrapper']");

            if (childNodes == null)
            {
                childNodes =
                    doc.DocumentNode.SelectNodes(
                        "//*[contains(concat(' ', normalize-space(@class), ' '), ' detail_main ')]");

                if (childNodes == null)
                {
                    yield break;
                }
                else
                {
                    foreach (var element in childNodes)
                    {
                        var valid =
                            element.SelectNodes(
                                "//div[contains(concat(' ', normalize-space(@class), ' '), ' displayElementText TITLE ')]");

                        if (valid == null || !valid.Any())
                        {
                            yield break;
                        }

                        var firstOrDefault = valid.FirstOrDefault();
                        if (firstOrDefault != null) yield return firstOrDefault.InnerText;
                    }
                }
            }

            foreach (var element in childNodes)
            {
                var valid = element.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' results_cell ')]");

                if (valid == null || !valid.Any())
                {
                    yield break;
                }

                var firstOrDefault = valid.FirstOrDefault();
                if (firstOrDefault != null) yield return firstOrDefault.InnerText;
            }
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