using PdfValidator.Infrastracture;
using System;

namespace PdfValidator.ParsedLine
{
    internal class ObjectDataParsedLine : IParsedLine
    {
        public long LineOffset { get; }
        public ParseMode ParseMode { get; }
        public ObjectData ObjectData { get; }

        public ObjectDataParsedLine(long lineOffset, ParseMode processMode, ObjectData objectData)
        {
            LineOffset = lineOffset;
            ParseMode = processMode;
            ObjectData = objectData ?? 
                throw new ArgumentException($"{nameof(ObjectData)} cannot be null");
        }
    }
}