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

namespace blu.Sources.Librivox.Sources
{
    [Export(typeof(ILibrary))]
    public class Librivox : Library
    {
        protected  override IList<Format> AllowedFormats => new List<Format>
        {
            Format.DownloadableAudiobook
        };

        private string Url { get; } = "https://librivox.org/api/feed/audiobooks?[QUERY]";

        protected override IEnumerable<string> SourceLookup(string title, string author, Format format)
        {
            string response;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

                var query = BuildQuery(title, author);

                var lookupUrl = Url.Replace("[QUERY]", query);

                response = null;

                try
                {
                    response = wc.DownloadString(lookupUrl);
                }
                catch (Exception)
                {
                    // ignored
                }
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

            return sb.ToString().UrlEscape();
        }
    }
}