using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MetaPatterns.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaPatterns
{
    public class Net45MetaPatternsPlatform : IMetaPatternCompilerPlatform
    {
        private readonly object _referenceCacheSyncRoot = new object();
        private ImmutableDictionary<string, MetadataReference> _referenceCache = ImmutableDictionary.Create<string, MetadataReference>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMetaPatternCompilerPlatform

        public MetadataReference CreateMetadataReference(Assembly assembly)
        {
            return MetadataReference.CreateFromFile(assembly.Location);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Assembly LoadAssemblyFromBytes(byte[] bytes)
        {
            return Assembly.Load(bytes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Print(string message)
        {
            Console.WriteLine(message);
        }

        #endregion
    }
}
