using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Blu.Enums;
using HtmlAgilityPack;

namespace Blu.Sources
{
    [Export(typeof (ILibrary))]
    public class Gutenberg : ILibrary
    {
        private const string Filename = "gutenberg-api.txt";

        private static readonly IList<Format> AllowedFormats = new List<Format>
        {
            Format.EBook
        };

        private static readonly object Locker = new object();
        private DateTime _nextAccessTime;

        public Gutenberg()
        {
            if (!File.Exists(Filename))
            {
                File.Create(Filename);
            }

            using (var reader = new StreamReader(Filename))
            {
                var access = reader.ReadLine();

                if (!DateTime.TryParse(access, out _nextAccessTime))
                {
                    _nextAccessTime = DateTime.Now;
                }
            }
        }

        public string Url { get; } = "https://www.gutenberg.org/ebooks/search/?query=[QUERY]";

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            if (!AllowedFormats.Contains(format))
            {
                yield break;
            }

            lock (Locker)
            {
                var now = DateTime.Now;

                if (_nextAccessTime > now)
                {
                    Console.WriteLine("?");
                    yield break;
                }

                var wc = new WebClient();
                wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

                var query = BuildQuery(title, author);

                var lookupUrl = Url.Replace("[QUERY]", query);

                string response;

                try
                {
                    response = wc.DownloadString(lookupUrl);
                }
                catch (Exception)
                {
                    Console.WriteLine("Gutenberg is blocking us. Let's wait a day.");
                    var wait = new TimeSpan(24, 0, 0);

                    UpdateAccessTime(wait);
                    yield break;
                }

                var doc = new HtmlDocument();

                doc.LoadHtml(response);

                UpdateAccessTime();

                var childNodes =
                    doc.DocumentNode.SelectNodes(
                        "//ul[contains(concat(' ', normalize-space(@class), ' '), ' results ')]");

                if (childNodes == null)
                {
                    yield break;
                }

                foreach (var element in childNodes)
                {
                    var valid =
                        element.SelectNodes("li[contains(concat(' ', normalize-space(@class), ' '), ' booklink ')]");

                    if (valid == null || !valid.Any())
                    {
                        yield break;
                    }

                    var firstOrDefault = valid.FirstOrDefault();
                    if (firstOrDefault != null) yield return firstOrDefault.InnerText;
                }
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

            using (var writer = new StreamWriter(Filename))
            {
                var date = DateTime.Now + ts;

                writer.WriteLine(date);
                _nextAccessTime = date;
            }
        }

        private string BuildQuery(string title, string author)
        {
            var sb = new StringBuilder();

            sb.Append($"t.({title}) ");

            sb.Append($"a.({author}) ");

            sb.Append("l.english");

            return sb.ToString()
                .Replace(" ", "%20")
                .Replace(":", "%3A")
                .Replace("(", "%28")
                .Replace(")", "%29");
        }
    }
}