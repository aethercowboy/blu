using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Blu.Enums;
using System.ComponentModel.Composition;

namespace Blu.Sources
{
    [Export(typeof(ILibrary))]
    internal class Hoopla : ILibrary
    {
        private static readonly IList<Format> allowedFormats = new List<Format>
        {
            Format.DownloadableAudiobook,
            Format.EBook,
            Format.EComic,
        };

        private readonly string url = "https://www.hoopladigital.com/search?results=&q=[QUERY]&kind=[KIND]";

        public string Url
        {
            get
            {
                return url;
            }
        }

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            if (!allowedFormats.Contains(format))
            {
                yield break;
            }

            var wc = new WebClient();
            wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

            string query = BuildQuery(title, author);
            string kind = GetKind(format);

            string lookupUrl = Url.Replace("[QUERY]", query)
                                  .Replace("[KIND]", kind);

            string response = wc.DownloadString(lookupUrl);

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            HtmlNodeCollection childNodes = doc.DocumentNode.SelectNodes("div[contains(concat(' ', normalize-space(@class), ' '), ' row ')]");

            if (childNodes == null)
            {
                yield break;
            }

            foreach (HtmlNode element in childNodes)
            {
                HtmlNodeCollection valid = element.SelectNodes("div[contains(concat(' ', normalize-space(@class), ' '), ' columns ')]");

                if (valid == null || !valid.Any())
                {
                    yield break;
                }

                yield return valid.FirstOrDefault().InnerText;
            }
        }

        public string BuildQuery(string title, string author)
        {
            string retval = String.Join("+", title.Replace(" ", "+"), author);

            return retval;
        }

        private string GetKind(Format format)
        {
            switch (format)
            {
                case Format.DownloadableAudiobook:
                    return "AUDIOBOOK";
                case Format.EBook:
                    return "EBOOK";
                case Format.EComic:
                    return "COMIC";
                default:
                    return String.Empty;
            }
        }
    }
}
