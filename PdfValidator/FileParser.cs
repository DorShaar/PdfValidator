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

            using (FileStream stream = File.OpenRead(filePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    i++;
                    if(i == 62213)
                    {
                        int x = 3;
                    }

                    ObjectData pdfObject = mLineParser.Parse(line);
                    if (pdfObject != null)
                        objects.Add(pdfObject);
                }
            }

            return objects;
        }
    }
}