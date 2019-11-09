using PdfValidator.Infrastracture;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PdfValidator
{
    internal class FileParser : IPdfValidator
    {
        private readonly ILineParser mLineParser;

        public FileParser(ILineParser lineParser)
        {
            mLineParser = lineParser;
        }

        public async Task<ValidationResult> Validate(string filePath)
        {
            List<ObjectData> objects = new List<ObjectData>();

            using (FileStream stream = File.OpenRead(filePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    ObjectData pdfObject = mLineParser.ParseLine(line);
                    if (pdfObject != null)
                        objects.Add(pdfObject);
                }
            }

            // TODO Get Xref table and validate.
            return new ValidationResult(false, "bla bla");
        }
    }
}