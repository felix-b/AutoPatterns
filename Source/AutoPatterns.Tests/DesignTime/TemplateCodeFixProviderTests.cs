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
    public class TemplateCodeFixProviderTests : DesignTimeUnitTestBase
    {
        [Test]
        public void PreprocessTemplate_EmptyAndPartial()
        {
            //-- arrange

            #region Original Source
            var originalSource = @"#line 1
                using AutoPatterns; 
                namespace MyNS 
                {
                    [MetaProgram.Annotation.ClassTemplate]
                    public partial class MyClass 
                    { 
                        public void MyMethod() 
                        {  
                        } 
                    }
                }
            ";
            #endregion

            #region Expected Source after Fix
            var expectedSourceAfterFix = @"#line 1
                using AutoPatterns; 
                namespace MyNS 
                {
                    [MetaProgram.Annotation.ClassTemplate]
                    public partial class MyTemplate 
                    { 
                        public void MyMethod() 
                        {  
                        } 
                    }
                    partial class MyTemplate : AutoPatterns.DesignTime.IPatternTemplate
                    { 
                        void IPatternTemplate.Apply(AutoPatterns.Runtime.PatternWriterContext context)
                        {
                        }
                    }
                }
            ";
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
