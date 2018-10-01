using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MemoryAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FinalizationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MemoryAnalyzersFinalization";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FinzalizationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.FinalizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.Culture), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.DestructorDeclaration);
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var method = (IMethodSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (method == null)
                return;
            var methodSyntax = (DestructorDeclarationSyntax)context.Node;
            var bodyExpression = (BlockSyntax)methodSyntax.Body;;
            if (!bodyExpression.Statements.Any())
            {
                var diagnostic = Diagnostic.Create(Rule, method.Locations[0], method.ContainingType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
