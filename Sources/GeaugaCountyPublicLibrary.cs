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
    public class GeaugaCountyPublicLibrary : ILibrary
    {
        private static IList<Format> allowedFormats = new List<Format>
        {
            Format.AudiobookCD,
            Format.DownloadableAudiobook,
            Format.EBook,
            Format.Print,
        };

        private string url = "http://geapl-mt.iii.com/iii/encore/search/C__S[QUERY]__Orightresult__U?lang=eng&suite=cobalt";
        public string Url
        {
            get { return url; }
        }

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            if (!allowedFormats.Contains(format))
            {
                yield break;
            }

            WebClient wc = new WebClient();
            wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

            string query = BuildQuery(title, author, format);

            string lookupUrl = Url.Replace("[QUERY]", query);

            string response = wc.DownloadString(lookupUrl);

            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' dpBibTitle ')]");

            if (childNodes == null) {
                yield break;
            }

            foreach (HtmlNode element in childNodes)
            {
                var valid = element.SelectNodes("span[contains(concat(' ', normalize-space(@class), ' '), ' title ')]");

                if (valid == null || !valid.Any())
                {
                    yield break;
                }

                yield return valid.FirstOrDefault().InnerText;
            }
        }

        private string BuildQuery(string title, string author, Format format)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format("t:({0}) ", title));

            sb.Append(String.Format("a:({0}) ", author));

            string fmt = String.Empty;

            switch (format) {
                case Format.DownloadableAudiobook:
                    fmt = "y";
                    break;
                case Format.EBook:
                    fmt = "z";
                    break;
                case Format.AudiobookCD:
                    fmt = "i";
                    break;
                case Format.Print:
                    fmt = "a";
                    break;
                default:
                    break;
            }

            sb.Append(String.Format("f:({0})", fmt));

            return sb.ToString()
                .Replace(" ", "%20")
                .Replace(":", "%3A")
                .Replace("(", "%28")
                .Replace(")", "%29");
        }
    }
}
