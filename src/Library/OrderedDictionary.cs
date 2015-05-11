using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RGiesecke.PlainCsv
{

  /// <summary>
  /// A dictionary that keeps the elements in their original order
  /// </summary>
  public class OrderedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IDictionary
#if ReadOnlyDictionary
  , IReadOnlyDictionary<TKey, TValue>
#endif
  {
    private readonly ICollection<TKey> _Keys;
    private readonly IDictionary<TKey, TValue> _Dictionary;
    private readonly IEqualityComparer<TKey> _KeyComparer;


    public OrderedDictionary()
      : this(EqualityComparer<TKey>.Default)
    {
    }

    public OrderedDictionary(IEqualityComparer<TKey> keyComparer)
      : this(new TKey[0], new Dictionary<TKey, TValue>(keyComparer), keyComparer)
    {
    }

    public OrderedDictionary(IEnumerable<TKey> keys, IDictionary<TKey, TValue> dictionary)
      : this(keys, dictionary, EqualityComparer<TKey>.Default)
    {
    }

    public OrderedDictionary(IEnumerable<TKey> keys, IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer)
    {
      if (keys == null) throw new ArgumentNullException("keys");
      if (dictionary == null) throw new ArgumentNullException("dictionary");
      _Keys = new HashSet<TKey>(keys, _KeyComparer);
      _Dictionary = dictionary;
      _KeyComparer = keyComparer;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return GetEnumeratorCore();
    }

    protected virtual OrderedDictionaryEnumerator GetEnumeratorCore()
    {
      return new OrderedDictionaryEnumerator(_Keys.Select(k => new KeyValuePair<TKey, TValue>(k, _Dictionary[k])).GetEnumerator());
    }

    void IDictionary.Remove(object key)
    {
      Remove((TKey)key);
    }

    object IDictionary.this[object key]
    {
      get { return this[(TKey)key]; }
      set { this[(TKey)key] = (TValue)value; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable)_Dictionary).GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      _Keys.Add(item.Key);
      _Dictionary.Add(item);
    }

    bool IDictionary.Contains(object key)
    {
      return ContainsKey((TKey)key);
    }

    void IDictionary.Add(object key, object value)
    {
      Add((TKey)key, (TValue)value);
    }

    public void Clear()
    {
      _Keys.Clear();
      _Dictionary.Clear();
    }

    protected class OrderedDictionaryEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>,IDictionaryEnumerator
    {
      private readonly IEnumerator<KeyValuePair<TKey, TValue>> _Enumerator;

      public OrderedDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>>  enumerator)
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

      public KeyValuePair<TKey, TValue> Current
      {
        get { return _Enumerator.Current; }
      }

      object IEnumerator.Current
      {
        get { return _Enumerator.Current; }
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

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return GetEnumeratorCore();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      return _Dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      _Dictionary.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      _Keys.Remove(item.Key);
      return _Dictionary.Remove(item);
    }

    void ICollection.CopyTo(Array array, int index)
    {
      this.ToArray().CopyTo(array, index);
    }

    public int Count
    {
      get { return _Dictionary.Count; }
    }

    object ICollection.SyncRoot
    {
      get { return null; }
    }

    bool ICollection.IsSynchronized
    {
      get { return false; }
    }

    ICollection IDictionary.Values
    {
      get { return (ICollection)Values; }
    }

    public bool IsReadOnly
    {
      get { return _Dictionary.IsReadOnly; }
    }

    bool IDictionary.IsFixedSize
    {
      get { return false; }
    }

    public bool ContainsKey(TKey key)
    {
      return _Dictionary.ContainsKey(key);
    }

    public void Add(TKey key, TValue value)
    {
      _Keys.Add(key);
      _Dictionary.Add(key, value);
    }

    public bool Remove(TKey key)
    {
      _Keys.Remove(key);
      return _Dictionary.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      return _Dictionary.TryGetValue(key, out value);
    }

    public TValue this[TKey key]
    {
      get { return _Dictionary[key]; }
      set { _Dictionary[key] = value; }
    }

#if ReadOnlyDictionary
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
    {
      get { return Keys; }
    }

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
    {
      get { return Values; }
    }
#endif
    public ICollection<TKey> Keys
    {
      get { return _Keys; }
    }

    ICollection IDictionary.Keys
    {
      get { return _Keys.ToList(); }
    }

    public ICollection<TValue> Values
    {
      get { return this.Select(t => t.Value).ToList(); }
    }
  }
}