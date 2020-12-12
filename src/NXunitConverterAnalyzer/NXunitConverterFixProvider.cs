using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using NXunitConverterAnalyzer.Rewriters;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace NXunitConverterAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NXunitConverterFixProvider)), Shared]
    public class NXunitConverterFixProvider : CodeFixProvider
    {
        private const string title = "Convert to xUnit";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(NXunitConverterAnalyzer.DiagnosticId); }
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
                    equivalenceKey: NXunitConverterAnalyzer.DiagnosticId),
                diagnostic);
        }
    }
}
