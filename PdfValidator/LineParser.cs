namespace PdfValidator
{
    internal class LineParser : ILineParser
    {
        public ObjectData Parse(string line, ref long offset)
        {
            string[] lineParts = line.Split(' ');
            if (lineParts.Length < 3)
                return null;

            if (!lineParts[2].Equals("obj"))
                return null;

            if (!int.TryParse(lineParts[0], out int objectNumber))
                return null;

            if (!int.TryParse(lineParts[1], out int objectGeneration))
                return null;

            return new ObjectData(objectNumber, objectGeneration, 0);
        }
    }
}