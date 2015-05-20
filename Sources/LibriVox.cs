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
    [Export(typeof(ILibrary))]
    public class Librivox : ILibrary
    {
        private static readonly IList<Format> allowedFormats = new List<Format>
        {
            Format.DownloadableAudiobook,
        };

        private readonly string url = "https://librivox.org/api/feed/audiobooks?[QUERY]";

        public string Url
        {
            get
            {
                return url;
            }
        }

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            var wc = new WebClient();
            wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

            if (!allowedFormats.Contains(format))
            {
                yield break;
            }

            string query = BuildQuery(title, author);

            string lookupUrl = Url.Replace("[QUERY]", query);

            string response = null;

            try
            {
                response = wc.DownloadString(lookupUrl);
            }
            catch (Exception)
            {
            }

            if (response == null)
            {
                yield break;
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            HtmlNodeCollection childNodes = doc.DocumentNode.SelectNodes("//books");

            if (childNodes == null)
            {
                yield break;
            }

            foreach (HtmlNode element in childNodes)
            {
                HtmlNodeCollection valid = element.SelectNodes("book");

                if (valid == null || !valid.Any())
                {
                    yield break;
                }

                yield return valid.FirstOrDefault().InnerText;
            }
        }

        private string BuildQuery(string title, string author)
        {
            var sb = new StringBuilder();

            sb.Append(String.Format("title={0}", title.Replace(" ", "+")));

            sb.Append(String.Format("&authors={0}", author.Replace(" ", "+")));

            sb.Append(String.Format("&status=complete"));
            sb.Append(String.Format("&recorded_language=1"));

            return sb.ToString()
                     .Replace(" ", "%20")
                     .Replace(":", "%3A")
                     .Replace("(", "%28")
                     .Replace(")", "%29");
        }
    }
}