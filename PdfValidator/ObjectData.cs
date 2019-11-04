namespace PdfValidator
{
    internal class ObjectData
    {
        public int ObjectNumber { get; }
        public int ObjectGeneration { get; }

        public ObjectData(int objectNumber, int objectGeneration)
        {
            ObjectNumber = objectNumber;
            ObjectGeneration = objectGeneration;
        }
    }
}