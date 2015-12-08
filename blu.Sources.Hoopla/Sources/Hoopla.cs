using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Blu.Enums;
using HtmlAgilityPack;

namespace Blu.Sources
{
    [Export(typeof(ILibrary))]
    internal class Hoopla : ILibrary
    {
        private static readonly IList<Format> AllowedFormats = new List<Format>
        {
            Format.DownloadableAudiobook,
            Format.EBook,
            Format.EComic,
            Format.EMusic
        };

        public string Url { get; } = "https://www.hoopladigital.com/search?results=&q=[QUERY]&kind=[KIND]";

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            if (!AllowedFormats.Contains(format))
            {
                yield break;
            }

            var wc = new WebClient();
            wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

            var query = BuildQuery(title, author);
            var kind = GetKind(format);

            var lookupUrl = Url.Replace("[QUERY]", query)
                .Replace("[KIND]", kind);

            var response = wc.DownloadString(lookupUrl);

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

        public string BuildQuery(string title, string author)
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