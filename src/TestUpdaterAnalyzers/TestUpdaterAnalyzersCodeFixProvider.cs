using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TestUpdaterAnalyzersCodeFixProvider)), Shared]
    public class TestUpdaterAnalyzersCodeFixProvider : CodeFixProvider
    {
        private const string ChangeToNSubstitute = "Convert to NSubstitute";
        private const string ChangeToNSubstituteInDoc = "Convert to NSubstitute In Document";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TestUpdaterAnalyzersAnalyzer.RhinoUsageId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (!context.Document.Project.MetadataReferences.Any(x => x.Display.Contains("NSubstitute.dll")))
                return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            bool localScope = true;
            if (diagnostic.Properties.TryGetValue("localscope", out var scope))
                if (bool.TryParse(scope, out bool parsedScope))
                    localScope = parsedScope;

            context.RegisterCodeFix(
              CodeAction.Create(title: ChangeToNSubstitute, createChangedDocument: async c =>
              {
                  var walker = new RhinoSyntaxFixer(await context.Document.GetSemanticModelAsync(), context.Document);
                  return await walker.WalkAsync(diagnosticSpan, localScope);
              }, equivalenceKey: diagnostic.Id),
              diagnostic);

            if (localScope)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(title: ChangeToNSubstituteInDoc, createChangedDocument: async c =>
                    {
                        var walker = new RhinoSyntaxFixer(await context.Document.GetSemanticModelAsync(), context.Document);
                        return await walker.WalkAsync(diagnosticSpan, false);
                    }, equivalenceKey: diagnostic.Id + "-global"),
                    diagnostic);
            }
        }
    }
}
