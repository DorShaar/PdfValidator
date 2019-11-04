using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfValidator
{
    internal interface IFileParser
    {
        Task<List<ObjectData>> Parse(string filePath);
    }
}