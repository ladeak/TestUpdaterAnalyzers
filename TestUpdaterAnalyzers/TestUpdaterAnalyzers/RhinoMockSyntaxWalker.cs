using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Threading;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public class RhinoMockSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private Document _document;

        public RhinoMockSyntaxWalker(SemanticModel semanticModel, Document document)
        {
            _semanticModel = semanticModel;
            _document = document;
        }

        public async Task<Document> WalkAsync(SyntaxNode node, bool localScope)
        {
            var documentUpdater = new DocumentUpdater(_document);
            var invocationWalker = new RhinoMockInvocationSyntaxWalker(_semanticModel, documentUpdater);


            var newDocument = await invocationWalker.WalkAsync(localScope ? node : await _document.GetSyntaxRootAsync());

            documentUpdater = new DocumentUpdater(newDocument);
            node = await newDocument.GetSyntaxRootAsync();
            var newSemantics = await newDocument.GetSemanticModelAsync();
            var arugmentsWalker = new RhinoMockArgumentsSyntaxWalker(newSemantics, documentUpdater);
            return await arugmentsWalker.WalkAsync(node);
        }
    }

    public class RhinoMockInvocationSyntaxWalker : CSharpSyntaxWalker
    {
        private SemanticModel _originalSemantics;
        private readonly IDocumentUpdater _documentUpdater;

        public RhinoMockInvocationSyntaxWalker(SemanticModel semanticModel, IDocumentUpdater documentUpdater)
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

                var memberSymbol = _originalSemantics.GetSymbolInfo(memberAccessExpr).Symbol as IMethodSymbol;
                if (memberSymbol != null)
                {

                    if (TestReturnMethod(memberSymbol))
                    {
                        _documentUpdater.UseReturns(memberAccessExpr, CancellationToken.None);
                    }
                    if (TestExpectMethod(memberSymbol))
                    {
                        _documentUpdater.DropExpectCall(memberAccessExpr, CancellationToken.None);
                    }
                    if (TestGenerateMockMethod(memberSymbol))
                    {
                        _documentUpdater.UseSubstituteFor(memberAccessExpr, CancellationToken.None);
                    }
                }
            }
            base.VisitInvocationExpression(invocationExpr);
        }

        private static bool TestReturnMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "Return", "IMethodOptions");

        private static bool TestGenerateMockMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "GenerateMock", "MockRepository");

        private static bool TestExpectMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "Expect", "RhinoMocksExtensions");

        private static bool TestSymbol(ISymbol symbolsType, string name, string type, string assembly = "Rhino.Mocks")
        {
            return symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }
    }

    public class RhinoMockArgumentsSyntaxWalker : CSharpSyntaxWalker
    {
        private SemanticModel _originalSemantics;
        private readonly IDocumentUpdater _documentUpdater;

        public RhinoMockArgumentsSyntaxWalker(SemanticModel semanticModel, IDocumentUpdater documentUpdater)
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
                    if (TestAnythingProperty(argumentSymbol))
                    {
                        var innerSymbol = _originalSemantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IPropertySymbol;
                        if (TestIsArgProperty(innerSymbol))
                        {
                            _documentUpdater.UseArgsAny(node, CancellationToken.None);
                        }
                    }
                }
            }
            base.VisitArgument(node);
        }

        private static bool TestIsArgProperty(IPropertySymbol propertySymbol) =>
            TestSymbol(propertySymbol, "Is", "Arg");

        private static bool TestAnythingProperty(IPropertySymbol propertySymbol) =>
            TestSymbol(propertySymbol, "Anything", "IsArg");

        private static bool TestSymbol(ISymbol symbolsType, string name, string type, string assembly = "Rhino.Mocks")
        {
            return symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }
    }
}
