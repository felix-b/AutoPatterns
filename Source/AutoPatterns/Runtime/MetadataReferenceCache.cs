using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace AutoPatterns.Runtime
{
    internal class MetadataReferenceCache
    {
        private readonly object _syncRoot = new object();
        private ImmutableDictionary<string, MetadataReference> _referenceByAssemblyName = ImmutableDictionary.Create<string, MetadataReference>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetadataReference EnsureReference(Assembly assembly)
        {
            var cacheKey = assembly.FullName;
            MetadataReference reference;

            if (!_referenceByAssemblyName.TryGetValue(cacheKey, out reference))
            {
                lock (_syncRoot)
                {
                    if (!_referenceByAssemblyName.TryGetValue(cacheKey, out reference))
                    {
                        reference = MetadataReference.CreateFromFile(assembly.Location);
                        _referenceByAssemblyName = _referenceByAssemblyName.Add(cacheKey, reference);
                    }
                }
            }

            return reference;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetadataReference EnsureReference(Type type)
        {
            return EnsureReference(type.GetTypeInfo().Assembly);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<MetadataReference> GetAllReferences()
        {
            return _referenceByAssemblyName.Values;
        }
    }
}
