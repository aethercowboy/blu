using blu.Common.Sources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using blu.Common.Enums;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace blu.Sources.OhioDigitalLibrary.Sources
{
    public class OhioDigitalLibrary : Library
    {
        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.DownloadableAudiobook,
            Format.EBook,
        };

        private string Url => "https://ohdbks.overdrive.com/search/title?[QUERY]&sortBy=newlyadded";
        //https://ohdbks.overdrive.com/search/title?query=sabriel&creator=nix&mediaType=audiobook&sortBy=newlyadded

        protected override async Task<IEnumerable<string>> SourceLookup(string title, string author, Format format)
        {
            string response;

            using (var wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.Add("User-Agent", UserAgent.GoogleChrome);

                var query = BuildQuery(title, author, format);

                var lookupUrl = Url.Replace("[QUERY]", query);

                response = await wc.GetStringAsync(lookupUrl);
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(response);

            var childNodes =
                doc.DocumentNode.Descendants("script").Where(x => x.InnerText.Contains("window.OverDrive.mediaItems")).ToList();

            var results = new List<string>();

            var re = new Regex(@"window[.]OverDrive[.]mediaItems\s*=\s*(.+?);");

            foreach (var element in childNodes)
            {
                var match = re.Match(element.InnerText);
                if (match.Groups.Count < 2)
                {
                    return results;
                }

                var group = match.Groups[1];

                if (group == null)
                {
                    return results;
                }

                var jobject = JObject.Parse(group.Value);

                foreach (var entry in jobject)
                {
                    var entryTitle = entry.Value["title"].ToString();
                    var entryAuthor = entry.Value["firstCreatorName"].ToString();

                    if (!entryTitle.ToLower().Contains(title) || !entryAuthor.ToLower().Contains(author)) continue;

                    results.Add(entryTitle);
                    break;
                }
            }

            return results;
        }

        private string BuildQuery(string title, string author, Format format)
        {
            var parts = new List<string> { $"query={title}", $"creator={author}" };

            switch (format)
            {
                case Format.DownloadableAudiobook:
                    parts.Add("mediaType=audiobook");
                    break;
                case Format.EBook:
                    parts.Add("mediaType=ebook");
                    break;
                default:
                    throw new NotSupportedException("This source does not support that type.");
            }

            return string.Join("&", parts);
        }
    }
}
