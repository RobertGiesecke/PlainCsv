using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using RGiesecke.PlainCsv.Core;

namespace RGiesecke.PlainCsv
{
  public class PlainCsvWriter : PlainCsvBase<CsvWriterOptions>
  {
    protected IDictionaryEntryConverter DictionaryEntryConverter { get; private set; }

    public PlainCsvWriter(CsvWriterOptions csvOptions = null)
      : this(Core.DictionaryEntryConverter.Default, csvOptions)
    {
    }

    public PlainCsvWriter(IDictionaryEntryConverter dictionaryEntryConverter, CsvWriterOptions csvOptions = null)
      : base(csvOptions)
    {
      if (dictionaryEntryConverter == null) throw new ArgumentNullException("dictionaryEntryConverter");
      DictionaryEntryConverter = dictionaryEntryConverter;
    }

    public StringBuilder DictionariesToCsvString<TKey, TValue>(
      IEnumerable<IEnumerable<KeyValuePair<TKey, TValue>>> list,
      IEqualityComparer<TKey> keyComparer = null,
      CultureInfo cultureInfo = null)
    {
      var sb = new StringBuilder();
      using (var w = new StringWriter(sb))
      {
        DictionariesToCsv(w, list, keyComparer, cultureInfo);
      }
      return sb;
    }

    public StringBuilder DictionariesToCsvString(
      IEnumerable<IDictionary> list,
      IEqualityComparer keyComparer = null,
      CultureInfo cultureInfo = null)
    {
      var sb = new StringBuilder();
      using (var w = new StringWriter(sb))
      {
        DictionariesToCsv(w, list, keyComparer, cultureInfo);
      }
      return sb;
    }

    public void DictionariesToCsv(TextWriter writer, IEnumerable<IDictionary> list,
      IEqualityComparer keyComparer = null, CultureInfo cultureInfo = null)
    {
      var comparer = WrappedEqualityComparer.FromUntyped(keyComparer);

      DictionariesToCsv(writer,
          list.Select(t => t.Cast<object>().Select(DictionaryEntryConverter.FromUntyped(t))),
          comparer, cultureInfo);
    }

    IEnumerable<DictionaryWrapper<TKey, TValue>> EnumerateRows<TKey, TValue>(
      IEnumerator<DictionaryWrapper<TKey, TValue>> rowEnumerator)
    {
      yield return rowEnumerator.Current;
      while (rowEnumerator.MoveNext())
        yield return rowEnumerator.Current;
    }

    public void DictionariesToCsv<TKey, TValue>(
      TextWriter writer,
      IEnumerable<IEnumerable<KeyValuePair<TKey, TValue>>> list,
      IEqualityComparer<TKey> keyComparer = null,
      CultureInfo cultureInfo = null)
    {
      cultureInfo = cultureInfo ?? GetPersistentCultureInfo();

      ReadOnlyCollection<TKey> headerValues;
      IEnumerable<DictionaryWrapper<TKey, TValue>> rows;
      if (CsvOptions.AssumeFixedColumnCount)
      {
        var asList = from t in list
                     select DictionaryEntryConverter.WrapDictionary(t, keyComparer);

        var enumerator = asList.GetEnumerator();

        if (!enumerator.MoveNext())
          return;

        var firstRow = enumerator.Current;

        rows = EnumerateRows(enumerator);

        headerValues = firstRow.Keys
          .Distinct(keyComparer)
          .ToList()
          .AsReadOnly();
      }
      else
      {
        var asList = (from t in list
                      select DictionaryEntryConverter.WrapDictionary(t, keyComparer))
                       .ToList()
                       .AsReadOnly();
        if (asList.Count == 0)
          return;
        rows = asList;

        headerValues = asList.SelectMany(t => t.Keys)
          .Distinct(keyComparer)
          .ToList()
          .AsReadOnly();
      }

      IList<string> sortedColumnNames = CsvOptions.SortedColumnNames.ToList();
      IList<string> headerNames = (from t in headerValues
                                   select ConvertToString(t, cultureInfo))
                                    .ToList()
                                    .AsReadOnly();

      if (sortedColumnNames.Count > 0)
      {
        var reorderedHeaders = (from idx in Enumerable.Range(0, headerNames.Count)
                                let name = headerNames[idx]
                                let value = headerValues[idx]
                                let sortIndex = sortedColumnNames.IndexOf(name)
                                orderby sortIndex < 0 ? 1 : 0,
                                        sortIndex
                                select new
                                {
                                  name,
                                  value
                                }).ToList();
        headerValues = reorderedHeaders.ConvertAll(t => t.value).AsReadOnly();
        headerNames = reorderedHeaders.ConvertAll(t => t.name).AsReadOnly();
      }


      var escapeChars = GetEscapeChars(CsvOptions);

      Func<string, string> escapeText = n =>
      {
        if (string.IsNullOrEmpty(n))
          return null;

        if (CsvOptions.QuoteFormulars && n.Trim().StartsWith("="))
          return CsvUtils.QuoteText("=" + CsvUtils.QuoteText(n, CsvOptions.QuoteChar),
                                    CsvOptions.QuoteChar);

        if (n.IndexOfAny(escapeChars) > -1)
          return CsvUtils.QuoteText(n, CsvOptions.QuoteChar);

        return n;
      };

      if (CsvOptions.UseHeaderRow)
      {
        for (int i = 0; i < headerValues.Count; i++)
        {
          if (i > 0)
            writer.Write(CsvOptions.Delimiter);

          writer.Write(escapeText(headerNames[i]));
        }

        writer.WriteLine();
      }
      var rowIndex = CsvOptions.UseHeaderRow ? 1 : 0;
      foreach (var row in rows)
      {
        ValidateRowValues(rowIndex, cultureInfo, row, headerNames, headerValues, keyComparer);

        for (int i = 0; i < headerValues.Count; i++)
        {
          if (i > 0)
            writer.Write(CsvOptions.Delimiter);

          TValue value;
          if (row.TryGetValue(headerValues[i], out value))
            writer.Write(escapeText(ConvertToString(value, cultureInfo)));
        }
        writer.WriteLine();
        rowIndex += 1;
      }
    }

    private void ValidateRowValues<TKey, TValue>(int rowIndex, CultureInfo cultureInfo, DictionaryWrapper<TKey, TValue> row,
       IList<string> headerNames,
       IEnumerable<TKey> headerValues,
       IEqualityComparer<TKey> keyComparer)
    {
      if (!CsvOptions.AssumeFixedColumnCount)
        return;

      var keyValues = new HashSet<TKey>(row.Keys, keyComparer);

      var failed = row.Count != headerNames.Count ||
                   keyValues.Count != headerNames.Count;

      if (!failed)
      {
        keyValues.IntersectWith(headerValues);
        failed = keyValues.Count != headerNames.Count;
      }

      if (!failed) return;

      var keyNames = keyValues.Select(t => ConvertToString(t, cultureInfo))
        .ToList()
        .AsReadOnly();

      throw new IncorrectCsvColumnCountException(rowIndex,
                                                 keyNames,
                                                 new ReadOnlyCollection<string>(headerNames));
    }

    protected virtual char[] GetEscapeChars(CsvWriterOptions csvOptions)
    {
      return new[] { csvOptions.Delimiter, csvOptions.QuoteChar, '\r', '\n' };
    }

    protected override CsvWriterOptions GetDefaultOptions()
    {
      return CsvWriterOptions.Default;
    }
  }
}
