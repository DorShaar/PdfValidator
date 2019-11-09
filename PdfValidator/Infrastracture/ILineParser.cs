namespace PdfValidator.Infrastracture
{
    internal interface ILineParser
    {
        ObjectData ParseLine(string line, long offset = 0);
    }
}