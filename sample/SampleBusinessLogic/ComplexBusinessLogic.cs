using System;
using System.Linq;

namespace SampleBusinessLogic
{
    public class ComplexBusinessLogic
    {
        private readonly IValidator _validator;
        private readonly INameProvider _nameProvider;
        private readonly ILogger<ComplexBusinessLogic> _logger;

        public ComplexBusinessLogic(IValidator validator, INameProvider nameProvider, ILogger<ComplexBusinessLogic> logger)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _nameProvider = nameProvider ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (!_nameProvider.Initialized)
                _nameProvider.Initialize();
            _logger.IsEnabled = true;
        }

        public int CalculateId(Request request)
        {
            if (!_validator.TryValidate(request, out var someValue) && someValue)
                throw new ArgumentException(nameof(request));

            var fullName = _nameProvider.GetFullName(request.Name);
            _logger.Log(fullName);

            return (int)(fullName.Count(x => x == 'e') * request.Age * request.Height);
        }
    }
}
