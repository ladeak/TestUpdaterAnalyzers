namespace SampleBusinessLogic
{
    public interface IValidator
    {
        bool Validate(Request request);

        void TryValidate(Request request, out bool result);
    }
}