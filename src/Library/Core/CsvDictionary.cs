using System.Collections.Generic;

namespace RGiesecke.PlainCsv.Core
{
  public class CsvDictionary<TValue> : OrderedDictionary<string, TValue>
  {
    public CsvDictionary()
    {
    }

    public CsvDictionary(IEqualityComparer<string> keyComparer)
      : base(keyComparer)
    {
    }

    public CsvDictionary(IEnumerable<string> keys, IDictionary<string, TValue> dictionary)
      : base(keys, dictionary)
    {
    }

    public CsvDictionary(IEnumerable<string> keys, IDictionary<string, TValue> dictionary, IEqualityComparer<string> keyComparer)
      : base(keys, dictionary, keyComparer)
    {
    }
  }
}