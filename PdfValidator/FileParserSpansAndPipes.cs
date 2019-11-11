using PdfValidator.Infrastracture;
using PdfValidator.ParsedLine;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace PdfValidator
{
    internal class FileParserSpansAndPipes : IPdfValidator
    {
        private const int mLengthLimit = 16384;

        public async Task<ValidationResult> Validate(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                PipeReader reader = PipeReader.Create(stream);
                ParsingData parsingData = new ParsingData();

                bool isReadComplete = false;
                while (!isReadComplete)
                {
                    ReadResult read = await reader.ReadAsync();
                    ReadOnlySequence<byte> buffer = read.Buffer;

                    ReadFromBuffer(ref buffer, parsingData);

                    reader.AdvanceTo(buffer.Start, buffer.End);
                    isReadComplete = read.IsCompleted;
                }

                return ValidatePdfObjectsToXrefObjects(parsingData.PdfObjects, parsingData.XRefObjects);
            }
        }

        private void ReadFromBuffer(ref ReadOnlySequence<byte> buffer, ParsingData parsingData)
        {
            while (TryReadLine(ref buffer, out ReadOnlySequence<byte> sequence))
            {
                IParsedLine parsedLine = ProcessSequence(
                    sequence, ref parsingData.CurrentOffset, parsingData.ParseMode);

                if (parsedLine == null)
                    continue;

                if (parsedLine.ParseMode == ParseMode.Regular &&
                    parsedLine is ObjectDataParsedLine objectDataParsedLine)
                {
                    parsingData.PdfObjects.Add(objectDataParsedLine.ObjectData);
                }

                if (parsedLine.ParseMode == ParseMode.InsideXref &&
                    parsedLine is XRefTableHeaderParsedLine xRefTableHeaderParsedLine)
                {
                    parsingData.CurrentXRefTableFirstObjectNumber =
                        xRefTableHeaderParsedLine.FirstObjectNumber;
                    parsingData.CurrentXRefTableSize =
                        xRefTableHeaderParsedLine.TableSize;
                    parsingData.CurrentXRefTableObjectNumberIndex = 0;
                }

                if (parsedLine.ParseMode == ParseMode.InsideXref &&
                    parsedLine is ObjectDataParsedLine xrefObjectDataParsedLine)
                {
                    parsingData.XRefObjects.Add(
                        new ObjectData(
                            parsingData.CurrentXRefTableObjectNumberIndex + parsingData.CurrentXRefTableFirstObjectNumber,
                            xrefObjectDataParsedLine.ObjectData.ObjectGeneration,
                            xrefObjectDataParsedLine.ObjectData.ObjectOffset));

                    parsingData.CurrentXRefTableObjectNumberIndex++;
                }

                parsingData.ParseMode = parsedLine.ParseMode;
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

        private IParsedLine ProcessSequence(ReadOnlySequence<byte> sequence, ref long offset, ParseMode parseMode)
        {
            if (sequence.IsSingleSegment)
                return Parse(sequence.First.Span, ref offset, parseMode);

            var length = (int)sequence.Length;
            if (length > mLengthLimit)
                throw new ArgumentException($"Line has a length exceeding the limit: {length}");

            Span<byte> span = stackalloc byte[(int)sequence.Length];
            sequence.CopyTo(span);

            return Parse(span, ref offset, parseMode);
        }

        private IParsedLine Parse(ReadOnlySpan<byte> bytes, ref long offset, ParseMode parseMode)
        {
            Span<char> chars = stackalloc char[bytes.Length];
            Encoding.UTF8.GetChars(bytes, chars);

            IParsedLine parsedLine = LineParserSpans.ParseLine(chars, offset, parseMode);

            // Add 1 for CR or LF byte.
            offset++;

            offset += bytes.Length;
            return parsedLine;
        }

        ValidationResult ValidatePdfObjectsToXrefObjects(List<ObjectData> pdfObjects, List<ObjectData> xrefObjects)
        {
            bool validatonResult = true;
            StringBuilder errorText = new StringBuilder();
            foreach(ObjectData pdfObject in pdfObjects)
            {
                ObjectData xrefObject = xrefObjects.Find(obj =>
                    obj.ObjectNumber == pdfObject.ObjectNumber
                    &&
                    obj.ObjectOffset == pdfObject.ObjectOffset);

                if (xrefObject != null)
                    xrefObjects.Remove(xrefObject);
                else
                {
                    errorText.AppendLine($"Object number {pdfObject.ObjectNumber} does not exist in xref table");
                    validatonResult = false;
                }
            }

            if(xrefObjects.Count > 1)
            {
                errorText.Append($"The next object numbers found on xref table but no object found in pdf:");
                foreach (ObjectData pdfObject in xrefObjects)
                {
                    errorText.Append($" {pdfObject.ObjectNumber}");
                }

                validatonResult = false;
            }

            return new ValidationResult(validatonResult, errorText.ToString());
        }
    }
}