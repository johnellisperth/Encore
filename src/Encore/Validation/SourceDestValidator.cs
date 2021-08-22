using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveDuplicateFiles.Validation
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

    public class ValidationResult
    {
        public string Message { get; protected set; }
        public bool IsValid { get; protected set; }

        public static ValidationResult InvalidResult(string message) => new ValidationResult() { Message = message, IsValid = false };

        public static ValidationResult ValidResult(string message ="") => new ValidationResult() { Message = message, IsValid = true };
    }
}
