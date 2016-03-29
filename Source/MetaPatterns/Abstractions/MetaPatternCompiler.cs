using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaPatterns.Abstractions
{
    public abstract class MetaPatternCompiler
    {
        public const string FactoryMethodNamePrefix = "FactoryMethod__";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly IMetaPatternCompilerPlatform _platform;
        private readonly List<ClassDeclarationSyntax> _classes;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MetaPatternCompiler(IMetaPatternCompilerPlatform platform)
        {
            _platform = platform;
            _classes = new List<ClassDeclarationSyntax>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildClassSyntax(TypeKey typeKey, Type baseType = null, Type primaryInterface = null)
        {
            BuildClassSyntax(
                typeKey, 
                baseType, 
                primaryInterfaces: primaryInterface != null ? new[] { primaryInterface } : null, 
                secondaryInterfaces: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildClassSyntax(TypeKey typeKey, Type baseType, Type[] primaryInterfaces, Type[] secondaryInterfaces)
        {
            var context = new MetaPatternCompilerContext(typeKey, baseType, primaryInterfaces, secondaryInterfaces);
            var pipeline = BuildPipeline(context);

            for (int i = 0 ; i < pipeline.Length ; i++)
            {
                pipeline[i].Compile(context);
            }

            var classSyntax = GetClassSyntax(context);
            _classes.Add(classSyntax);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract IMetaPatternTemplate[] BuildPipeline(MetaPatternCompilerContext context);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ClassDeclarationSyntax GetClassSyntax(MetaPatternCompilerContext context)
        {
            return null;
        }
    }
}
