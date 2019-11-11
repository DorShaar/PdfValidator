namespace PdfValidator.Infrastracture
{
    internal interface IParsedLine
    {
        long LineOffset { get; }
        ParseMode ParseMode { get; }
    }
}