using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public class RhinoSyntaxFixer
    {
        private SemanticModel _semanticModel;
        private Document _document;

        public RhinoSyntaxFixer(SemanticModel semanticModel, Document document)
        {
            _semanticModel = semanticModel;
            _document = document;
        }

        public async Task<Document> WalkAsync(TextSpan diagnosticSpan, bool localScope)
        {
            var root = await _document.GetSyntaxRootAsync();
            SyntaxNode node = root.FindNode(diagnosticSpan);

            var nodeScope = localScope ? node : root;

            var rewriter = new RhinoSyntaxRewriter(_semanticModel);
            var newNode = rewriter.Rewrite(nodeScope);

            if (localScope)
                newNode = root.ReplaceNode(node, newNode);

            _document = _document.WithSyntaxRoot(newNode);

            var documentUpdater = new DocumentUpdater(_document);
            await documentUpdater.Start();
            documentUpdater.AddNSubstituteUsing();
            if (rewriter.UseExceptionExtensions)
                documentUpdater.AddNSubstituteExceptionExtensionsUsing();
            if (rewriter.UseReceivedExtensions)
                documentUpdater.AddNSubstituteReceivedExtensionsUsing();
            if (!localScope)
                documentUpdater.RemoveRhinoMocksUsing();
            _document = documentUpdater.Complete();

            return _document;
        }

    }
}
