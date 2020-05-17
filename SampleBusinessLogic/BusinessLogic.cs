using System;

namespace SampleBusinessLogic
{
    public class BusinessLogic
    {
        private readonly IValidator _validator;

        public BusinessLogic(IValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public int CalculateId(Request request)
        {
            if (!_validator.Validate(request))
                throw new ArgumentException(nameof(request));
            return (int)(request.Name.Length * request.Age + request.Height);
        }

        public int TryCalculateId(Request request)
        {
            if (!_validator.TryValidate(request, out var someValue) && someValue)
                throw new ArgumentException(nameof(request));
            return (int)(request.Name.Length * request.Age + request.Height);
        }

        public bool IsEmptyNameAllowed()
        {
            return _validator.IsEmptyNameValid;
        }
    }
}
