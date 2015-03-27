using blu.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blu.Sources
{
    public interface ILibrary
    {
        string Url { get; }
        IEnumerable<string> Lookup(string title, string author, Format format);
    }
}
