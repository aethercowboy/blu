using Blu.Sources;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blu
{
    public class BlueEngine
    {
        [ImportMany(typeof(ILibrary))]
        public IEnumerable<ILibrary> Libraries { get; set; }
    }
}
