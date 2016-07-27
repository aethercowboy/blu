using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using blu.Common.Enums;
using blu.Common.Extensions;
using blu.Common.Sources;
using Newtonsoft.Json;

namespace blu.Sources.ArchiveOrg.Sources
{
    [Export(typeof(ILibrary))]
    public class ArchiveOrg : Library
    {
        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.EBook
        };

        private const string Url = "https://archive.org/advancedsearch.php?q=[QUERY]&output=json";

        protected override IEnumerable<string> SourceLookup(string title, string author, Format format)
        {
            dynamic result;

            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", UserAgent.GoogleChrome);

                var query = BuildQuery(title, author, format);

                var lookupUrl = Url.Replace("[QUERY]", query);

                var json = client.DownloadString(lookupUrl);
                result = JsonConvert.DeserializeObject(json);
            }

            if (result == null || result.response.numFound <= 0) yield break;

            foreach (var doc in result.response.docs)
            {
                yield return doc.title;
            }
        }

        //https://archive.org/advancedsearch.php?q=title%3A%28fiscal%20wonderland%29%20AND%20creator%3A%28geake%29%20AND%20mediatype%3A%28texts%29&output=json

        private string BuildQuery(string title, string author, Format format)
        {
            var parts = new List<string> { $"title:({title})", $"creator:({author})" };

            var fmt = string.Empty;

            switch (format)
            {
                case Format.EBook:
                    fmt = "texts";
                    break;
            }

            parts.Add($"mediatype:({fmt})");

            return string.Join(" AND ", parts).UrlEscape();
        }
    }
}
