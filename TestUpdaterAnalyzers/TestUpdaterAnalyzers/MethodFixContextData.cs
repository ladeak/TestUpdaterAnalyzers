using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace TestUpdaterAnalyzers
{
    public class MethodFixContextData
    {
        public Dictionary<string, Queue<ExpressionSyntax>> MockedExpectCalls { get; } = new Dictionary<string, Queue<ExpressionSyntax>>();
        private HashSet<string> VerifiedMocks { get; } = new HashSet<string>();

        public void Add(string identifier, ExpressionSyntax syntax)
        {
            MockedExpectCalls.TryAdd(identifier, new Queue<ExpressionSyntax>());
            if (MockedExpectCalls.TryGetValue(identifier, out var list))
            {
                list.Enqueue(syntax);
            }
        }

        public bool TakeFirst(string identifier, out ExpressionSyntax result)
        {
            if (MockedExpectCalls.TryGetValue(identifier, out var queue) && queue.Count > 0)
            {
                VerifiedMocks.Add(identifier);
                result = queue.Dequeue();
                return true;
            }
            result = null;
            return false;
        }

        public IEnumerable<ExpressionSyntax> TakeRest()
        {
            foreach (var identifier in VerifiedMocks)
                if (MockedExpectCalls.TryGetValue(identifier, out var queue))
                    foreach (var invocation in queue)
                        yield return invocation;
        }

        public HashSet<InvocationExpressionSyntax> RemovableExpressions { get; } = new HashSet<InvocationExpressionSyntax>();

        public SyntaxToken LambdaToken { get; set; } = SyntaxFactory.Identifier("a0");
    }
}
