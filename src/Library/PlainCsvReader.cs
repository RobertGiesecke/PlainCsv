using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RGiesecke.PlainCsv.Core;
using System.Collections.ObjectModel;
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
    protected static readonly ICollection<char> LineBreakChars = new ReadOnlyCollection<char>(new[] { '\r', '\n' });

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
      if (CsvOptions.UseHeaderRow)
      {
        headerNames = rowsEnum.Current;
        if (!rowsEnum.MoveNext())
          yield break;
      }
      else
        headerNames = rowsEnum.Current.Select((n, i) => "Field" + (i + 1)).ToList().AsReadOnly();

      var rowIndex = 0;
      do
      {
        var currentValues = rowsEnum.Current;
        var d = new OrderedDictionary<string, string>(keyComparer);
        if (currentValues.Count != headerNames.Count)
          throw new IncorrectCsvColumnCountException(rowIndex, currentValues, headerNames);

        for (int i = 0; i < headerNames.Count; i++)
        {
          d.Add(headerNames[i], currentValues[i]);
        }
        yield return d;
        rowIndex += 1;
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
          else if (!withinString && LineBreakChars.Contains(currentChar))
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

      if (maxValuesCount == 0 || (previousChar == null || LineBreakChars.Contains(previousChar.Value)))
        yield break;
      currentValues.Add(currentValue.ToString());
      yield return currentValues.AsReadOnly();
    }
  }
}