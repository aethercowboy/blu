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
    public class Gutenberg : ILibrary
    {
        private string url = "https://www.gutenberg.org/ebooks/search/?query=[QUERY]";
        public string Url
        {
            get { return url; }
        }

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

            if (format != Format.EBook)
            {
                yield break;
            }

            string query = BuildQuery(title, author);

            string lookupUrl = Url.Replace("[QUERY]", query);

            string response = wc.DownloadString(lookupUrl);

            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes = doc.DocumentNode.SelectNodes("//ul[contains(concat(' ', normalize-space(@class), ' '), ' results ')]");

            if (childNodes == null) {
                yield break;
            }

            foreach (HtmlNode element in childNodes)
            {
                var valid = element.SelectNodes("li[contains(concat(' ', normalize-space(@class), ' '), ' booklink ')]");

                if (valid == null || !valid.Any()) {
                    yield break;
                }

                yield return valid.FirstOrDefault().InnerText;
            }
        }

        private string BuildQuery(string title, string author)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format("t.({0}) ", title));

            sb.Append(String.Format("a.({0}) ", author));

            sb.Append(String.Format("l.english"));

            return sb.ToString()
                .Replace(" ", "%20")
                .Replace(":", "%3A")
                .Replace("(", "%28")
                .Replace(")", "%29");
        }
    }
}
