using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public class RhinoSyntaxFixer
    {
        private readonly SemanticModel _semanticModel;
        private Document _document;

        public RhinoSyntaxFixer(SemanticModel semanticModel, Document document)
        {
            _semanticModel = semanticModel;
            _document = document;
        }

        public async Task<Document> WalkAsync(SyntaxNode node, bool localScope)
        {
            var documentUpdater = new DocumentUpdater(_document);
            var invocationWalker = new RhinoInvocationSyntaxFixer(_semanticModel, documentUpdater);


            var newDocument = await invocationWalker.WalkAsync(localScope ? node : await _document.GetSyntaxRootAsync());

            documentUpdater = new DocumentUpdater(newDocument);
            node = await newDocument.GetSyntaxRootAsync();
            var newSemantics = await newDocument.GetSemanticModelAsync();
            var arugmentsWalker = new RhinoArgumentsSyntaxFixer(newSemantics, documentUpdater);
            return await arugmentsWalker.WalkAsync(node);
        }
    }
}
