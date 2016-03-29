using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaPatterns.Abstractions
{
    public interface IMetaPatternTemplate
    {
        void Compile(MetaPatternCompilerContext context);
    }
}
