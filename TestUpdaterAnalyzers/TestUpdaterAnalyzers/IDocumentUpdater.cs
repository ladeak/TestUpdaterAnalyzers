using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public interface IDocumentUpdater
    {
        Document Complete();
        Task DropExpectCall(SyntaxNode node, CancellationToken cancellationToken);
        Task Start(CancellationToken token = default);
        Task UseArgsAny(ArgumentSyntax node, CancellationToken cancellationToken);
        Task UseReturns(SyntaxNode node, CancellationToken cancellationToken);
        Task UseSubstituteFor(SyntaxNode node, CancellationToken cancellationToken);
    }
}