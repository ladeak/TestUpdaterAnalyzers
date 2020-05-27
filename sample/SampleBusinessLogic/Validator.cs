using System;
using System.Threading;

namespace SampleBusinessLogic
{
    public class Validator : IValidator
    {
        public bool IsEmptyNameValid { get; set; }

        public void Run()
        {
            Thread.Yield();
        }

        public bool TryValidate(Request request, out bool result)
        {
            if (request == null)
                result = false;
            else
                result = (IsEmptyNameValid ? request.Name != null : !string.IsNullOrWhiteSpace(request.Name))
                               && request.Age > 0
                               && request.Age < 120
                               && request.Height > 0
                               && request.Height < 250;

            return result;
        }

        public bool Validate(Request request)
        {
            if (request == null)
                throw new ArgumentException(nameof(request));
            return (IsEmptyNameValid ? request.Name != null : !string.IsNullOrWhiteSpace(request.Name))
                && request.Age > 0
                && request.Age < 120
                && request.Height > 0
                && request.Height < 250;
        }
    }
}
