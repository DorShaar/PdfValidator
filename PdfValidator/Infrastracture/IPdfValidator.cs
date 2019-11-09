using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfValidator.Infrastracture
{
    internal interface IPdfValidator
    {
        Task<ValidationResult> Validate(string filePath);
    }
}