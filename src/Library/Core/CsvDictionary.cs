using System.Collections.Generic;

namespace RGiesecke.PlainCsv.Core
{
  public class CsvDictionary<TValue> : OrderedDictionary<string, TValue>
  {
#if DynamicObject
private readonly DynamicDictionaryView<TValue> _DynamicView;

    public dynamic DynamicView
    {
      get { return _DynamicView; }
    }
#endif
    public CsvDictionary()
    {
#if DynamicObject
      _DynamicView = new DynamicDictionaryView<TValue>(this);
#endif
    }

    public CsvDictionary(IEqualityComparer<string> keyComparer)
      : base(keyComparer)
    {
#if DynamicObject
      _DynamicView = new DynamicDictionaryView<TValue>(this);
#endif
    }

    public CsvDictionary(IEnumerable<string> keys, IDictionary<string, TValue> dictionary)
      : base(keys, dictionary)
    {
#if DynamicObject
      _DynamicView = new DynamicDictionaryView<TValue>(this);
#endif
    }

    public CsvDictionary(IEnumerable<string> keys, IDictionary<string, TValue> dictionary, IEqualityComparer<string> keyComparer)
      : base(keys, dictionary, keyComparer)
    {
#if DynamicObject
      _DynamicView = new DynamicDictionaryView<TValue>(this);
#endif
    }
  }
}