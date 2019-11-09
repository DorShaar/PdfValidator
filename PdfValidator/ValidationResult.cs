namespace PdfValidator
{
    internal class ValidationResult
    {
        public string ErrorsText { get; }
        public bool Result { get; }

        public ValidationResult(bool validationResult, string errorsText = "")
        {
            Result = validationResult;
            ErrorsText = errorsText;
        }
    }
}