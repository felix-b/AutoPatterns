using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MetaPatterns.Abstractions;

namespace MetaPatterns.Tests.Examples
{
    public class ExampleAutomaticPropertyFactory : MetaPatternsFactory
    {
        public ExampleAutomaticPropertyFactory() 
            : base(new[] { Assembly.GetExecutingAssembly() })
        {
        }

        public T CreateInstance<T>()
        {
            var key = new TypeKey<Type>(typeof(T));
            var instance = base.CreateInstance(key, constructorIndex: 0);
            return (T)instance;
        }
    }
}
