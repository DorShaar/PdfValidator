using System;

namespace PdfValidator
{
    internal class LineParserSpans : ILineParser
    {
        public ObjectData Parse(string line, ref long offset)
        {
            ReadOnlySpan<char> span = line.AsSpan();

            return Parse(span, ref offset);
        }

        public static ObjectData Parse(ReadOnlySpan<char> span, ref long offset)
        {
            var scanned = -1;
            var position = 0;

            string objectNumberStr = ParseChunk(ref span, ref scanned, ref position);
            string objectGenerationStr = ParseChunk(ref span, ref scanned, ref position);
            string objectStr = ParseChunk(ref span, ref scanned, ref position);

            if (!int.TryParse(objectNumberStr, out int objectNumber))
                return null;

            if (!int.TryParse(objectGenerationStr, out int objectGeneration))
                return null;

            if (objectStr == null || !objectStr.Equals("obj"))
                return null;

            return new ObjectData(objectNumber, objectGeneration, offset);
        }

        private static string ParseChunk(ref ReadOnlySpan<char> span, ref int scanned, ref int position)
        {
            scanned += position + 1;

            if (span.Length < scanned)
                return null;

            position = span.Slice(scanned, span.Length - scanned).IndexOf(' ');
            if (position < 0)
            {
                position = span.Slice(scanned, span.Length - scanned).Length;
            }

            return span.Slice(scanned, position).ToString();
        }
    }
}