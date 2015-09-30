using System.Collections.Generic;
using System.ComponentModel.Composition;
using Blu.Sources;

namespace Blu
{
    public class BlueEngine
    {
        [ImportMany(typeof (ILibrary))]
        public IEnumerable<ILibrary> Libraries { get; set; }
    }
}