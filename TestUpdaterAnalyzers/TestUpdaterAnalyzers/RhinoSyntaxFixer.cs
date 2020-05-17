using Microsoft.CodeAnalysis;
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

        public async Task<Document> WalkAsync(SyntaxNode node, bool localScope)
        {
            var rewriter = new RhinoSyntaxRewriter(_semanticModel);
            var newRoot = rewriter.Rewrite(await _document.GetSyntaxRootAsync());
            _document = _document.WithSyntaxRoot(newRoot);

            var documentUpdater = new DocumentUpdater(_document);
            await documentUpdater.Start();
            documentUpdater.AddNSubstituteUsing();
            if (rewriter.UseExceptionExtensions)
                documentUpdater.AddNSubstituteExceptionExtensionsUsing();
            if (rewriter.UseReceivedExtensions)
                documentUpdater.AddNSubstituteReceivedExtensionsUsing();
            _document = documentUpdater.Complete();
            return _document;
        }
    }
}
