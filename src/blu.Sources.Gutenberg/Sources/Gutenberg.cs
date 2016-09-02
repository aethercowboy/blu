using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using blu.Common.Enums;
using blu.Common.Sources;
using HtmlAgilityPack;
using System.Linq;
using blu.Common.Extensions;

namespace blu.Sources.Gutenberg.Sources
{
    public class Gutenberg : Library
    {
        private const string Filename = "gutenberg-api.txt";

        protected override IList<Format> AllowedFormats => new List<Format>
        {
            Format.EBook
        };

        private static readonly object Locker = new object();
        private DateTime _nextAccessTime;

        public Gutenberg()
        {
            if (!File.Exists(Filename))
            {
                var x = File.Create(Filename);
                x.Dispose();
            }

            var access = File.ReadLines(Filename).FirstOrDefault();

            if (!DateTime.TryParse(access, out _nextAccessTime))
            {
                _nextAccessTime = DateTime.Now;
            }
        }

        private string Url { get; } = "http://www.gutenberg.org/ebooks/search/?query=[QUERY]";

        protected override async Task<IEnumerable<string>> SourceLookup(string title, string author, Format format)
        {
            lock (Locker)
            {
                var now = DateTime.Now;

                var results = new List<string>();

                if (_nextAccessTime > now)
                {
                    Console.WriteLine("?");
                    return results;
                }

                string response;

                using (var wc = new HttpClient())
                {
                    wc.DefaultRequestHeaders.Add("User-Agent", UserAgent.GoogleChrome);

                    var query = BuildQuery(title, author);

                    var lookupUrl = Url.Replace("[QUERY]", query);

                    try
                    {
                        response = wc.GetStringAsync(lookupUrl).Result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Gutenberg is blocking us ({ex.Message}). Let's wait a day.");
                        var wait = new TimeSpan(24, 0, 0);

                        UpdateAccessTime(wait);
                        return results;
                    }
                }

                var doc = new HtmlDocument();

                doc.LoadHtml(response);

                UpdateAccessTime();

                var childNodes =
                    doc.DocumentNode.Descendants("ul").Where(x => HtmlNodeHasClass(x, "results")).ToList();

                if (childNodes == null)
                {
                    return results;
                }

                foreach (var element in childNodes)
                {
                    var valid =
                        element.Descendants("li").Where(x => HtmlNodeHasClass(x, "booklink")).ToList();

                    if (valid == null || !valid.Any())
                    {
                        return results;
                    }

                    var firstOrDefault = valid.FirstOrDefault();
                    if (firstOrDefault != null) results.Add(firstOrDefault.InnerText);
                }

                return results;
            }
        }

        private void UpdateAccessTime()
        {
            var ts = new TimeSpan(0, 0, 30);

            UpdateAccessTime(ts);
        }

        private void UpdateAccessTime(TimeSpan ts)
        {
            if (!File.Exists(Filename))
            {
                File.Create(Filename);
            }

            var date = DateTime.Now + ts;

            File.WriteAllText(Filename, date.ToString(CultureInfo.InvariantCulture));

            _nextAccessTime = date;
        }

        private static string BuildQuery(string title, string author)
        {
            var sb = new StringBuilder();

            sb.Append($"t.({title}) ");

            sb.Append($"a.({author}) ");

            sb.Append("l.english");

            return sb.ToString().UrlEscape();
        }
    }
}
