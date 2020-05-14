using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace TestUpdaterAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestUpdaterAnalyzersAnalyzer : DiagnosticAnalyzer
    {
        public const string RhinoUsageId = nameof(RhinoUsageId);

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "RhinoMocks";

        private static DiagnosticDescriptor RhinoUsageRule = new DiagnosticDescriptor(RhinoUsageId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(RhinoUsageRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMethods, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethods(SyntaxNodeAnalysisContext context)
        {
            var methodSyntax = (MethodDeclarationSyntax)context.Node;
            var finder = new RhinoMockSyntaxFinder(context.SemanticModel, (node, localScope) =>
            {
                var diagnostic = Diagnostic.Create(RhinoUsageRule, methodSyntax.GetLocation(), ImmutableDictionary<string, string>.Empty.Add("localscope", localScope.ToString()), methodSyntax.Identifier.ValueText);
                context.ReportDiagnostic(diagnostic);
            });
            finder.Find(methodSyntax);
        }
    }

    public class RhinoMockSyntaxFinder : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semantics;
        private readonly Action<SyntaxNode, bool> _action;

        private bool _localScope;
        private SyntaxNode _targetNode;

        public RhinoMockSyntaxFinder(SemanticModel semanticModel, Action<SyntaxNode, bool> action)
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
                var memberSymbol = _semantics.GetSymbolInfo(memberAccessExpr).Symbol as IMethodSymbol;
                if (memberSymbol != null)
                {

                    if (TestReturnMethod(memberSymbol))
                    {
                        SetTargetNode(memberAccessExpr);
                    }
                    else if (TestExpectMethod(memberSymbol))
                    {
                        SetTargetNode(memberAccessExpr);
                        if (memberAccessExpr.Expression is IdentifierNameSyntax identifier)
                        {
                            var fieldOrProperty = _semantics.GetSymbolInfo(identifier).Symbol;
                            if (fieldOrProperty is IFieldSymbol || fieldOrProperty is IPropertySymbol)
                                _localScope = false;
                        }
                    }
                    else if (TestGenerateMockMethod(memberSymbol))
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
                    if (TestAnythingProperty(argumentSymbol))
                    {
                        var innerSymbol = _semantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IPropertySymbol;
                        if (TestIsArgProperty(innerSymbol))
                        {
                            SetTargetNode(argumentExpr);
                        }
                    }
                }
            }
            base.VisitArgument(node);
        }

        private static bool TestReturnMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "Return", "IMethodOptions");

        private static bool TestGenerateMockMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "GenerateMock", "MockRepository");

        private static bool TestExpectMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "Expect", "RhinoMocksExtensions");

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
