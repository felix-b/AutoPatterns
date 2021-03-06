﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.DesignTime;
using AutoPatterns.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.DesignTime
{
    [TestFixture]
    public class TemplateApplyMethodBuilderTests : DesignTimeUnitTestBase
    {
        [Test]
        public void Attribute_ClassLevel()
        {
            //-- arrange

            #region Template Code

            var templateCode = @"
                using System.Runtime.Serialization;
                [MetaProgram.Annotation.ClassTemplate]
                [DataContractAttribute(Namespace=MyTemplate.TestNamespaceName)]
                public partial class MyTemplate 
                { 
                    public const string TestNamespaceName = ""testns"";
                }
            ";

            #endregion

            #region Expected Implementation Code

            var expectedImplementationCode = @"
                [AutoPatterns.DesignTime.GeneratedTemplateImplementationAttribute(Hash = 1553256510)]
                public partial class MyTemplate : IPatternTemplate
                { 
                    void IPatternTemplate.Apply(PatternWriterContext context)
                    {
                        context.Output.ClassWriter.AddClassAttribute(typeof(System.Runtime.Serialization.DataContractAttribute), new object[0], new object[] { ""Namespace"", MyTemplate.TestNamespaceName });
                    }
                }
            ";

            #endregion

            //-- act 

            var implementationSyntax = ImplementTemplate(templateCode);

            //-- assert

            implementationSyntax.ShouldBeSourceCode(expectedImplementationCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void BaseType_Single()
        {
            //-- arrange

            #region Template Code

            var templateCode = @"
                [MetaProgram.Annotation.ClassTemplate]
                public partial class MyExceptionTemplate : Exception
                { 
                }
            ";

            #endregion

            #region Expected Implementation Code

            var expectedImplementationCode = @"
                [AutoPatterns.DesignTime.GeneratedTemplateImplementationAttribute(Hash = 852793621)]
                public partial class MyExceptionTemplate : IPatternTemplate
                { 
                    void IPatternTemplate.Apply(PatternWriterContext context)
                    {
                        context.Output.ClassWriter.AddBaseType(typeof(System.Exception));
                    }
                }
            ";

            #endregion

            //-- act 

            var implementationSyntax = ImplementTemplate(templateCode);

            //-- assert

            implementationSyntax.ShouldBeSourceCode(expectedImplementationCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void BaseTypes_Multiple()
        {
            //-- arrange

            #region Template Code

            var templateCode = @"
                [MetaProgram.Annotation.ClassTemplate]
                public partial class MyExceptionTemplate : Exception, IDisposable
                { 
                    public void Dispose()
                    {
                    }
                }
            ";

            #endregion

            #region Expected Implementation Code

            var expectedImplementationCode = @"
                [AutoPatterns.DesignTime.GeneratedTemplateImplementationAttribute(Hash = 1303872184)]
                public partial class MyExceptionTemplate : IPatternTemplate
                { 
                    void IPatternTemplate.Apply(PatternWriterContext context)
                    {
                        context.Output.ClassWriter.AddBaseType(typeof(System.Exception));
                        context.Output.ClassWriter.AddBaseType(typeof(System.IDisposable));
                    }
                }
            ";

            #endregion

            //-- act 

            var implementationSyntax = ImplementTemplate(templateCode);

            //-- assert

            implementationSyntax.ShouldBeSourceCode(expectedImplementationCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void MethodMember_ImplementOnce()
        {
            //-- arrange

            #region Template Code

            var templateCode = @"
                [MetaProgram.Annotation.ClassTemplate]
                public partial class MyTemplate
                { 
                    [MetaProgram.Annotation.MetaMember(Repeat = RepeatOption.Once)]
                    public void MyMethod()
                    {
                        var rand = new Random();
                        var s = (rand.Next(0, 100) < 50 ? ""A"" : ""B"");
                    }
                }
            ";

            #endregion

            #region Expected Implementation Code

            var expectedImplementationCode = @"
                [AutoPatterns.DesignTime.GeneratedTemplateImplementationAttribute(Hash = 1303872184)]
                public partial class MyTemplate : IPatternTemplate
                { 
                    void IPatternTemplate.Apply(PatternWriterContext context)
                    {
                    }
                }
            ";

            #endregion

            //-- act 

            var implementationSyntax = ImplementTemplate(templateCode);

            //-- assert

            implementationSyntax.ShouldBeSourceCode(expectedImplementationCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ClassDeclarationSyntax ImplementTemplate(string templateSourceCode)
        {
            var newSyntaxRoot = base.RunDiagnosticsAndCodefixEndToEnd(
                new TemplateDiagnosticAnalyzer(),
                new TemplateCodeFixProvider(),
                CompleteCompilationUnitSource(templateSourceCode),
                expectedFinalSource: null,
                allowNewCompilerDiagnostics: false);

            var implementationSyntax = newSyntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(IsTemplateImplementationClassSyntax);
            implementationSyntax.ShouldNotBeNull("Template implementation syntax not found after running the code fix!");

            return implementationSyntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsTemplateImplementationClassSyntax(ClassDeclarationSyntax syntax)
        {
            return syntax.AttributeLists
                .SelectMany(list => list.Attributes)
                .Any(attr => attr.Name.ToFullString() == _s_implementationAttributeTypeFullName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_implementationAttributeTypeFullName = typeof(GeneratedTemplateImplementationAttribute).FullName;
    }
}
