using blu.Common.Enums;
using blu.Common.Extensions;
using blu.Common.Sources;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace blu.Sources.Librivox.Sources
{
    public class Librivox : Library
    {

        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.DownloadableAudiobook
        };

        private string Url { get; } = "https://librivox.org/api/feed/audiobooks?[QUERY]";

        protected override async Task<IEnumerable<string>> SourceLookup(string title, string author, Format format)
        {
            var query = BuildQuery(title, author);

            var lookupUrl = Url.Replace("[QUERY]", query);

            string response = null;

            try
            {
                response = await HttpClient.GetStringAsync(lookupUrl);
            }
            catch (Exception)
            {
                // ignored
            }


            var results = new List<string>();

            if (response == null)
            {
                return results;
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes = doc.DocumentNode.Descendants("books");

            if (childNodes == null)
            {
                return results;
            }

            foreach (var element in childNodes)
            {
                var valid = element.Descendants("book");

                if (valid == null || !valid.Any())
                {
                    return results;
                }

                var firstOrDefault = valid.FirstOrDefault();
                if (firstOrDefault != null) results.Add(firstOrDefault.InnerText);
            }

            return results;
        }

        private static string BuildQuery(string title, string author)
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
