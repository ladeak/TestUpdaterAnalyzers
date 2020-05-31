using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Rename;

namespace ConvertNxUnitAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConvertNxUnitCodeFixProvider)), Shared]
    public class ConvertNxUnitCodeFixProvider : CodeFixProvider
    {
        private const string title = "Convert to xUnit";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ConvertNxUnitAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: async c =>
                    {
                        var rewriter = new XunitRewriter();
                        return await rewriter.UpdateToXUnitAsync(context.Document, await context.Document.GetSemanticModelAsync(), diagnosticSpan, c);
                    },
                    equivalenceKey: ConvertNxUnitAnalyzer.DiagnosticId),
                diagnostic);
        }
    }
}
