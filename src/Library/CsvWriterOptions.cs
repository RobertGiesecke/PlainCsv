using System;
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
    public bool AssumeFixedColumnCount { get; private set; }
    public static readonly new CsvWriterOptions Default = new CsvWriterOptions(CsvOptions.Default);
    public static readonly new CsvWriterOptions Excel = new CsvWriterOptions(CsvOptions.Excel);

    public ReadOnlyStrings SortedColumnNames { get; private set; }

    public CsvWriterOptions(
      char? quoteChar = null, 
      char? delimiter = null, 
      CsvFlags? csvFlags = null,
      bool? assumeFixedColumnCount = null,
      IEnumerable<string> sortedColumnNames = null)
      : this(Default, quoteChar, delimiter, csvFlags, assumeFixedColumnCount, sortedColumnNames)
    {
    }

    public CsvWriterOptions(
      CsvWriterOptions source, 
      char? quoteChar = null, 
      char? delimiter = null,
      CsvFlags? csvFlags = null,
      bool? assumeFixedColumnCount = null,
      IEnumerable<string> sortedColumnNames = null)
      : base(source, quoteChar, delimiter, csvFlags)
    {
      AssumeFixedColumnCount = assumeFixedColumnCount ?? Default.AssumeFixedColumnCount;
      if (sortedColumnNames != null)
        SortedColumnNames = GetReadOnlyColumnNames(sortedColumnNames);
      else
        SortedColumnNames = source.SortedColumnNames;
    }

    public CsvWriterOptions(CsvOptions source)
      : base(source)
    {
      SortedColumnNames = new ReadOnlyCollection<string>(new string[0]);
      AssumeFixedColumnCount = false;
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