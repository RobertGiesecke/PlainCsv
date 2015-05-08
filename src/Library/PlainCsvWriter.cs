using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RGiesecke.PlainCsv
{
  public class PlainCsvWriter : PlainCsvBase
  {
    public PlainCsvWriter(CsvOptions csvOptions = null)
      : base(csvOptions)
    {
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
      IEnumerable<Hashtable> list,
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

    protected sealed class WrappedEqualityComparer : IEqualityComparer<object>
    {
      private readonly IEqualityComparer _Comparer;
      public bool Equals(object x, object y)
      {
        return _Comparer.Equals(x, y);
      }

      public int GetHashCode(object obj)
      {
        return _Comparer.GetHashCode(obj);
      }

      public WrappedEqualityComparer(IEqualityComparer comparer)
      {
        _Comparer = comparer;
      }
    }

    public void DictionariesToCsv(TextWriter writer, IEnumerable<Hashtable> list,
      IEqualityComparer keyComparer = null, CultureInfo cultureInfo = null)
    {
      IEqualityComparer<object> comparer = keyComparer != null ? new WrappedEqualityComparer(keyComparer) : null;
      DictionariesToCsv(writer,
          list.Select(t => t.Cast<DictionaryEntry>()
                            .Select(e => new KeyValuePair<object, object>(e.Key, e.Value))),
          comparer, cultureInfo);
    }

    public void DictionariesToCsv<TKey, TValue>(TextWriter writer, IEnumerable<IEnumerable<KeyValuePair<TKey, TValue>>> list, IEqualityComparer<TKey> keyComparer = null, CultureInfo cultureInfo = null)
    {
      var asList = list.Select(t => WrapDictionary(t, keyComparer)).ToList().AsReadOnly();
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
          return "=" + CsvUtils.QuoteText(n, csvOptions.QuoteChar);

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

    protected virtual DictionaryWrapper<TKey, TValue> WrapDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> keyComparer)
    {
      if (keyComparer != null)
      {
        var d = keyValuePairs as Dictionary<TKey, TValue>;
        if (d != null && !Equals(d.Comparer, keyComparer))
          keyValuePairs = keyValuePairs.ToDictionary(k => k.Key, v => v.Value, keyComparer);
      }

#if ReadOnlyDictionary
      var rd = keyValuePairs as IReadOnlyDictionary<TKey, TValue>;
      if (rd != null)
        return new DictionaryWrapper<TKey, TValue>(rd.Keys, rd.Count, rd.TryGetValue);
#endif
      var dx = keyValuePairs as IDictionary<TKey, TValue> ?? keyValuePairs.ToDictionary(k => k.Key, v => v.Value, keyComparer);
      return new DictionaryWrapper<TKey, TValue>(dx.Keys.AsEnumerable(), dx.Count, dx.TryGetValue);
    }

    protected virtual char[] GetEscapeChars(CsvOptions csvOptions)
    {
      return new[] { csvOptions.Delimiter, csvOptions.QuoteChar, '\r', '\n' };
    }
  }
}
