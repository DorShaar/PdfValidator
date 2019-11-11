using System.Collections.Generic;

namespace PdfValidator
{
    internal class ParsingData
    {
        public ParseMode ParseMode = ParseMode.Regular;
        public long CurrentOffset = 0;
        public List<ObjectData> PdfObjects { get; } = new List<ObjectData>();
        public List<ObjectData> XRefObjects { get; } = new List<ObjectData>();
        public int CurrentXRefTableFirstObjectNumber;
        public int CurrentXRefTableSize;
        public int CurrentXRefTableObjectNumberIndex;
    }
}