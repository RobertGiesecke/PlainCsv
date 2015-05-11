using System;

namespace RGiesecke.PlainCsv
{

  [Flags]
  public enum CsvFlags
  {
    None = 0,
    UseHeaderRow = 1,
    QuoteFormulars = 2,
  }

  public sealed class CsvOptions
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

    public CsvOptions(char quoteChar, char delimiter, CsvFlags? csvFlags = null)
    {
      QuoteChar = quoteChar;
      Delimiter = delimiter;
      CsvFlags = csvFlags ?? Default.CsvFlags;
    }

    public CsvOptions()
      : this('"', ',', CsvFlags.UseHeaderRow)
    { }

    public CsvOptions(CsvOptions source, char? quoteChar = null, char? delimiter = null, CsvFlags? csvFlags = null)
    :this(quoteChar ?? source.QuoteChar,
          delimiter ?? source.Delimiter,
          csvFlags ?? source.CsvFlags)
    {}
  }
}