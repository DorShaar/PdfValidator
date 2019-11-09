﻿using PdfValidator.Infrastracture;

namespace PdfValidator
{
    internal class LineParser : ILineParser
    {
        public ObjectData ParseLine(string line, long offset = 0)
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