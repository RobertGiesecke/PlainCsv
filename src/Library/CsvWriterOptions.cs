using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
#if ReadOnlyDictionary
using ReadOnlyStrings = System.Collections.Generic.IReadOnlyList<string>;
#else
using ReadOnlyStrings = System.Collections.ObjectModel.ReadOnlyCollection<string>;
#endif

namespace RGiesecke.PlainCsv
{
  public class CsvWriterOptions : CsvOptions
  {
    public static readonly new CsvWriterOptions Default = new CsvWriterOptions(CsvOptions.Default);
    public static readonly new CsvWriterOptions Excel = new CsvWriterOptions(CsvOptions.Excel);

    public ReadOnlyStrings SortedColumnNames { get; private set; }

    public CsvWriterOptions(char quoteChar, char delimiter, CsvFlags? csvFlags = null, IEnumerable<string> sortedColumnNames = null)
      : base(quoteChar, delimiter, csvFlags)
    {
      SortedColumnNames = GetReadOnlyColumnNames(sortedColumnNames);
    }

    public CsvWriterOptions(IEnumerable<string> sortedColumnNames = null)
    {
      SortedColumnNames = GetReadOnlyColumnNames(sortedColumnNames);
    }

    public CsvWriterOptions(params string[] sortedColumnNames)
      : this(sortedColumnNames.AsEnumerable())
    {
    }

    public CsvWriterOptions(CsvWriterOptions source, char? quoteChar = null, char? delimiter = null, CsvFlags? csvFlags = null, IEnumerable<string> sortedColumnNames = null)
      : base(source, quoteChar, delimiter, csvFlags)
    {
      if (sortedColumnNames != null)
        SortedColumnNames = GetReadOnlyColumnNames(sortedColumnNames);
      else
        SortedColumnNames = source.SortedColumnNames;
    }

    public CsvWriterOptions(CsvOptions source, char? quoteChar = null, char? delimiter = null, CsvFlags? csvFlags = null, IEnumerable<string> sortedColumnNames = null)
      : base(source, quoteChar, delimiter, csvFlags)
    {
      SortedColumnNames = GetReadOnlyColumnNames(sortedColumnNames);
    }

    protected virtual ReadOnlyStrings GetReadOnlyColumnNames(IEnumerable<string> sortedColumnNames)
    {
      IList<string> columnNames;
      if (sortedColumnNames != null)
        columnNames = sortedColumnNames.ToList();
      else
        columnNames = new string[0];

      return new ReadOnlyCollection<string>(columnNames);
    }
  }
}