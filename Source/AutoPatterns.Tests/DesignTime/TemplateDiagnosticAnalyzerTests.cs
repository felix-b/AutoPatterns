using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.DesignTime;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.DesignTime
{
    [TestFixture]
    public class TemplateDiagnosticAnalyzerTests : DesignTimeUnitTestBase
    {
        [Test]
        public void WithoutTemplateAttribute_NoDiagnostics()
        {
            //-- arrange

            var source = @"#line 1
                namespace MyNS 
                {
                    public class MyClass 
                    { 
                        public void MyMethod() 
                        {  
                        } 
                    }
                }
            ";
            var document = base.CreateDocumentVerifyCompilable(source);
            var analyzer = new TemplateDiagnosticAnalyzer();

            //-- act

            var diagnostics = base.AnalyzeDocuments(analyzer, document);

            //-- assert

            diagnostics.ShouldNotContainAnalyzerDiagnostics(analyzer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void WithTemplateAttribute_IPatternTemplateNotImplemented_HasDiagnostics()
        {
            //-- arrange

            var source = @"#line 1
                using AutoPatterns; 
                namespace MyNS 
                {
                    [MetaProgram.Annotation.ClassTemplate]
                    public class MyClass 
                    { 
                        public void MyMethod() 
                        {  
                        } 
                    }
                }
            ";
            var document = base.CreateDocumentVerifyCompilable(source);
            var analyzer = new TemplateDiagnosticAnalyzer();

            //-- act

            var diagnostics = base.AnalyzeDocuments(analyzer, document);

            //-- assert

            diagnostics.ShouldMatchAnalyzerDiagnostics(
                analyzer, 
                new DiagnosticExpectation(
                    DiagnosticSeverity.Warning, 
                    TemplateDiagnosticIds.TemplateWasNotPreprocessed, 
                    "Test0.cs", 
                    atLine: 5) 
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void WithTemplateAttribute_IPatternTemplateImplementedInPlace_NoDiagnostics()
        {
            //-- arrange

            var source = @"#line 1
                using AutoPatterns; 
                using AutoPatterns.DesignTime;
                using AutoPatterns.Runtime;
                namespace MyNS 
                {
                    [MetaProgram.Annotation.ClassTemplate]
                    public class MyClass : IPatternTemplate
                    { 
                        public void MyMethod() 
                        {        
                        } 
                        public void Apply(PatternWriterContext context)
                        {
                        }
                    }
                }
            ";
            var document = base.CreateDocumentVerifyCompilable(source);
            var analyzer = new TemplateDiagnosticAnalyzer();

            //-- act

            var diagnostics = base.AnalyzeDocuments(analyzer, document);

            //-- assert

            diagnostics.ShouldNotContainAnalyzerDiagnostics(analyzer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void WithTemplateAttribute_IPatternTemplateImplementedInPartial_NoDiagnostics()
        {
            //-- arrange

            var source = @"#line 1
                using AutoPatterns; 
                using AutoPatterns.DesignTime;
                using AutoPatterns.Runtime;
                namespace MyNS 
                {
                    [MetaProgram.Annotation.ClassTemplate]
                    public partial class MyClass
                    { 
                        public void MyMethod() 
                        {        
                        } 
                    }
                    partial class MyClass : IPatternTemplate
                    { 
                        void IPatternTemplate.Apply(PatternWriterContext context)
                        {
                        }
                    }
                }
            ";
            var document = base.CreateDocumentVerifyCompilable(source);
            var analyzer = new TemplateDiagnosticAnalyzer();

            //-- act

            var diagnostics = base.AnalyzeDocuments(analyzer, document);

            //-- assert

            diagnostics.ShouldNotContainAnalyzerDiagnostics(analyzer);
        }
    }
}
