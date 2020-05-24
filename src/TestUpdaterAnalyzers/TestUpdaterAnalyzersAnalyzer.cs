using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace TestUpdaterAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestUpdaterAnalyzersAnalyzer : DiagnosticAnalyzer
    {
        public const string RhinoUsageId = nameof(RhinoUsageId);

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "RhinoMocks";

        private static DiagnosticDescriptor RhinoUsageRule = new DiagnosticDescriptor(RhinoUsageId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(RhinoUsageRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeMethods, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethods(SyntaxNodeAnalysisContext context)
        {
            var methodSyntax = (MethodDeclarationSyntax)context.Node;
            var finder = new RhinoSyntaxFinder(context.SemanticModel, (node, localScope) =>
            {
                var diagnostic = Diagnostic.Create(RhinoUsageRule, methodSyntax.GetLocation(), ImmutableDictionary<string, string>.Empty.Add("localscope", localScope.ToString()), methodSyntax.Identifier.ValueText);
                context.ReportDiagnostic(diagnostic);
            });
            finder.Find(methodSyntax);
        }
    }
}
