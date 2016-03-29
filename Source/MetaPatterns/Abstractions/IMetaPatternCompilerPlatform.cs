using System;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaPatterns.Abstractions
{
    public interface IMetaPatternCompilerPlatform
    {
        ISyntaxCache CreateSyntaxCache();
        MetadataReference GetMetadataReference(Assembly assembly);
        MetadataReference GetMetadataReference(Type type);
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
