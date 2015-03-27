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
    public class HarrisCountyPublicLibrary : ILibrary
    {
        private string url = "http://hcpl.ent.sirsi.net/client/webcat/search/results?qu=[QUERY]";
        public string Url
        {
            get { return url; }
        }

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            if (format == Format.AudiobookCD || format == Format.Print)
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

            var childNodes = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' results_cell ')]");

            if (childNodes == null) {
                yield break;
            }

            foreach (HtmlNode element in childNodes)
            {
                var valid = element.SelectNodes("div[contains(concat(' ', normalize-space(@class), ' '), ' results_bio ')]");

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

            sb.Append(String.Format("&qu=TITLE%3D{0}", title));

            sb.Append(String.Format("&qu=AUTHOR%3D{0} ", author));

            string fmt = String.Empty;

            switch (format) {
                case Format.DownloadableAudiobook:
                    fmt = "E_SOUNDREC\teAudiobook";
                    break;
                case Format.EBook:
                    fmt = "E_BOOK\teBook";
                    break;
                default:
                    break;
            }

            sb.Append(String.Format("&qf=FORMAT\tFormat\t{0}", fmt));

            return sb.ToString()
                .Replace("\t", "%09")
                .Replace(" ", "%20")
                .Replace(":", "%3A")
                .Replace("(", "%28")
                .Replace(")", "%29");
        }
    }
}
