using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using blu.Common;
using blu.Common.Enums;
using blu.Common.Sources;
using Newtonsoft.Json;
using blu.Common.Extensions;

namespace blu.Sources.ArchiveOrg.Sources
{
    public class ArchiveOrg : Library
    {
        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.EBook,
            Format.DownloadableAudiobook
        };

        private const string Url = "https://archive.org/advancedsearch.php?q=[QUERY]&output=json";

        protected override async Task<IEnumerable<string>> SourceLookup(string title, string author, Format format)
        {
            try
            {
                var query = BuildQuery(title, author, format);

                var lookupUrl = Url.Replace("[QUERY]", query);

                var json = await HttpClient.GetStringAsync(lookupUrl);
                dynamic result = JsonConvert.DeserializeObject(json);

                var results = new List<string>();

                if (result == null || result.response.numFound <= 0) return results;

                foreach (var doc in result.response.docs)
                {
                    results.Add(doc.title.ToString());
                }

                return results;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static string BuildQuery(string title, string author, Format format)
        {
            var parts = new List<string> { $"title:({title})", $"creator:({author})" };

            var fmt = string.Empty;

            switch (format)
            {
                case Format.EBook:
                    fmt = "texts";
                    break;
                case Format.DownloadableAudiobook:
                    fmt = "audio";
                    break;
            }

            parts.Add($"mediatype:({fmt})");

            return string.Join(" AND ", parts).UrlEscape();
        }
    }
}
