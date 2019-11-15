using System;

namespace blu.Sources.GeaugaCountyPublicLibrary.Sources
{
    [Obsolete("Geauga County Public Library's catalog has merged with CLEVNET.", true)]
    public class GeaugaCountyPublicLibrary //: Library
    {
        //protected override IList<Format> AllowedFormats => new List<Format>
        //{
        //    Format.AudiobookCd,
        //    Format.DownloadableAudiobook,
        //    Format.EBook,
        //    Format.Print,
        //    Format.EMusic
        //};

        //private string Url { get; } =
        //    "http://geapl-mt.iii.com/iii/encore/search/C__S[QUERY]__Orightresult__U?lang=eng&suite=cobalt";

        //protected override async Task<IEnumerable<string>> SourceLookup(string title, string author, Format format)
        //{
        //    string response;

        //    using (var wc = new HttpClient())
        //    {
        //        wc.DefaultRequestHeaders.Add("User-Agent", UserAgent.GoogleChrome);
        //        var query = BuildQuery(title, author, format);

        //        var lookupUrl = Url.Replace("[QUERY]", query);

        //        response = await wc.GetStringAsync(lookupUrl);
        //    }

        //    var doc = new HtmlDocument();

        //    doc.LoadHtml(response);

        //    var childNodes =
        //        doc.DocumentNode.Descendants("div").Where(x => HtmlNodeHasClass(x, "dpBibTitle")).ToList();

        //    var results = new List<string>();

        //    if (!childNodes.Any())
        //    {
        //        return results;
        //    }

        //    foreach (var element in childNodes)
        //    {
        //        var valid = element.Descendants("span").Where(x => HtmlNodeHasClass(x, "title")).ToList();

        //        if (!valid.Any())
        //        {
        //            return results;
        //        }

        //        var firstOrDefault = valid.FirstOrDefault();
        //        if (firstOrDefault != null) results.Add(firstOrDefault.InnerText);
        //    }

        //    return results;
        //}

        //private string BuildQuery(string title, string author, Format format)
        //{
        //    var sb = new StringBuilder();

        //    sb.Append($"t:({title}) ");

        //    sb.Append($"a:({author}) ");

        //    var fmt = string.Empty;

        //    switch (format)
        //    {
        //        case Format.DownloadableAudiobook:
        //            fmt = "y";
        //            break;
        //        case Format.EBook:
        //            fmt = "z";
        //            break;
        //        case Format.AudiobookCd:
        //            fmt = "i";
        //            break;
        //        case Format.Print:
        //            fmt = "a";
        //            break;
        //        case Format.EMusic:
        //            fmt = "w";
        //            break;
        //        case Format.EComic:
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(format), format, null);
        //    }

        //    sb.Append($"f:({fmt})");

        //    return sb.ToString().UrlEscape();
        //}
    }
}
