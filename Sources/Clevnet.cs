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
    public class Clevnet : ILibrary
    {
        private string url = "http://clevnet.bibliocommons.com/search?custom_query=[QUERY]&suppress=true&custom_edit=false";
        public string Url
        {
            get { return url; }
        }

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

            string query = BuildQuery(title, author, format);

            string lookupUrl = Url.Replace("[QUERY]", query);

            string response = wc.DownloadString(lookupUrl);

            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' info ')]");

            if (childNodes == null) {
                yield break;
            }

            foreach (HtmlNode element in childNodes)
            {
                var valid = element.SelectNodes("span[contains(concat(' ', normalize-space(@class), ' '), ' title ')]");

                if (valid == null || !valid.Any()) {
                    yield break;
                }

                yield return valid.FirstOrDefault().InnerText;
            }
        }

        private string BuildQuery(string title, string author, Format format)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format("title:({0}) ", title));

            sb.Append("AND ");

            sb.Append(String.Format("contributor:({0}) ", author));

            sb.Append("AND ");

            string fmt = String.Empty;

            switch (format) {
                case Format.DownloadableAudiobook:
                    fmt = "AB";
                    break;
                case Format.EBook:
                    fmt = "EBOOK";
                    break;
                case Format.AudiobookCD:
                    fmt = "BOOK_CD";
                    break;
                case Format.Print:
                    fmt = "BK";
                    break;
                default:
                    break;
            }

            sb.Append(String.Format("formatcode:({0})", fmt));

            return sb.ToString()
                .Replace(" ", "%20")
                .Replace(":", "%3A")
                .Replace("(", "%28")
                .Replace(")", "%29");
        }
    }
}
