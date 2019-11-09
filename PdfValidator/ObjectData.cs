namespace PdfValidator
{
    internal class ObjectData
    {
        public int ObjectNumber { get; }
        public int ObjectGeneration { get; }
        public long ObjectOffset { get; }

        public ObjectData(int objectNumber, int objectGeneration, long offset)
        {
            ObjectNumber = objectNumber;
            ObjectGeneration = objectGeneration;
            ObjectOffset = offset;
        }
    }
}