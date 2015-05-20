using System;
using System.Collections.Generic;
using System.Linq;
using Blu.Enums;

namespace Blu.Sources
{
    public interface ILibrary
    {
        string Url { get; }

        IEnumerable<string> Lookup(string title, string author, Format format);
    }
}
