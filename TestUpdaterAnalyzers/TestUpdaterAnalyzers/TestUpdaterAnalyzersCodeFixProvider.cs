using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TestUpdaterAnalyzersCodeFixProvider)), Shared]
    public class TestUpdaterAnalyzersCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Change to NSubstitute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TestUpdaterAnalyzersAnalyzer.RhinoUsageId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (!context.Document.Project.MetadataReferences.Any(x => x.Display.Contains("NSubstitute.dll")))
                return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            SyntaxNode parentNode = root.FindNode(diagnosticSpan);
            bool localScope = true;
            if (diagnostic.Properties.TryGetValue("localscope", out var scope))
                if (bool.TryParse(scope, out bool parsedScope))
                    localScope = parsedScope;

            context.RegisterCodeFix(
              CodeAction.Create(title: Title, createChangedDocument: async c =>
              {
                  var walker = new RhinoMockSyntaxWalker(await context.Document.GetSemanticModelAsync(), context.Document);
                  return await walker.WalkAsync(parentNode, localScope);
              }, equivalenceKey: diagnostic.Id),
              diagnostic);
        }
    }
}
