

namespace Encore.Validation
{
    public class SourceDestValidator
    {
        public SourceDestValidator()
        {

        }

        public ValidationResult IsSourceDestValid(string source, string dest)
        {
            if (string.Equals(source, dest, StringComparison.OrdinalIgnoreCase))
                return ValidationResult.InvalidResult($"The destination:{dest} is the same as the source:{source}");
            return ValidationResult.ValidResult();
        }

        
    }
}
