using System;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaPatterns.Abstractions
{
    public interface IMetaPatternCompilerPlatform
    {
        MetadataReference CreateMetadataReference(Assembly assembly);
        Assembly LoadAssemblyFromBytes(byte[] bytes);
        void Print(string message);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISyntaxCache
    {
        MemberDeclarationSyntax GetOrBuild(TypeKey key, Func<MemberDeclarationSyntax> builder);
        MemberDeclarationSyntax[] ExportAll();
    }
}
