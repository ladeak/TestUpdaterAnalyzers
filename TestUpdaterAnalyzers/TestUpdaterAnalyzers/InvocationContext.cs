using System;

namespace TestUpdaterAnalyzers
{
    public class SyntaxWalkContext<T> where T : new()
    {
        public T Data { get; private set; } = new T();

        public int Level { get; private set; } = 0;

        public int Enter() => ++Level;

        public int Exit()
        {
            if (--Level == 0)
                Clean();
            return 0;
        }

        private void Clean() => Data = new T();
    }
}