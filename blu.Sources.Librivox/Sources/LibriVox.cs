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
    public class Librivox : ILibrary
    {
        private static readonly IList<Format> AllowedFormats = new List<Format>
        {
            Format.DownloadableAudiobook
        };

        public string Url { get; } = "https://librivox.org/api/feed/audiobooks?[QUERY]";

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            var wc = new WebClient();
            wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

            if (!AllowedFormats.Contains(format))
            {
                yield break;
            }

            var query = BuildQuery(title, author);

            var lookupUrl = Url.Replace("[QUERY]", query);

            string response = null;

            try
            {
                response = wc.DownloadString(lookupUrl);
            }
            catch (Exception)
            {
                // ignored
            }

            if (response == null)
            {
                yield break;
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes = doc.DocumentNode.SelectNodes("//books");

            if (childNodes == null)
            {
                yield break;
            }

            foreach (var element in childNodes)
            {
                var valid = element.SelectNodes("book");

                if (valid == null || !valid.Any())
                {
                    yield break;
                }

                var firstOrDefault = valid.FirstOrDefault();
                if (firstOrDefault != null) yield return firstOrDefault.InnerText;
            }
        }

        private string BuildQuery(string title, string author)
        {
            var sb = new StringBuilder();

            sb.Append($"title={title.Replace(" ", "+")}");

            sb.Append($"&authors={author.Replace(" ", "+")}");

            sb.Append("&status=complete");
            sb.Append("&recorded_language=1");

            return sb.ToString()
                .Replace(" ", "%20")
                .Replace(":", "%3A")
                .Replace("(", "%28")
                .Replace(")", "%29");
        }
    }
}