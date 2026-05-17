namespace ReplayLab.Parsers.Csv;

public sealed class CsvParseException : Exception
{
    public CsvParseException(string message)
        : base(message)
    {
    }
}
