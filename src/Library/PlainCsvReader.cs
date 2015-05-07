using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
#if ReadOnlyDictionary
using ReturnedDictionary = System.Collections.Generic.IReadOnlyDictionary<string, string>;
using ReadOnlyStrings = System.Collections.Generic.IReadOnlyList<string>;
#else
using ReturnedDictionary = System.Collections.Generic.IDictionary<string, string>;
using ReadOnlyStrings = System.Collections.ObjectModel.ReadOnlyCollection<string>;
#endif
namespace RGiesecke.PlainCsv
{
  public class PlainCsvReader : PlainCsvBase
  {
    static readonly char[] _LineBreakChars = { '\r', '\n' };

    public PlainCsvReader(CsvOptions csvOptions = null)
      : base(csvOptions)
    {
    }

    public IEnumerable<ReturnedDictionary> CsvToDictionaries(IEnumerable<char> characters, IEqualityComparer<string> keyComparer = null)
    {
      var rows = ReadCsvRows(characters);

      var rowsEnum = rows.GetEnumerator();
      if (!rowsEnum.MoveNext())
        yield break;

      ReadOnlyStrings headerNames;
      ReadOnlyStrings currentValues;
      if (CsvOptions.UseHeaderRow)
      {
        headerNames = rowsEnum.Current;
        if (!rowsEnum.MoveNext())
          yield break;
        currentValues = rowsEnum.Current;
      }
      else
        headerNames = (currentValues = rowsEnum.Current).Select((n, i) => "Field" + (i + 1)).ToList().AsReadOnly();

      var rowIndex = 0;
      do
      {
        var d = new OrderedDictionary<string, string>(keyComparer);
        if (currentValues.Count != headerNames.Count)
          throw new InvalidOperationException(string.Format("Row {0}: The row value count ({1}) does not equal the head count ({2}).\n{3}\n{4}", rowIndex, currentValues.Count, headerNames.Count, string.Join(",", headerNames.ToArray()), string.Join(",", currentValues.ToArray())));

        for (int i = 0; i < headerNames.Count; i++)
        {
          d.Add(headerNames[i], currentValues[i]);
        }
        yield return d;
        rowIndex += 1;
        currentValues = rowsEnum.Current;
      } while (rowsEnum.MoveNext());
    }

    public IEnumerable<ReadOnlyStrings> ReadCsvRows(IEnumerable<Char> characters)
    {
      var currentValues = new List<string>();

      var withinString = false;

      var currentValue = new StringBuilder();

      var charEnum = characters.GetEnumerator();
      Func<char?> peek;
      Func<bool> moveNext;
      {
        bool? peeked = null;

        peek = () =>
        {
          peeked = peeked ?? charEnum.MoveNext();
          if (!peeked.Value)
            return null;
          return charEnum.Current;
        };

        moveNext = () =>
        {
          try
          {
            return peeked ?? charEnum.MoveNext();
          }
          finally
          {
            peeked = null;
          }
        };
      }

      Func<string> getAndResetCurrentValue = () =>
      {
        try
        {
          return currentValue.Length > 0 ? currentValue.ToString() : null;
        }
        finally
        {
          currentValue.Length = 0;
        }
      };

      char? previousChar = null;
      var maxValuesCount = 0;
      while (moveNext())
      {
        var currentChar = charEnum.Current;
        {
          if (currentChar == CsvOptions.QuoteChar)
          {
            char? nextChar;

            if (withinString && (nextChar = peek()) != null && nextChar.Value == CsvOptions.QuoteChar)
            {
              currentValue.Append(CsvOptions.QuoteChar);
              moveNext();
              previousChar = currentChar;

              continue;
            }

            withinString = !withinString;
            previousChar = currentChar;

            continue;
          }

          if (!withinString && currentChar == CsvOptions.Delimiter)
          {
            currentValues.Add(getAndResetCurrentValue());
          }
          else if (!withinString && _LineBreakChars.Contains(currentChar))
          {
            if (currentChar == '\r' && peek() == '\n')
              moveNext();
            if (previousChar != null)
              currentValues.Add(getAndResetCurrentValue());

            yield return currentValues.AsReadOnly();
            maxValuesCount = Math.Max(maxValuesCount, currentValues.Count);
            currentValues = new List<string>(maxValuesCount);
          }
          else
          {
            currentValue.Append(currentChar);
          }
          previousChar = currentChar;
        }
      }

      if (maxValuesCount == 0 || (previousChar == null || _LineBreakChars.Contains(previousChar.Value)))
        yield break;
      currentValues.Add(currentValue.ToString());
      yield return currentValues.AsReadOnly();
    }
  }
}