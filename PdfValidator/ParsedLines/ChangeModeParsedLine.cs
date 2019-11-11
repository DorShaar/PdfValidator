using PdfValidator.Infrastracture;

namespace PdfValidator.ParsedLines
{
    internal class ChangeModeParsedLine : IParsedLine
    {
        public long LineOffset { get; }

        public ParseMode ParseMode { get; }

        public ChangeModeParsedLine(long offset, ParseMode parseMode)
        {
            LineOffset = offset;
            ParseMode = parseMode;
        }
    }
}