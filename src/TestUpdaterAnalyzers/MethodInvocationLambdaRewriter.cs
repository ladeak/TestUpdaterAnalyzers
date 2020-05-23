using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestUpdaterAnalyzers
{
    public class MethodInvocationLambdaRewriter : CSharpSyntaxRewriter
    {
        private readonly SyntaxToken _parameterIdentifier;
        private readonly SyntaxToken _newParamIdentifier;

        public ExpressionSyntax? ReturnStatement { get; set; }
        private bool _keepStatement;

        public MethodInvocationLambdaRewriter(ParameterSyntax parameter, SyntaxToken newParamIdentifier)
        {
            _parameterIdentifier = parameter.Identifier;
            _newParamIdentifier = newParamIdentifier;
        }

        public bool Rewrite(SyntaxNode node, out SyntaxNode result)
        {
            _keepStatement = true;
            result = Visit(node);
            return _keepStatement;
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var s = base.VisitMemberAccessExpression(node);
            if (s is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression.IsKind(SyntaxKind.IdentifierName)
                    && memberAccess.Expression is IdentifierNameSyntax identifier
                    && identifier.Identifier.ValueText == _parameterIdentifier.ValueText)
                {
                    if (memberAccess.Name.Identifier.ValueText == "Arguments")
                    {
                        return SyntaxFactory.IdentifierName(_newParamIdentifier);
                    }

                    if (memberAccess.Name.Identifier.ValueText == "ReturnValue" && memberAccess.Parent is AssignmentExpressionSyntax returnValueAssignment)
                    {
                        ReturnStatement = returnValueAssignment.Right;
                        _keepStatement = false;
                    }
                }
            }

            return s;
        }
    }
}
