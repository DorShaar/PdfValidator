namespace PdfValidator
{
    internal enum ParseMode
    {
        Regular = 0,
        ExpectingXRefTableHeader = 1,
        InsideXref = 2,
    }
}