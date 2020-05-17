using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public class IgnoreArgumentsFinder : CSharpSyntaxWalker
    {
        private SemanticModel _originalSemantics;
        private SyntaxNode _innerChild;

        public IgnoreArgumentsFinder(SemanticModel semanticModel)
        {
            _originalSemantics = semanticModel;
        }

        public async Task<SyntaxNode> WalkAsync(SyntaxNode node)
        {
            Visit(node);
            return _innerChild;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpr)
        {
            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpr != null)
            {
                var symbolInfo = _originalSemantics.GetSymbolInfo(memberAccessExpr);
                var memberSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.SingleOrDefault() as IMethodSymbol;
                if (memberSymbol != null)
                {
                    if (RhinoRecognizer.IsIgnoreArgumentsMethod(memberSymbol))
                    {
                        _innerChild = memberAccessExpr;
                        return;
                    }
                }
            }
            base.VisitInvocationExpression(invocationExpr);
        }
    }
}
