using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RGiesecke.PlainCsv
{

  /// <summary>
  /// A dictionary that keeps the elements in their original order
  /// </summary>
  public class OrderedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
#if ReadOnlyDictionary
  , IReadOnlyDictionary<TKey, TValue>
#endif
  {
    private readonly ICollection<TKey> _Keys;
    private readonly IDictionary<TKey, TValue> _Dictionary;
    private readonly IEqualityComparer<TKey> _KeyComparer;

    public IEqualityComparer<TKey> KeyComparer
    {
      get { return _KeyComparer; }
    }

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

    protected virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumeratorCore()
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


    public ICollection<TValue> Values
    {
      get { return this.Select(t => t.Value).ToList(); }
    }
  }

  public class OrderedDictionary : IDictionary
  {
    readonly OrderedDictionary<object, object> _GenericDictionary;

    static OrderedDictionary<object, object> FromUntyped(IEnumerable<DictionaryEntry> entries, IEqualityComparer keyComparer = null)
    {
      var asList = entries.ToList();
      var c = WrappedEqualityComparer.FromUntyped(keyComparer);
      var d = asList.ToDictionary(k => k.Key, v => v.Value, c);
      return new OrderedDictionary<object, object>(asList.Select(t => t.Key), d, c);
    }

    public OrderedDictionary(IEnumerable<DictionaryEntry> entries, IEqualityComparer keyComparer = null)
      : this(FromUntyped(entries, keyComparer))
    {
    }

    public OrderedDictionary(IEqualityComparer keyComparer = null)
      : this(new OrderedDictionary<object, object>(WrappedEqualityComparer.FromUntyped(keyComparer)))
    {
    }

    public OrderedDictionary(OrderedDictionary<object, object> typed)
    {
      if (typed == null) throw new ArgumentNullException("typed");
      _GenericDictionary = typed;
    }

    public bool Contains(object key)
    {
      return ((IDictionary<object, object>)_GenericDictionary).ContainsKey(key);
    }

    public void Add(object key, object value)
    {
      ((IDictionary<object, object>)_GenericDictionary).Add(key, value);
    }

    public void Clear()
    {
      ((IDictionary<object, object>)_GenericDictionary).Clear();
    }

    protected virtual OrderedDictionaryEnumerator<object, object> GetEnumeratorCore()
    {
      return new OrderedDictionaryEnumerator<object, object>(((IDictionary<object, object>)_GenericDictionary).GetEnumerator());
    }

    public IDictionaryEnumerator GetEnumerator()
    {
      return GetEnumeratorCore();
    }

    public void Remove(object key)
    {
      ((IDictionary<object, object>)_GenericDictionary).Remove(key);
    }

    public object this[object key]
    {
      get { return ((IDictionary<object, object>)_GenericDictionary)[key]; }
      set { ((IDictionary<object, object>)_GenericDictionary)[key] = value; }
    }

    public ICollection Keys
    {
      get { return (ICollection)((IDictionary<object, object>)_GenericDictionary).Keys; }
    }

    public ICollection Values
    {
      get { return (ICollection)((IDictionary<object, object>)_GenericDictionary).Values; }
    }

    public bool IsReadOnly
    {
      get { return ((IDictionary<object, object>)_GenericDictionary).IsReadOnly; }
    }

    public bool IsFixedSize
    {
      get { return false; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void CopyTo(Array array, int index)
    {
      this.Cast<DictionaryEntry>().ToArray().CopyTo(array, index);
    }

    public int Count
    {
      get { return ((IDictionary<object, object>)_GenericDictionary).Count; }
    }

    public object SyncRoot
    {
      get { throw new NotImplementedException(); }
    }

    public bool IsSynchronized
    {
      get { return false; }
    }
  }
}