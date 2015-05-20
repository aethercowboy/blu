using blu.Enums;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace blu.Sources
{
    public class Gutenberg : ILibrary
    {
        private static IList<Format> allowedFormats = new List<Format>
        {
            Format.EBook,
        };

        private static Object _locker = new Object();

        private string _filename = "gutenberg-api.txt";
        private DateTime _nextAccessTime;

        private string url = "https://www.gutenberg.org/ebooks/search/?query=[QUERY]";
        public string Url
        {
            get { return url; }
        }

        public Gutenberg()
        {
            if (!File.Exists(_filename))
            {
                File.Create(_filename);
            }

            using (StreamReader reader = new StreamReader(_filename))
            {
                var access = reader.ReadLine();

                if (!DateTime.TryParse(access, out _nextAccessTime))
                {
                    _nextAccessTime = DateTime.Now;
                }
            }
        }

        private void UpdateAccessTime()
        {
            TimeSpan ts = new TimeSpan(0, 0, 30);

            UpdateAccessTime(ts);
        }

        private void UpdateAccessTime(TimeSpan ts)
        {
            if (!File.Exists(_filename))
            {
                File.Create(_filename);
            }

            using (StreamWriter writer = new StreamWriter(_filename))
            {
                var date = DateTime.Now + ts;

                writer.WriteLine(date);
                _nextAccessTime = date;
            }
        }

        public IEnumerable<string> Lookup(string title, string author, Format format)
        {
            if (!allowedFormats.Contains(format))
            {
                yield break;
            }

            lock (_locker)
            {
                var now = DateTime.Now;

                if (_nextAccessTime > now)
                {
                    Console.WriteLine("?");
                    yield break;
                }

                WebClient wc = new WebClient();
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
                    TimeSpan wait = new TimeSpan(24, 0, 0);

                    UpdateAccessTime(wait);
                    yield break;
                }

                HtmlDocument doc = new HtmlDocument();

                doc.LoadHtml(response);

                UpdateAccessTime();

                var childNodes = doc.DocumentNode.SelectNodes("//ul[contains(concat(' ', normalize-space(@class), ' '), ' results ')]");

                if (childNodes == null)
                {
                    yield break;
                }

                foreach (HtmlNode element in childNodes)
                {
                    var valid = element.SelectNodes("li[contains(concat(' ', normalize-space(@class), ' '), ' booklink ')]");

                    if (valid == null || !valid.Any())
                    {
                        yield break;
                    }

                    yield return valid.FirstOrDefault().InnerText;
                }
            }
        }

        private string BuildQuery(string title, string author)
        {
            StringBuilder sb = new StringBuilder();

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
