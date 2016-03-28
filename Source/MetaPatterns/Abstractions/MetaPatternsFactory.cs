using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MetaPatterns.Extensions;

namespace MetaPatterns.Abstractions
{
    public abstract class MetaPatternsFactory
    {
        private readonly object _syncRoot = new object();
        private readonly Assembly[] _assemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MetaPatternsFactory(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected object CreateInstance(TypeKey key)
        {

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NamespaceName => this.GetType().Name.TrimSuffix("Factory");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected class TypeEntry
        {
            public TypeEntry(TypeKey key, TypeInfo type)
            {
                this.Key = key;
                this.Type = type;
                this.FactoryMethods = DiscoverFactoryMethods(type);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeKey Key { get; private set; }
            public TypeInfo Type { get; private set; }
            public IReadOnlyList<Delegate> FactoryMethods { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static IReadOnlyList<Delegate> DiscoverFactoryMethods(TypeInfo type)
            {
                type.DeclaringMethod
            }
        }
    }
}
