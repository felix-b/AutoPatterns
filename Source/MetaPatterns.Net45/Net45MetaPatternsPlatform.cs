using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        #region Implementation of IMetaPatternCompilerPlatform

        public ISyntaxCache CreateSyntaxCache()
        {
            return new SyntaxCache();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetadataReference GetMetadataReference(Assembly assembly)
        {
            return MetadataReference.CreateFromFile(assembly.Location);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetadataReference GetMetadataReference(Type type)
        {
            return MetadataReference.CreateFromFile(type.Assembly.Location);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SyntaxCache : ISyntaxCache
        {
            private readonly ConcurrentDictionary<TypeKey, MemberDeclarationSyntax> _classSyntaxByKey = 
                new ConcurrentDictionary<TypeKey, MemberDeclarationSyntax>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IClassSyntaxCache

            public MemberDeclarationSyntax GetOrBuild(TypeKey key, Func<MemberDeclarationSyntax> builder)
            {
                return _classSyntaxByKey.GetOrAdd(key, k => builder());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MemberDeclarationSyntax[] ExportAll()
            {
                return _classSyntaxByKey.Values.ToArray();
            }

            #endregion
        }
    }
}
