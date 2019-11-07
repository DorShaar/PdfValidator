using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PdfValidator
{
    internal class FileParser : IFileParser
    {
        private readonly ILineParser mLineParser;

        public FileParser(ILineParser lineParser)
        {
            mLineParser = lineParser;
        }

        public async Task<List<ObjectData>> Parse(string filePath)
        {
            List<ObjectData> objects = new List<ObjectData>();
            long offset = 0;

            using (FileStream stream = File.OpenRead(filePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    ObjectData pdfObject = mLineParser.Parse(line, ref offset);
                    if (pdfObject != null)
                        objects.Add(pdfObject);
                }
            }

            return objects;
        }
    }
}