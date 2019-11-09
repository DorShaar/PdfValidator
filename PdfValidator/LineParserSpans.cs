using PdfValidator.Infrastracture;
using System;

namespace PdfValidator
{
    internal class LineParserSpans : ILineParser
    {
        public ObjectData ParseLine(string line, long offset = 0)
        {
            ReadOnlySpan<char> span = line.AsSpan();

            return ParseLine(span, offset).ObjectData;
        }

        public static ParsedLine ParseLine(ReadOnlySpan<char> span, long offset)
        {
            int scanned = -1;
            int position = 0;
            char separator = ' ';

            // In case of DataObject.
            ReadOnlySpan<char> objectNumberSpan = ParseChunk(ref span, ref scanned, ref position, separator);
            if (int.TryParse(objectNumberSpan, out int objectNumber))
            {
                ReadOnlySpan<char> objectGenerationSpan = ParseChunk(ref span, ref scanned, ref position, separator);
                if (!int.TryParse(objectGenerationSpan, out int objectGeneration))
                    return null;

                ReadOnlySpan<char> objectSpan = ParseChunk(ref span, ref scanned, ref position, separator);
                if (objectSpan == null || !objectSpan.Equals("obj"))
                    return null;

                return new ParsedLine(
                    offset,
                    ProcessMode.Regular,
                    new ObjectData(objectNumber, objectGeneration, offset));
            }


            if (objectNumberSpan.ToString().Equals("xref"))
                return new ParsedLine(offset, ProcessMode.InsideXref);

            return new ParsedLine(offset, ProcessMode.Regular);
        }

        private static ReadOnlySpan<char> ParseChunk(
            ref ReadOnlySpan<char> span, ref int scanned, ref int position, char separator)
        {
            scanned += position + 1;

            if (span.Length < scanned)
                return null;

            position = span.Slice(scanned, span.Length - scanned).IndexOf(separator);
            if (position < 0)
            {
                position = span.Slice(scanned, span.Length - scanned).Length;
            }

            return span.Slice(scanned, position);
        }
    }
}