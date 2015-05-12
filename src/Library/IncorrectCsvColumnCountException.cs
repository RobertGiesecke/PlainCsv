using System;
using System.Diagnostics;
using System.Linq;
#if ReadOnlyDictionary
using ReadOnlyStrings = System.Collections.Generic.IReadOnlyList<string>;
#else
using ReadOnlyStrings = System.Collections.ObjectModel.ReadOnlyCollection<string>;
#endif

namespace RGiesecke.PlainCsv
{
  [DebuggerDisplay("RowIndex: {RowIndex}, CurrentValues: {CurrentValues.Count}, HeaderNames: {HeaderNames.Count}")]
  public class IncorrectCsvColumnCountException : InvalidOperationException
  {
    public int RowIndex { get; private set; }
    public ReadOnlyStrings CurrentValues { get; private set; }
    public ReadOnlyStrings HeaderNames { get; private set; }

    private string _Message;
    readonly object _MessageLock = new object();

    public override string Message
    {
      get
      {
        lock (_MessageLock)
          return _Message ?? (_Message = GetMessage());
      }
    }

    protected virtual string GetMessage()
    {
      return string.Format("Row {0}: The row value count ({1}) does not equal the head count ({2}).\n{3}\n{4}",
        RowIndex,
        CurrentValues.Count, 
        HeaderNames.Count, 
        string.Join(",", HeaderNames.ToArray()),
        string.Join(",", CurrentValues.ToArray()));
    }

    public IncorrectCsvColumnCountException(int rowIndex, ReadOnlyStrings currentValues, ReadOnlyStrings headerNames)
    {
      RowIndex = rowIndex;
      CurrentValues = currentValues;
      HeaderNames = headerNames;
    }
  }
}