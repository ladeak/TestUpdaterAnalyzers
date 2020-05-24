using System;

namespace SampleBusinessLogic
{
    public class Logger<T> : ILogger<T>
    {
        private readonly string _prefix;

        public Logger() => _prefix = typeof(T).Name;

        public bool IsEnabled { get; set; }

        public void Log(string message)
        {
            if (IsEnabled)
                Console.WriteLine($"{_prefix}: message");
        }
    }
}
