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
            var originalSource = NormalizeSourceCode(@"
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
                }
            ");
            #endregion

            #region Expected Source after Fix

            var expectedSourceAfterFix = NormalizeSourceCode(@"
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

                    public partial class MyTemplate : AutoPatterns.Runtime.IPatternTemplate
                    { 
                        void AutoPatterns.Runtime.IPatternTemplate.Apply(AutoPatterns.Runtime.PatternWriterContext context)
                        {
                        }
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void PreprocessTemplate_EmptyAndNonPartial()
        {
            //-- arrange

            #region Original Source
            var originalSource = NormalizeSourceCode(@"
                using AutoPatterns; 
                namespace MyNS 
                {
                    [MetaProgram.Annotation.ClassTemplate]
                    public class MyTemplate 
                    { 
                        public void MyMethod() 
                        {  
                        } 
                    }
                }
            ");
            #endregion

            #region Expected Source after Fix

            var expectedSourceAfterFix = NormalizeSourceCode(@"
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

                    public partial class MyTemplate : AutoPatterns.Runtime.IPatternTemplate
                    { 
                        void AutoPatterns.Runtime.IPatternTemplate.Apply(AutoPatterns.Runtime.PatternWriterContext context)
                        {
                        }
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
