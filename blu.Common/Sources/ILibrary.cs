using System.Collections.Generic;
using blu.Common.Enums;

namespace blu.Common.Sources
{
    public interface ILibrary
    {
        IEnumerable<string> Lookup(string title, string author, Format format);
    }

    public abstract class Library : ILibrary
    {
        protected abstract IList<Format> AllowedFormats { get; }
        public virtual IEnumerable<string> Lookup(string title, string author, Format format)
        {
            if (!AllowedFormats.Contains(format))
            {
                yield break;
            }

            foreach (var sourceResult in SourceLookup(title, author, format))
            {
                yield return sourceResult;
            }
        }

        protected abstract IEnumerable<string> SourceLookup(string title, string author, Format format);
    }
}