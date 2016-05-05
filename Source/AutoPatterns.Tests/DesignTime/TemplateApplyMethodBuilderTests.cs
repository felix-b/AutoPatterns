using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.DesignTime;
using NUnit.Framework;

namespace AutoPatterns.Tests.DesignTime
{
    [TestFixture]
    public class TemplateApplyMethodBuilderTests : DesignTimeUnitTestBase
    {
        [Test]
        public void Attribute_ClassLevel_NoValues()
        {
            //-- arrange

            #region Original Source

            var originalSource = CompleteCompilationUnitSource(@"
                using System.Runtime.Serialization;
                [MetaProgram.Annotation.ClassTemplate]
                [DataContractAttribute]
                public partial class MyTemplate 
                { 
                }
            ");

            #endregion

            #region Expected Source after Fix

            var expectedSourceAfterFix = CompleteCompilationUnitSource(@"
                [MetaProgram.Annotation.ClassTemplate]
                public partial class MyTemplate 
                { 
                    public void MyMethod() 
                    {  
                    } 
                }
                [AutoPatterns.DesignTime.GeneratedTemplateImplementationAttribute(Hash = -909438831)]
                public partial class MyTemplate : IPatternTemplate
                { 
                    void IPatternTemplate.Apply(PatternWriterContext context)
                    {
                    }
                }
            ");

            #endregion

            //-- act & assert

            base.RunDiagnosticsAndCodefixEndToEnd(
                new TemplateDiagnosticAnalyzer(),
                new TemplateCodeFixProvider(),
                originalSource,
                expectedSourceAfterFix,
                allowNewCompilerDiagnostics: false);
        }
    }
}
