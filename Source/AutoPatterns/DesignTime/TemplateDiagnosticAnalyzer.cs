using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoPatterns.Extensions;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoPatterns.DesignTime
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TemplateDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _s_supportedDiagnostics;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var operation = new AnalysisOperation(context);
            operation.Execute();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_categoryTitle = "AutoPatterns";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly LocalizableString _s_analyzerTitle = 
            new LocalizableResourceString(nameof(Resources.TemplateNotImplementedDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _s_templateNotImplementedMessage = 
            new LocalizableResourceString(nameof(Resources.TemplateNotImplementedMessage), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _s_templateNotImplementedDescription = 
            new LocalizableResourceString(nameof(Resources.TemplateNotImplementedDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _s_templateImplementationOutOfDateMessage = 
            new LocalizableResourceString(nameof(Resources.TemplateImplementationIsOutOfDateMessage), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _s_templateImplementationOutOfDateDescription = 
            new LocalizableResourceString(nameof(Resources.TemplateImplementationIsOutOfDateDescription), Resources.ResourceManager, typeof(Resources));

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DiagnosticDescriptor _s_templateNotImplementedRule = new DiagnosticDescriptor(
            TemplateDiagnosticIds.TemplateIsNotImplemented, 
            _s_analyzerTitle, 
            _s_templateNotImplementedMessage, 
            _s_categoryTitle, 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: _s_templateNotImplementedDescription);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DiagnosticDescriptor _s_templateImplementationOutOfDateRule = new DiagnosticDescriptor(
            TemplateDiagnosticIds.TemplateImplementationIsOutOfDate,
            _s_analyzerTitle,
            _s_templateImplementationOutOfDateMessage,
            _s_categoryTitle,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: _s_templateImplementationOutOfDateDescription);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly ImmutableArray<DiagnosticDescriptor> _s_supportedDiagnostics = ImmutableArray.Create(
            _s_templateNotImplementedRule,
            _s_templateImplementationOutOfDateRule
        );

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_classTemplateAttributeFullName = typeof(MetaProgram.Annotation.ClassTemplateAttribute).FullName;
        private static readonly string _s_generatedImplementationAttributeFullName = typeof(GeneratedTemplateImplementationAttribute).FullName;
        private static readonly string _s_patternTemplateInterfaceFullName = typeof(IPatternTemplate).FullName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AnalysisOperation
        {
            private readonly SymbolAnalysisContext _context;
            private readonly INamedTypeSymbol _typeSymbol;
            private readonly INamedTypeSymbol _classTemplateAttributeTypeSymbol;
            private readonly AttributeData _templateAttributeData;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AnalysisOperation(SymbolAnalysisContext context)
            {
                _context = context;

                _typeSymbol = (INamedTypeSymbol)_context.Symbol;
                _classTemplateAttributeTypeSymbol = _context.Compilation.GetTypeByMetadataName(_s_classTemplateAttributeFullName);
                _templateAttributeData = _typeSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass == _classTemplateAttributeTypeSymbol);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Execute()
            {
                if (_typeSymbol.IsReferenceType && _templateAttributeData != null)
                {
                    if (IsTemplateImplemented())
                    {
                        AttributeData generatedImplementationAttribute;

                        if (IsGeneratedImplementation(out generatedImplementationAttribute) &&
                            !IsTemplateImplementationUpToDate(generatedImplementationAttribute))
                        {
                            _context.ReportDiagnostic(Diagnostic.Create(
                                _s_templateImplementationOutOfDateRule, 
                                _typeSymbol.Locations[0], 
                                _typeSymbol.Name));
                        }
                    }
                    else
                    {
                        _context.ReportDiagnostic(Diagnostic.Create(
                            _s_templateNotImplementedRule, 
                            _typeSymbol.Locations[0], 
                            _typeSymbol.Name));
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsTemplateImplementationUpToDate(AttributeData generatedImplementationAttribute)
            {
                var hashValueConstant = ImmutableArrayExtensions.FirstOrDefault(
                    generatedImplementationAttribute.NamedArguments,
                    arg => arg.Key == nameof(GeneratedTemplateImplementationAttribute.Hash));

                if (hashValueConstant.Key != null)
                {
                    var generatedHash = (int)hashValueConstant.Value.Value;
                    var typeSyntaxRef =_context.Symbol.DeclaringSyntaxReferences.FirstOrDefault(
                        r => HasClassTemplateAttributeSyntax(
                            _context, 
                            _classTemplateAttributeTypeSymbol, 
                            (ClassDeclarationSyntax)r.GetSyntax()));

                    if (typeSyntaxRef != null)
                    {
                        var currentSyntaxText = typeSyntaxRef.GetSyntax().NormalizeWhitespace().ToFullString();
                        var currentHash = currentSyntaxText.GetHashCode();

                        return (generatedHash == currentHash);
                    }
                }

                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsGeneratedImplementation(out AttributeData attribute)
            {
                var attributeTypeSymbol = _context.Compilation.GetTypeByMetadataName(_s_generatedImplementationAttributeFullName);
                attribute = _typeSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass == attributeTypeSymbol);

                return (attribute.AttributeClass != null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsTemplateImplemented()
            {
                var patternTemplateInterfaceTypeSymbol = _context.Compilation.GetTypeByMetadataName(TemplateDiagnosticAnalyzer._s_patternTemplateInterfaceFullName);
                var isImplemented = _typeSymbol.Interfaces.Any(intf => intf == patternTemplateInterfaceTypeSymbol);
                return isImplemented;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool HasClassTemplateAttributeSyntax(
                SymbolAnalysisContext context,
                ITypeSymbol classTemplateAttributeTypeSymbol,
                ClassDeclarationSyntax classSyntax)
            {
                return classSyntax.HasAttributeSyntax(
                    classTemplateAttributeTypeSymbol, 
                    context.Compilation.GetSemanticModel(classSyntax.SyntaxTree));
            }
        }
    }
}
