using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaPatterns.Bases;

namespace MetaPatterns.Tests.Examples
{
    public class ExampleAutomaticPropertyFactory : MetaPatternsFactory
    {
        public ExampleAutomaticPropertyFactory(IMetaPatternsPlatform platform)
            : base(platform)
        {
        }

        public T CreateInstance<T>()
        {
            throw new NotImplementedException();
        }
    }
}
