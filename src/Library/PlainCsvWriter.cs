using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using RGiesecke.PlainCsv.Core;

namespace RGiesecke.PlainCsv
{
  public class PlainCsvWriter : PlainCsvBase
  {
    protected IDictionaryEntryConverter DictionaryEntryConverter { get; private set; }

    public PlainCsvWriter(CsvOptions csvOptions = null)
      : this(Core.DictionaryEntryConverter.Default, csvOptions)
    {
    }

    public PlainCsvWriter(IDictionaryEntryConverter dictionaryEntryConverter, CsvOptions csvOptions = null)
      : base(csvOptions)
    {
      if (dictionaryEntryConverter == null) throw new ArgumentNullException("dictionaryEntryConverter");
      DictionaryEntryConverter = dictionaryEntryConverter;
    }

    public StringBuilder DictionariesToCsvString<TKey, TValue>(IEnumerable<IEnumerable<KeyValuePair<TKey, TValue>>> list,
      IEqualityComparer<TKey> keyComparer = null, CultureInfo cultureInfo = null)
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

    public void DictionariesToCsv<TKey, TValue>(TextWriter writer, IEnumerable<IEnumerable<KeyValuePair<TKey, TValue>>> list, IEqualityComparer<TKey> keyComparer = null, CultureInfo cultureInfo = null)
    {
      var asList = list.Select(t => DictionaryEntryConverter.WrapDictionary(t, keyComparer)).ToList().AsReadOnly();
      if (asList.Count == 0)
        return;

      cultureInfo = cultureInfo ?? GetPersistentCultureInfo();

      var csvOptions = CsvOptions;

      var headerValues = asList.SelectMany(t => t.Keys).Distinct(keyComparer).ToList();

      var escapeChars = GetEscapeChars(csvOptions);

      Func<string, string> escapeText = n =>
      {
        if (string.IsNullOrEmpty(n))
          return null;

        if (csvOptions.QuoteFormulars && n.Trim().StartsWith("="))
          return CsvUtils.QuoteText("=" + CsvUtils.QuoteText(n, csvOptions.QuoteChar), csvOptions.QuoteChar);

        if (n.IndexOfAny(escapeChars) > -1)
          return CsvUtils.QuoteText(n, csvOptions.QuoteChar);

        return n;
      };

      if (csvOptions.UseHeaderRow)
      {
        for (int i = 0; i < headerValues.Count; i++)
        {
          if (i > 0)
            writer.Write(csvOptions.Delimiter);

          writer.Write(escapeText(CsvUtils.ConvertToString(headerValues[i], cultureInfo)));
        }

        writer.WriteLine();
      }

      foreach (var row in asList)
      {
        for (int i = 0; i < headerValues.Count; i++)
        {
          if (i > 0)
            writer.Write(csvOptions.Delimiter);

          TValue value;
          if (row.TryGetValue(headerValues[i], out value))
            writer.Write(escapeText(CsvUtils.ConvertToString(value, cultureInfo)));
        }
        writer.WriteLine();
      }
    }

    protected virtual char[] GetEscapeChars(CsvOptions csvOptions)
    {
      return new[] { csvOptions.Delimiter, csvOptions.QuoteChar, '\r', '\n' };
    }
  }
}
