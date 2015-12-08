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
    public class GeaugaCountyPublicLibrary : ILibrary
    {
        private static readonly IList<Format> AllowedFormats = new List<Format>
        {
            Format.AudiobookCD,
            Format.DownloadableAudiobook,
            Format.EBook,
            Format.Print,
            Format.EMusic
        };

        public string Url { get; } =
            "http://geapl-mt.iii.com/iii/encore/search/C__S[QUERY]__Orightresult__U?lang=eng&suite=cobalt";

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
                doc.DocumentNode.SelectNodes(
                    "//div[contains(concat(' ', normalize-space(@class), ' '), ' dpBibTitle ')]");

            if (childNodes == null)
            {
                yield break;
            }

            foreach (var element in childNodes)
            {
                var valid = element.SelectNodes("span[contains(concat(' ', normalize-space(@class), ' '), ' title ')]");

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

            sb.Append($"t:({title}) ");

            sb.Append($"a:({author}) ");

            var fmt = string.Empty;

            switch (format)
            {
                case Format.DownloadableAudiobook:
                    fmt = "y";
                    break;
                case Format.EBook:
                    fmt = "z";
                    break;
                case Format.AudiobookCD:
                    fmt = "i";
                    break;
                case Format.Print:
                    fmt = "a";
                    break;
                case Format.EMusic:
                    fmt = "w";
                    break;
                case Format.EComic:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            sb.Append($"f:({fmt})");

            return sb.ToString().Replace(" ", "%20").Replace(":", "%3A").Replace("(", "%28").Replace(")", "%29");
        }
    }
}