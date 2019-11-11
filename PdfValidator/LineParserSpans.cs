using PdfValidator.Infrastracture;
using PdfValidator.ParsedLine;
using PdfValidator.ParsedLines;
using System;

namespace PdfValidator
{
    internal class LineParserSpans : ILineParser
    {
        public ObjectData ParseLine(string line, long offset = 0)
        {
            return (ParseLine(line.AsSpan(), offset, ParseMode.Regular) as ObjectDataParsedLine)?.ObjectData;
        }

        public static IParsedLine ParseLine(ReadOnlySpan<char> span, long offset, ParseMode processMode)
        {
            char separator = ' ';

            if (processMode == ParseMode.Regular)
                return ParseLineRegularMode(span, offset, separator);

            if (processMode == ParseMode.ExpectingXRefTableHeader)
                return ParseLineXrefTableHeaderMode(span, offset, separator);

            if (processMode == ParseMode.InsideXref)
                return ParseLineXrefMode(span, offset, separator);

            return null;
        }

        private static IParsedLine ParseLineRegularMode(ReadOnlySpan<char> span, long offset, char separator)
        {
            int scanned = -1;
            int position = 0;

            ReadOnlySpan<char> objectNumberSpan = ParseChunk(ref span, ref scanned, ref position, separator);
            if (int.TryParse(objectNumberSpan, out int objectNumber))
            {
                ReadOnlySpan<char> objectGenerationSpan = ParseChunk(ref span, ref scanned, ref position, separator);
                if (!int.TryParse(objectGenerationSpan, out int objectGeneration))
                    return null;

                ReadOnlySpan<char> objectSpan = ParseChunk(ref span, ref scanned, ref position, separator);
                if (objectSpan == null || !objectSpan.ToString().Contains("obj"))
                    return null;

                return new ObjectDataParsedLine(
                    offset,
                    ParseMode.Regular,
                    new ObjectData(objectNumber, objectGeneration, offset));
            }

            if (span.ToString().Contains("xref"))
                return new ChangeModeParsedLine(offset, ParseMode.ExpectingXRefTableHeader);

            return null;
        }

        private static IParsedLine ParseLineXrefTableHeaderMode(ReadOnlySpan<char> span, long offset, char separator)
        {
            int scanned = -1;
            int position = 0;

            ReadOnlySpan<char> firstObjectNumberSpan = ParseChunk(ref span, ref scanned, ref position, separator);
            if (int.TryParse(firstObjectNumberSpan, out int firstObjectNumber))
            {
                ReadOnlySpan<char> xrefTebleSizeSpan = ParseChunk(ref span, ref scanned, ref position, separator);
                if (!int.TryParse(xrefTebleSizeSpan, out int xrefTebleSize))
                    return null;

                return new XRefTableHeaderParsedLine(firstObjectNumber, xrefTebleSize, offset);
            }

            return null;
        }

        private static IParsedLine ParseLineXrefMode(ReadOnlySpan<char> span, long offset, char separator)
        {
            int scanned = -1;
            int position = 0;

            ReadOnlySpan<char> objectOffsetSpan = ParseChunk(ref span, ref scanned, ref position, separator);
            if (long.TryParse(objectOffsetSpan, out long objectOffset))
            {
                ReadOnlySpan<char> objectGenerationSpan = ParseChunk(ref span, ref scanned, ref position, separator);
                if (!int.TryParse(objectGenerationSpan, out int objectGeneration))
                    return null;

                return new ObjectDataParsedLine(
                    offset,
                    ParseMode.InsideXref,
                    new ObjectData(0, objectGeneration, objectOffset));
            }

            if (objectOffsetSpan.ToString().Equals("startxref"))
                return new ChangeModeParsedLine(offset, ParseMode.Regular);

            return null;
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