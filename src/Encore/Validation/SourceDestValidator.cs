

using Storage;

namespace Encore.Validation
{
    public class SourceDestValidator
    {
        public SourceDestValidator(){}

        public ValidationResult IsSourceDestValid(string source, string dest)
        {
            if (string.Equals(source, dest, StringComparison.OrdinalIgnoreCase))
                return ValidationResult.InvalidResult($"The destination:{dest} is the same as the source:{source}");

            foreach (var file_string in FileCompareHelper.GetAllFiles(source))
                if (!FileCompareHelper.IsFileLocked(new FileInfo(file_string)))
                    return ValidationResult.InvalidResult($"The file:{file_string} is opened in another process.");

            foreach (var file_string in FileCompareHelper.GetAllFiles(dest))
                if (!FileCompareHelper.IsFileLocked(new FileInfo(file_string)))
                    return ValidationResult.InvalidResult($"The file:{file_string} is opened in another process.");

            return ValidationResult.ValidResult();
        }

        
    }
}
