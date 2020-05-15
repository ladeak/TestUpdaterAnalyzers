using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public class RhinoArgumentsSyntaxFixer : CSharpSyntaxWalker
    {
        private SemanticModel _originalSemantics;
        private readonly IDocumentUpdater _documentUpdater;

        public RhinoArgumentsSyntaxFixer(SemanticModel semanticModel, IDocumentUpdater documentUpdater)
        {
            _originalSemantics = semanticModel;
            _documentUpdater = documentUpdater;
        }

        public async Task<Document> WalkAsync(SyntaxNode node)
        {
            await _documentUpdater.Start();
            Visit(node);
            return _documentUpdater.Complete();
        }

        public override void VisitArgument(ArgumentSyntax node)
        {
            var argumentExpr = node.Expression as MemberAccessExpressionSyntax;
            if (argumentExpr != null)
            {
                var argumentSymbol = _originalSemantics.GetSymbolInfo(argumentExpr).Symbol as IPropertySymbol;
                if (argumentSymbol != null)
                {
                    if (RhinoRecognizer.TestAnythingProperty(argumentSymbol))
                    {
                        var innerSymbol = _originalSemantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IPropertySymbol;
                        if (RhinoRecognizer.TestIsArgProperty(innerSymbol))
                        {
                            _documentUpdater.UseArgsAny(node, CancellationToken.None);
                        }
                    }
                }
            }
            base.VisitArgument(node);
        }

    }
}
