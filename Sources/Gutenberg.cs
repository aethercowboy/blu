using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using Blu.Enums;
using System.ComponentModel.Composition;

namespace Blu.Sources
{
    [Export(typeof(ILibrary))]
    public class Gutenberg : ILibrary
    {
        private static readonly IList<Format> allowedFormats = new List<Format>
        {
            Format.EBook,
        };

        private static readonly Object locker = new Object();

        private readonly string filename = "gutenberg-api.txt";
        private readonly string url = "https://www.gutenberg.org/ebooks/search/?query=[QUERY]";
        private DateTime nextAccessTime;

        public Gutenberg()
        {
            if (!File.Exists(filename))
            {
                File.Create(filename);
            }

            using (var reader = new StreamReader(filename))
            {
                string access = reader.ReadLine();

                if (!DateTime.TryParse(access, out nextAccessTime))
                {
                    nextAccessTime = DateTime.Now;
                }
            }
        }

        public string Url
        {
            get
            {
                return url;
            }
        }

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            if (!allowedFormats.Contains(format))
            {
                yield break;
            }

            lock (locker)
            {
                DateTime now = DateTime.Now;

                if (nextAccessTime > now)
                {
                    Console.WriteLine("?");
                    yield break;
                }

                var wc = new WebClient();
                wc.Headers.Add("user-agent", UserAgent.GoogleChrome);

                string query = BuildQuery(title, author);

                string lookupUrl = Url.Replace("[QUERY]", query);

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

                HtmlNodeCollection childNodes = doc.DocumentNode.SelectNodes("//ul[contains(concat(' ', normalize-space(@class), ' '), ' results ')]");

                if (childNodes == null)
                {
                    yield break;
                }

                foreach (HtmlNode element in childNodes)
                {
                    HtmlNodeCollection valid = element.SelectNodes("li[contains(concat(' ', normalize-space(@class), ' '), ' booklink ')]");

                    if (valid == null || !valid.Any())
                    {
                        yield break;
                    }

                    yield return valid.FirstOrDefault().InnerText;
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
            if (!File.Exists(filename))
            {
                File.Create(filename);
            }

            using (var writer = new StreamWriter(filename))
            {
                DateTime date = DateTime.Now + ts;

                writer.WriteLine(date);
                nextAccessTime = date;
            }
        }

        private string BuildQuery(string title, string author)
        {
            var sb = new StringBuilder();

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
