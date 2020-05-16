namespace SampleBusinessLogic
{
    public interface IValidator
    {
        bool Validate(Request request);

        bool TryValidate(Request request, out bool result);
    }
}