using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestUpdaterAnalyzers
{
    public class RhinoSyntaxFinder : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semantics;
        private readonly Action<SyntaxNode, bool> _action;

        private bool _localScope;
        private SyntaxNode _targetNode;

        public RhinoSyntaxFinder(SemanticModel semanticModel, Action<SyntaxNode, bool> action)
        {
            _semantics = semanticModel;
            _action = action;
        }

        public void Find(SyntaxNode node)
        {
            try
            {
                _targetNode = null;
                _localScope = true;
                Visit(node);
                if (_targetNode != null)
                    _action(_targetNode, _localScope);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        private void SetTargetNode(SyntaxNode node)
        {
            if (_targetNode == null)
                _targetNode = node;
        }


        public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpr)
        {
            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpr != null)
            {
                var symbolInfo = _semantics.GetSymbolInfo(memberAccessExpr);
                var memberSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
                if (memberSymbol != null)
                {
                    if (RhinoRecognizer.TestReturnMethod(memberSymbol))
                    {
                        SetTargetNode(memberAccessExpr);
                    }
                    else if (RhinoRecognizer.TestExpectMethod(memberSymbol))
                    {
                        SetTargetNode(memberAccessExpr);
                        if (memberAccessExpr.Expression is IdentifierNameSyntax identifier)
                        {
                            var fieldOrProperty = _semantics.GetSymbolInfo(identifier).Symbol;
                            if (fieldOrProperty is IFieldSymbol || fieldOrProperty is IPropertySymbol)
                                _localScope = false;
                        }
                    }
                    else if (RhinoRecognizer.TestGenerateMockMethod(memberSymbol))
                    {
                        SetTargetNode(memberAccessExpr);
                    }
                    else if (RhinoRecognizer.TestGenerateStubMethod(memberSymbol))
                    {
                        SetTargetNode(memberAccessExpr);
                    }
                    else if (RhinoRecognizer.TestThrowMethod(memberSymbol))
                    {
                        SetTargetNode(memberAccessExpr);
                    }
                }
            }
            base.VisitInvocationExpression(invocationExpr);
        }

        public override void VisitArgument(ArgumentSyntax node)
        {
            var argumentExpr = node.Expression as MemberAccessExpressionSyntax;
            if (argumentExpr != null)
            {
                var argumentSymbol = _semantics.GetSymbolInfo(argumentExpr).Symbol as IPropertySymbol;
                if (argumentSymbol != null)
                {
                    if (RhinoRecognizer.TestAnythingProperty(argumentSymbol))
                    {
                        var innerSymbol = _semantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IPropertySymbol;
                        if (RhinoRecognizer.TestIsArgProperty(innerSymbol))
                        {
                            SetTargetNode(argumentExpr);
                        }
                    }
                }
            }
            base.VisitArgument(node);
        }

    }
}
