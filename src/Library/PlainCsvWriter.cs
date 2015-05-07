using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RGiesecke.PlainCsv
{
  public class PlainCsvWriter : PlainCsvBase
  {
    public PlainCsvWriter(CsvOptions csvOptions = null)
      : base(csvOptions)
    {
    }

    public void DictionariesToCsv<TKey, TValue>(TextWriter writer, IEnumerable<IEnumerable<KeyValuePair<TKey, TValue>>> list, IEqualityComparer<TKey> keyComparer = null, CultureInfo cultureInfo = null)
    {
      var asList = list.Select(WrapDictionary).ToList().AsReadOnly();
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

          writer.Write(escapeText(Convert.ToString(headerValues[i], cultureInfo)));
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
            writer.Write(escapeText(Convert.ToString(value, cultureInfo)));
        }
        writer.WriteLine();
      }
    }

    protected virtual DictionaryWrapper<TKey, TValue> WrapDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
    {
#if ReadOnlyDictionary
      var rd = keyValuePairs as IReadOnlyDictionary<TKey, TValue>;
      if (rd != null)
        return new DictionaryWrapper<TKey, TValue>(rd.Keys, rd.Count, rd.TryGetValue);
#endif
      var dx = (IDictionary<TKey, TValue>)keyValuePairs;
      return new DictionaryWrapper<TKey, TValue>(dx.Keys.AsEnumerable(), dx.Count, dx.TryGetValue);
    }

    protected virtual char[] GetEscapeChars(CsvOptions csvOptions)
    {
      return new[] { csvOptions.Delimiter, csvOptions.QuoteChar, '\r', '\n' };
    }
  }
}
