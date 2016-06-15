using System.Collections.Generic;
using System.ComponentModel.Composition;
using blu.Common.Sources;

namespace blu
{
    public class BlueEngine
    {
        public BlueEngine()
        {
            Libraries = new List<ILibrary>();
        }

        [ImportMany(typeof(ILibrary))]
        public IEnumerable<ILibrary> Libraries { get; private set; }
    }
}