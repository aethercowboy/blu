using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using Blu.Enums;
using HtmlAgilityPack;

namespace Blu.Sources
{
    [Export(typeof (ILibrary))]
    public class Clevnet : ILibrary
    {
        private static readonly IList<Format> AllowedFormats = new List<Format>
        {
            Format.AudiobookCD,
            Format.DownloadableAudiobook,
            Format.EBook,
            Format.Print
        };

        public string Url { get; } =
            "https://clevnet.bibliocommons.com/search?custom_query=[QUERY]&suppress=true&custom_edit=true";

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            if (!AllowedFormats.Contains(format))
            {
                yield break;
            }

            var wc = new WebClient();
            wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

            var query = BuildQuery(title, author, format);

            var lookupUrl = Url.Replace("[QUERY]", query);

            var response = wc.DownloadString(lookupUrl);

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes =
                doc.DocumentNode.SelectNodes("//*[@id='bibList']");

            if (childNodes == null)
            {
                yield break;
            }

            foreach (var element in childNodes)
            {
                var valid = element.SelectNodes("//span[contains(concat(' ', normalize-space(@class), ' '), ' title ')]");

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
            var sb = new StringBuilder();

            sb.Append($"title:({title}) ");

            sb.Append("AND ");

            sb.Append($"contributor:({author}) ");

            sb.Append("AND ");

            var fmt = string.Empty;

            switch (format)
            {
                case Format.DownloadableAudiobook:
                    fmt = "AB";
                    break;
                case Format.EBook:
                    fmt = "EBOOK";
                    break;
                case Format.AudiobookCD:
                    fmt = "BOOK_CD";
                    break;
                case Format.Print:
                    fmt = "BK";
                    break;
                case Format.EComic:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            sb.Append($"formatcode:({fmt})");

            return sb.ToString().Replace(" ", "%20").Replace(":", "%3A").Replace("(", "%28").Replace(")", "%29");
        }
    }
}