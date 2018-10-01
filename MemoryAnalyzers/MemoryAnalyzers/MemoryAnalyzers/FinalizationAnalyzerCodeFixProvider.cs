using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Text;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MemoryAnalyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FinalizationAnalyzerCodeFixProvider)), Shared]
    public class FinalizationAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Remove finalizer";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(FinalizationAnalyzer.DiagnosticId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<DestructorDeclarationSyntax>().First();
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => RemoveDestructorAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> RemoveDestructorAsync(Document document, DestructorDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            // Get the symbol representing the type to be renamed.
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.RemoveNode(declaration, SyntaxRemoveOptions.KeepEndOfLine);
            return document.WithSyntaxRoot(newRoot);
        }

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    }
}
