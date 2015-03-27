using blu.Enums;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace blu.Sources
{
    public class LibriVox : ILibrary
    {
        private string url = "https://librivox.org/api/feed/audiobooks?[QUERY]";
        public string Url
        {
            get { return url; }
        }

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

            if (format != Format.DownloadableAudiobook)
            {
                yield break;
            }

            string query = BuildQuery(title, author);

            string lookupUrl = Url.Replace("[QUERY]", query);

            string response = wc.DownloadString(lookupUrl);

            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes = doc.DocumentNode.SelectNodes("//books");

            if (childNodes == null) {
                yield break;
            }

            foreach (HtmlNode element in childNodes)
            {
                var valid = element.SelectNodes("book");

                if (valid == null || !valid.Any()) {
                    yield break;
                }

                yield return valid.FirstOrDefault().InnerText;
            }
        }

        private string BuildQuery(string title, string author)
        {
            StringBuilder sb = new StringBuilder();

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
