using System;

namespace SampleBusinessLogic
{
    public class Validator : IValidator
    {
        public void TryValidate(Request request, out bool result)
        {
            if (request == null)
                result = false;
            else
                result = !string.IsNullOrWhiteSpace(request.Name)
                               && request.Age > 0
                               && request.Age < 120
                               && request.Height > 0
                               && request.Height < 250;
        }

        public bool Validate(Request request)
        {
            if (request == null)
                throw new ArgumentException(nameof(request));
            return !string.IsNullOrWhiteSpace(request.Name)
                && request.Age > 0
                && request.Age < 120
                && request.Height > 0
                && request.Height < 250;
        }
    }
}
