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

namespace blu.Sources.GeaugaCountyPublicLibrary.Sources
{
    [Export(typeof (ILibrary))]
    public class GeaugaCountyPublicLibrary : Library
    {
        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.AudiobookCd,
            Format.DownloadableAudiobook,
            Format.EBook,
            Format.Print,
            Format.EMusic
        };

        private string Url { get; } =
            "http://geapl-mt.iii.com/iii/encore/search/C__S[QUERY]__Orightresult__U?lang=eng&suite=cobalt";

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
                case Format.AudiobookCd:
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

            return sb.ToString().UrlEscape();
        }
    }
}