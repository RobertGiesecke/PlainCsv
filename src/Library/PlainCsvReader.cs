﻿using System;
using System.Collections.Generic;
using System.IO;
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
  public class PlainCsvReader : PlainCsvBase<CsvOptions>
  {
    protected static readonly ICollection<char> LineBreakChars = new ReadOnlyCollection<char>(new[] { '\r', '\n' });

    public PlainCsvReader(CsvOptions csvOptions = null)
      : base(csvOptions)
    {
    }

    public IEnumerable<CsvDictionary> CsvToDictionaries(
      TextReader reader,
      IEqualityComparer<string> keyComparer = null)
    {
      return CsvToDictionaries(StreamToCharConverter.ReaderToEnumerable(reader), keyComparer);
    }

    public IEnumerable<CsvDictionary> CsvToDictionaries(
      Stream stream,
      IEqualityComparer<string> keyComparer = null)
    {
      return CsvToDictionaries(StreamToCharConverter.StreamToEnumerable(stream), keyComparer);
    }

    public IEnumerable<CsvDictionary> CsvToDictionaries(
      IEnumerable<char> characters,
      IEqualityComparer<string> keyComparer = null)
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
        var d = new CsvDictionary(keyComparer);
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

    public IEnumerable<ReadOnlyStrings> ReadCsvRows(TextReader reader)
    {
      return ReadCsvRows(StreamToCharConverter.ReaderToEnumerable(reader));
    }

    public IEnumerable<ReadOnlyStrings> ReadCsvRows(Stream stream)
    {
      return ReadCsvRows(StreamToCharConverter.StreamToEnumerable(stream));
    }

    public IEnumerable<ReadOnlyStrings> ReadCsvRows(IEnumerable<Char> characters)
    {
      var unquotedLineBreaks = (CsvOptions.CsvFlags & CsvFlags.UnQuotedLineBreaks) == CsvFlags.UnQuotedLineBreaks;
      var currentValues = new List<string>();

      var withinString = false;

      var currentValue = new StringBuilder();

      var charEnum = characters.GetEnumerator();
      Func<char?> peek;
      int? columnCount = null;
      ReadOnlyStrings firstRow = null;

      Func<IList<string>, bool> jumpToNewRow;
      Func<ReadOnlyStrings, ReadOnlyStrings> yieldRow;
      {
        if (!unquotedLineBreaks)
        {
          yieldRow = r => r;
          jumpToNewRow = cv => true;
        }
        else
        {
          jumpToNewRow = cv =>
          {
            var b = columnCount == null ||
                    cv.Count >= columnCount.Value - 1;
            return b;
          };
          yieldRow = r =>
          {
            if (columnCount == null)
            {
              if (firstRow == null)
                firstRow = r;
              columnCount = r.Count;
            }
            return r;
          };
        }
      }
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

      Func<bool, string> getAndResetCurrentValue = reset =>
      {
        try
        {
          return currentValue.Length > 0 ? currentValue.ToString() : null;
        }
        finally
        {
          if (reset)
            currentValue.Length = 0;
        }
      };

      char? previousChar = null;
      var maxValuesCount = 0;
      var rowIndex = 0;
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
            currentValues.Add(getAndResetCurrentValue(true));
          }
          else if (!withinString && LineBreakChars.Contains(currentChar) && jumpToNewRow(currentValues))
          {
            if (currentChar == '\r' && peek() == '\n')
              moveNext();
            if (previousChar != null)
              currentValues.Add(getAndResetCurrentValue(true));

            yield return yieldRow(currentValues.AsReadOnly());
            if (unquotedLineBreaks && maxValuesCount > 0 && currentValues.Count > maxValuesCount)
              throw new IncorrectCsvColumnCountException(rowIndex, currentValues.AsReadOnly(), firstRow);
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
      currentValues.Add(getAndResetCurrentValue(false));
      yield return yieldRow(currentValues.AsReadOnly());
    }

    protected override CsvOptions GetDefaultOptions()
    {
      return PlainCsv.CsvOptions.Default;
    }
  }
}