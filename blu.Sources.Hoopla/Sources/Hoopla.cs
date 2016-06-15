using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using blu.Common.Enums;
using blu.Common.Sources;
using HtmlAgilityPack;

namespace blu.Sources.Hoopla.Sources
{
    [Export(typeof(ILibrary))]
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

        protected override IEnumerable<string> SourceLookup(string title, string author, Format format)
        {
            string response;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

                var query = BuildQuery(title, author);
                var kind = GetKind(format);

                var lookupUrl = Url.Replace("[QUERY]", query)
                    .Replace("[KIND]", kind);

                response = wc.DownloadString(lookupUrl);
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes =
                doc.DocumentNode.SelectNodes("div[contains(concat(' ', normalize-space(@class), ' '), ' row ')]");

            if (childNodes == null)
            {
                yield break;
            }

            foreach (var element in childNodes)
            {
                var valid = element.SelectNodes("div[contains(concat(' ', normalize-space(@class), ' '), ' columns ')]");

                if (valid == null || !valid.Any())
                {
                    yield break;
                }

                var firstOrDefault = valid.FirstOrDefault();
                if (firstOrDefault != null) yield return firstOrDefault.InnerText;
            }
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