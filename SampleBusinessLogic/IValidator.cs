namespace SampleBusinessLogic
{
    public interface IValidator
    {
        bool IsEmptyNameValid { get; set; }

        bool Validate(Request request);

        bool TryValidate(Request request, out bool result);
    }
}