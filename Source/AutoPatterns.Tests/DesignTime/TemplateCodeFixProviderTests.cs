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

            var originalSource = CompleteCompilationUnitSource(@"
                [MetaProgram.Annotation.ClassTemplate]
                public partial class MyTemplate 
                { 
                    public void MyMethod() 
                    {  
                    } 
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void PreprocessTemplate_EmptyAndNonPartial()
        {
            //-- arrange

            #region Original Source

            var originalSource = CompleteCompilationUnitSource(@"
                [MetaProgram.Annotation.ClassTemplate]
                public class MyTemplate 
                { 
                    public void MyMethod() 
                    {  
                    } 
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void PreprocessTemplate_MissingRequiredUsings()
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
                using AutoPatterns.Runtime;
                using Microsoft.CodeAnalysis.CSharp;
                using Microsoft.CodeAnalysis.CSharp.Syntax;
                using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
                namespace MyNS 
                {
                    [MetaProgram.Annotation.ClassTemplate]
                    public partial class MyTemplate 
                    { 
                        public void MyMethod() 
                        {  
                        } 
                    }
                    public partial class MyTemplate : IPatternTemplate
                    { 
                        void IPatternTemplate.Apply(PatternWriterContext context)
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
        public void PreprocessTemplate_PreviouslyGeneratedPartial()
        {
            //-- arrange

            #region Original Source

            var originalSource = CompleteCompilationUnitSource(@"
                [MetaProgram.Annotation.ClassTemplate]
                public class MyTemplate 
                { 
                    public void MyMethod() 
                    {  
                    } 
                }
                public partial class MyTemplate : IPatternTemplate
                {
                    // previously generated partial
                    void IPatternTemplate.Apply(PatternWriterContext context)
                    {
                        // previously generated partial
                        System.Console.WriteLine();
                    }
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
