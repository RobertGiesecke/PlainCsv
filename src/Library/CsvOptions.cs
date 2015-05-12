using System;

namespace RGiesecke.PlainCsv
{

  [Flags]
  public enum CsvFlags
  {
    None = 0,
    UseHeaderRow = 1,
    QuoteFormulars = 2,
    Iso8601Dates = 4,
    /// <summary>
    /// When set <see cref="PlainCsvReader.ReadCsvRows(System.Collections.Generic.IEnumerable{char})"/> will take line breaks when the field count is less than those of the first row.
    /// </summary>
    UnQuotedLineBreaks = 8,
  }

  public class CsvOptions
  {
    public static readonly CsvOptions Default = new CsvOptions();
    public static readonly CsvOptions Excel = new CsvOptions('"', ',', CsvFlags.QuoteFormulars | CsvFlags.UseHeaderRow);

    public char QuoteChar { get; private set; }
    public char Delimiter { get; private set; }
    public CsvFlags CsvFlags { get; private set; }

    public bool QuoteFormulars
    {
      get
      {
        return (CsvFlags & CsvFlags.QuoteFormulars) == CsvFlags.QuoteFormulars;
      }
    }
    public bool UseHeaderRow
    {
      get
      {
        return (CsvFlags & CsvFlags.UseHeaderRow) == CsvFlags.UseHeaderRow;
      }
    }

    public CsvOptions(char? quoteChar = null, char? delimiter = null, CsvFlags? csvFlags = null)
    {
      var usedQuoteChar = quoteChar ?? Default.QuoteChar;
      var usedDelimiter = delimiter ?? Default.Delimiter;
      if (usedQuoteChar == usedDelimiter)
      {
        throw new ArgumentOutOfRangeException("delimiter",
                                              string.Format("delimiter ({0}) and quoteChar ({1}) cannot be the same.",
                                                            usedDelimiter,
                                                            usedQuoteChar));
      }
      QuoteChar = usedQuoteChar;
      Delimiter = usedDelimiter;
      CsvFlags = csvFlags ?? Default.CsvFlags;
    }

    protected CsvOptions()
      : this('"', ',', CsvFlags.UseHeaderRow | CsvFlags.Iso8601Dates)
    { }

    public CsvOptions(CsvOptions source, char? quoteChar = null, char? delimiter = null, CsvFlags? csvFlags = null)
    :this(quoteChar ?? source.QuoteChar,
          delimiter ?? source.Delimiter,
          csvFlags ?? source.CsvFlags)
    {}
  }
}