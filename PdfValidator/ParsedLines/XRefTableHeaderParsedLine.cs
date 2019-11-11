using PdfValidator.Infrastracture;

namespace PdfValidator.ParsedLine
{
    internal class XRefTableHeaderParsedLine : IParsedLine
    {
        public long LineOffset { get; }

        public ParseMode ParseMode => ParseMode.InsideXref;

        public int FirstObjectNumber { get; }

        public int TableSize { get; }

        public XRefTableHeaderParsedLine(int firstObjectNumber, int tableSize, long offset)
        {
            LineOffset = offset;
            FirstObjectNumber = firstObjectNumber;
            TableSize = tableSize;
        }
    }
}