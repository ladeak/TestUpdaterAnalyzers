using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace TestUpdaterAnalyzers
{
    public class MethodFixContextData
    {
        public Dictionary<string, Queue<InvocationExpressionSyntax>> MockedExpectCalls { get; } = new Dictionary<string, Queue<InvocationExpressionSyntax>>();
        private HashSet<string> VerifiedMocks { get; } = new HashSet<string>();

        public void Add(string identifier, InvocationExpressionSyntax syntax)
        {
            MockedExpectCalls.TryAdd(identifier, new Queue<InvocationExpressionSyntax>());
            if (MockedExpectCalls.TryGetValue(identifier, out var list))
            {
                list.Enqueue(syntax);
            }
        }

        public bool TakeFirst(string identifier, out InvocationExpressionSyntax result)
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

        public IEnumerable<InvocationExpressionSyntax> TakeRest()
        {
            foreach (var identifier in VerifiedMocks)
                if (MockedExpectCalls.TryGetValue(identifier, out var queue))
                    foreach (var invocation in queue)
                        yield return invocation;
        }
    }
}
