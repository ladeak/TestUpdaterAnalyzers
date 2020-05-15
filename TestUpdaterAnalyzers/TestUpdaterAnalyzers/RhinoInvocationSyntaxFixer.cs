using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public class RhinoInvocationSyntaxFixer : CSharpSyntaxWalker
    {
        private SemanticModel _originalSemantics;
        private readonly IDocumentUpdater _documentUpdater;

        public RhinoInvocationSyntaxFixer(SemanticModel semanticModel, IDocumentUpdater documentUpdater)
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

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpr)
        {
            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpr != null)
            {
                var symbolInfo = _originalSemantics.GetSymbolInfo(memberAccessExpr);
                var memberSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.SingleOrDefault() as IMethodSymbol;
                if (memberSymbol != null)
                {
                    if (RhinoRecognizer.TestReturnMethod(memberSymbol))
                    {
                        _documentUpdater.UseReturns(memberAccessExpr, CancellationToken.None);
                    }
                    if (RhinoRecognizer.TestExpectMethod(memberSymbol))
                    {
                        _documentUpdater.DropExpectCall(memberAccessExpr, CancellationToken.None);
                    }
                    if (RhinoRecognizer.TestGenerateMockMethod(memberSymbol) || RhinoRecognizer.TestGenerateStubMethod(memberSymbol))
                    {
                        _documentUpdater.UseSubstituteFor(memberAccessExpr, CancellationToken.None);
                    }
                    if (RhinoRecognizer.TestThrowMethod(memberSymbol))
                    {
                        _documentUpdater.UseThrows(memberAccessExpr, CancellationToken.None);
                    }
                }
            }
            base.VisitInvocationExpression(invocationExpr);
        }
    }
}
