namespace PdfValidator
{
    internal interface ILineParser
    {
        ObjectData Parse(string line);
    }
}