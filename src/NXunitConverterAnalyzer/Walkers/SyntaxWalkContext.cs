using Microsoft.CodeAnalysis;
using System;

namespace NXunitConverterAnalyzer.Walkers
{
    public class SyntaxWalkContext<T> : IDisposable where T : new()
    {
        public T Current { get; private set; } = new T();

        public int Level { get; private set; } = 0;

        public void Dispose() => Exit();

        public SyntaxWalkContext<T> Enter()
        {
            ++Level;
            return this;
        }

        public int Exit()
        {
            if (--Level == 0)
                Clean();
            return 0;
        }

        private void Clean() => Current = new T();
    }

    public class SyntaxWalkContext<T, TExp> : IDisposable
        where T : new()
        where TExp : SyntaxNode
    {
        private Func<TExp, T> _initialize;

        public SyntaxWalkContext(Func<TExp, T> initialize)
        {
            _initialize = initialize;
        }

        public T Current { get; private set; }

        public int Level { get; private set; } = 0;

        public void Dispose() => Exit();

        public SyntaxWalkContext<T, TExp> Enter(TExp expression)
        {
            if (Level == 0)
                Current = _initialize(expression);
            ++Level;
            return this;
        }

        public int Exit()
        {
            if (--Level == 0)
                Clean();
            return 0;
        }

        private void Clean() => Current = new T();
    }
}