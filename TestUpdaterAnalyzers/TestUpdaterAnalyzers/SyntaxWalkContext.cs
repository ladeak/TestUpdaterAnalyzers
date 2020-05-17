using System;

namespace TestUpdaterAnalyzers
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
}