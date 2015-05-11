using System.Collections.Generic;

namespace RGiesecke.PlainCsv.Core
{
  public class DictionaryWrapper<TKey, TValue>
  {
    public delegate bool TryGetValueHandler(TKey key, out TValue value);

    private readonly IEnumerable<TKey> _Keys;
    private readonly int _Count;
    private readonly TryGetValueHandler _TryGetValue;

    public IEnumerable<TKey> Keys
    {
      get { return _Keys; }
    }

    public int Count
    {
      get { return _Count; }
    }

    public TryGetValueHandler TryGetValue
    {
      get { return _TryGetValue; }
    }

    public DictionaryWrapper(IEnumerable<TKey> keys, int count, TryGetValueHandler tryGetValue)
    {
      _Keys = keys;
      _Count = count;
      _TryGetValue = tryGetValue;
    }
  }
}