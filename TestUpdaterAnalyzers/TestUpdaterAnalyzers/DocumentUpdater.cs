using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public class DocumentUpdater : IDocumentUpdater
    {
        private DocumentEditor _editor;
        private readonly Document _originalDoc;

        public DocumentUpdater(Document doc)
        {
            _originalDoc = doc;
        }

        public async Task Start(CancellationToken token = default)
        {
            _editor = await DocumentEditor.CreateAsync(_originalDoc, token);
        }

        public Document Complete() => _editor.GetChangedDocument();

        private TextSpan GetFromVariableDeclaration(SyntaxNode node)
        {
            while (true)
            {
                if (node is MethodDeclarationSyntax methodDeclaration)
                    return methodDeclaration.FullSpan;
                node = node.Parent;
            }
        }

        public async Task UseReturns(SyntaxNode parentNode, CancellationToken cancellationToken)
        {
            if (parentNode is MemberAccessExpressionSyntax memberAccess)
            {
                _editor.ReplaceNode(memberAccess.Name, SyntaxFactory.IdentifierName("Returns"));
            }

            AddNSubstituteUsings();
        }

        public async Task UseArgsAny(ArgumentSyntax argument, CancellationToken cancellationToken)
        {
            if (argument.Expression is MemberAccessExpressionSyntax anyProperty)
            {
                if (anyProperty.Name.ToString() == "Anything" && anyProperty.Expression is MemberAccessExpressionSyntax isProperty)
                {
                    if (isProperty.Expression is GenericNameSyntax argGenericArgument)
                    {
                        var typeArguments = argGenericArgument.TypeArgumentList;
                        var newArg = SyntaxFactory.Argument(
                            SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Arg"),
                            SyntaxFactory.GenericName(SyntaxFactory.Identifier("Any"), typeArguments))));
                        _editor.ReplaceNode(argument, newArg);
                        AddNSubstituteUsings();
                    }
                }
            }
        }

        public async Task UseSubstituteFor(SyntaxNode parentNode, CancellationToken cancellationToken)
        {
            if (parentNode is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is GenericNameSyntax generateMockIdentifier)
                {
                    var typeArguments = generateMockIdentifier.TypeArgumentList;

                    var newInvocation = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Substitute"),
                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("For"), typeArguments)));

                    _editor.ReplaceNode(parentNode.Parent, newInvocation);
                }
            }
            AddNSubstituteUsings();
        }

        public async Task DropExpectCall(SyntaxNode parentNode, CancellationToken cancellationToken)
        {
            var mockedObjectIdentifier = (parentNode as MemberAccessExpressionSyntax).Expression as IdentifierNameSyntax;

            var expectInvocationExpression = parentNode.Parent as InvocationExpressionSyntax;
            if (expectInvocationExpression == null)
                return;
            var argumentLambda = expectInvocationExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression as LambdaExpressionSyntax;
            var mockMethodInvocation = argumentLambda.Body as InvocationExpressionSyntax;
            if (!(mockMethodInvocation?.Expression is MemberAccessExpressionSyntax mockedMethod))
                return;

            var invocation = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                mockedObjectIdentifier, mockedMethod.Name), mockMethodInvocation.ArgumentList);

            _editor.ReplaceNode(expectInvocationExpression, invocation);
            AddNSubstituteUsings();
        }

        public void AddNSubstituteUsings()
        {
            CompilationUnitSyntax root = _editor.GetChangedRoot() as CompilationUnitSyntax;
            if (!root.Usings.Any(x => x.Name.GetText().ToString() == "NSubstitute"))
            {
                var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("NSubstitute"));
                _editor.InsertBefore((_editor.OriginalRoot as CompilationUnitSyntax).Usings.FirstOrDefault(), newUsing);
            }
        }
    }
}
