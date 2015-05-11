using System.Collections;
using System.Collections.Generic;

namespace RGiesecke.PlainCsv.Core
{
  public class OrderedDictionaryEnumerator<TKey, TValue> : IEnumerator<DictionaryEntry>, IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
  {
    private readonly IEnumerator<KeyValuePair<TKey, TValue>> _Enumerator;

    public OrderedDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
    {
      _Enumerator = enumerator;
    }

    public bool MoveNext()
    {
      var moveNext = _Enumerator.MoveNext();
      if (!moveNext)
      {
        _Entry.Key = null;
        _Entry.Value = null;
      }
      else
      {
        var kv = _Enumerator.Current;
        _Entry.Key = kv.Key;
        _Entry.Value = kv.Value;
      }
      return moveNext;
    }

    public void Reset()
    {
      _Enumerator.Reset();
    }

    DictionaryEntry IEnumerator<DictionaryEntry>.Current
    {
      get { return _Entry; }
    }

    KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current
    {
      get { return _Enumerator.Current; }
    }

    object IEnumerator.Current
    {
      get { return Entry; }
    }

    object IDictionaryEnumerator.Key { get { return _Enumerator.Current.Key; } }
    object IDictionaryEnumerator.Value { get { return _Enumerator.Current.Value; } }

    private DictionaryEntry _Entry;
    public DictionaryEntry Entry { get { return _Entry; } }

    public void Dispose()
    {
      _Enumerator.Dispose();
    }
  }
}