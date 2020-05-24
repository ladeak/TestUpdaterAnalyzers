namespace SampleBusinessLogic
{
    public interface ILogger<T>
    {
        bool IsEnabled { get; set; }

        void Log(string message);
    }
}
