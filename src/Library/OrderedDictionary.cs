using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RGiesecke.PlainCsv
{

  /// <summary>
  /// A dictionary that keeps the elements in their original order
  /// </summary>
  public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
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
      return _Keys.Select(k => new KeyValuePair<TKey, TValue>(k, _Dictionary[k])).GetEnumerator();
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

    public void Clear()
    {
      _Keys.Clear();
      _Dictionary.Clear();
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

    public int Count
    {
      get { return _Dictionary.Count; }
    }

    public bool IsReadOnly
    {
      get { return _Dictionary.IsReadOnly; }
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

    public ICollection<TKey> Keys
    {
      get { return _Keys; }
    }

    public ICollection<TValue> Values
    {
      get { return _Dictionary.Values; }
    }
  }
}