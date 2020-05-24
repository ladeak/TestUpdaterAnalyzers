namespace SampleBusinessLogic
{
    public interface INameProvider
    {
        bool Initialized { get; }

        void Initialize();

        string GetFullName(string name);
    }
}
