using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoPatterns.DesignTime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PatternTemplateDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AutoPatternsTemplate";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly LocalizableString _s_title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _s_messageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _s_description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly string _s_category = "Naming";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DiagnosticDescriptor _s_rule = new DiagnosticDescriptor(DiagnosticId, _s_title, _s_messageFormat, _s_category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _s_description);
        private static readonly string _s_classTemplateAttributeFullName = typeof(MetaProgram.Annotation.ClassTemplateAttribute).FullName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_s_rule);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;
            var templateAttributeTypeSymbol = context.Compilation.GetTypeByMetadataName(_s_classTemplateAttributeFullName);
            var templateAttributeData = typeSymbol.GetAttributes().FirstOrDefault(attr =>attr.AttributeClass == templateAttributeTypeSymbol);

            if (templateAttributeData != null)
            {
                var diagnostic = Diagnostic.Create(_s_rule, typeSymbol.Locations[0], typeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
