namespace SampleBusinessLogic
{
    public class NameProvider : INameProvider
    {
        public bool Initialized { get; private set; }

        public string GetFullName(string name) => $"Fullname: {name} {name}";

        public void Initialize() => Initialized = true;
    }
}
