using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace PdfValidator
{
    internal class FileParserSpansAndPipes : IFileParser
    {
        private const int mLengthLimit = 16384;

        public async Task<List<ObjectData>> Parse(string file)
        {
            var result = new List<ObjectData>();
            using (FileStream stream = File.OpenRead(file))
            {
                PipeReader reader = PipeReader.Create(stream);
                long offset = 0;
                while (true)
                {
                    ReadResult read = await reader.ReadAsync();
                    ReadOnlySequence<byte> buffer = read.Buffer;
                    while (TryReadLine(ref buffer, out ReadOnlySequence<byte> sequence))
                    {
                        ObjectData objectData = ProcessSequence(sequence, ref offset);
                        if (objectData != null)
                            result.Add(objectData);
                    }

                    reader.AdvanceTo(buffer.Start, buffer.End);
                    if (read.IsCompleted)
                        break;
                }

                return result;
            }
        }

        private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            SequencePosition? CRPosition = buffer.PositionOf((byte)'\r');
            SequencePosition? LFPosition = buffer.PositionOf((byte)'\n');

            if (!CRPosition.HasValue && !LFPosition.HasValue)
            {
                line = default;
                return false;
            }

            SequencePosition newLinePosition = GetNewLinePosition(buffer, CRPosition, LFPosition);
            line = buffer.Slice(0, newLinePosition);
            buffer = buffer.Slice(buffer.GetPosition(1, newLinePosition));

            return true;
        }

        private SequencePosition GetNewLinePosition(ReadOnlySequence<byte> buffer, SequencePosition? CRPosition, SequencePosition? LFPosition)
        {
            // In case of line ends with LF and no CR in the rest of the buffer.
            if (!CRPosition.HasValue)
                return LFPosition.Value;

            // In case of line ends with CR and no LF in the rest of the buffer.
            if (!LFPosition.HasValue)
                return CRPosition.Value;

            ReadOnlySequence<byte> CRLine = buffer.Slice(0, CRPosition.Value);
            ReadOnlySequence<byte> LFLine = buffer.Slice(0, LFPosition.Value);

            // In case of line end with CRLF.
            if (LFLine.Length - CRLine.Length == 1)
                return LFPosition.Value;

            // In case of line ends with LF.
            if (LFLine.Length < CRLine.Length)
                return LFPosition.Value;

            // In case of line ends with Cr.
            else
                return CRPosition.Value;
        }

        private ObjectData ProcessSequence(ReadOnlySequence<byte> sequence, ref long offset)
        {
            if (sequence.IsSingleSegment)
                return Parse(sequence.First.Span, ref offset);

            var length = (int)sequence.Length;
            if (length > mLengthLimit)
                throw new ArgumentException($"Line has a length exceeding the limit: {length}");

            Span<byte> span = stackalloc byte[(int)sequence.Length];
            sequence.CopyTo(span);

            return Parse(span, ref offset);
        }

        private ObjectData Parse(ReadOnlySpan<byte> bytes, ref long offset)
        {
            Span<char> chars = stackalloc char[bytes.Length];
            Encoding.UTF8.GetChars(bytes, chars);

            ObjectData objectData = LineParserSpans.Parse(chars, ref offset);

            // Add 1 for CR or LF byte.
            offset++;

            offset += bytes.Length;
            return objectData;
        }
    }
}