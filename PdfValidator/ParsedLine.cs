using PdfValidator.Infrastracture;

namespace PdfValidator
{
    internal class ParsedLine : ILineInfo
    {
        public long LineOffset { get; }

        public ProcessMode ProcessMode { get; }

        public ObjectData ObjectData { get; }

        public ParsedLine(long offset, ProcessMode processMode)
        {
            LineOffset = offset;
            ProcessMode = processMode;
        }

        public ParsedLine(long offset, ProcessMode processMode, ObjectData objectData)
        {
            LineOffset = offset;
            ProcessMode = processMode;
            ObjectData = objectData;
        }
    }
}