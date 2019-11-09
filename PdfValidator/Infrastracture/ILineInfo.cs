namespace PdfValidator.Infrastracture
{
    internal interface ILineInfo
    {
        long LineOffset { get; }
        ProcessMode ProcessMode { get; }
    }
}