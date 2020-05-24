using System;

namespace SampleBusinessLogic
{
    public class Logger<T> : ILogger<T>
    {
        private readonly string _prefix;

        public Logger()
        {
            _prefix = typeof(T).Name;
        }

        public void Log(string message) => Console.WriteLine($"{_prefix}: message");
    }
}
