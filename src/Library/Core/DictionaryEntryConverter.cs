using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RGiesecke.PlainCsv.Core
{
  public class DictionaryEntryConverter : IDictionaryEntryConverter
  {
    public static readonly DictionaryEntryConverter Default = new DictionaryEntryConverter(new Dictionary<TypePair, Func<object, KeyValuePair<object, object>>>());
    private readonly IDictionary<TypePair, Func<object, KeyValuePair<object, object>>> _EntryConvertersCache;
    
    public DictionaryEntryConverter()
      : this(Default._EntryConvertersCache)
    {
    }

    public DictionaryEntryConverter(IDictionary<TypePair, Func<object, KeyValuePair<object, object>>> entryConvertersCache)
    {
      _EntryConvertersCache = entryConvertersCache;
    }


    public virtual DictionaryWrapper<TKey, TValue> WrapDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> keyComparer)
    {
      if (keyComparer != null)
      {
        var d = keyValuePairs as Dictionary<TKey, TValue>;
        if (d != null && !Equals(d.Comparer, keyComparer))
        {
          var list = keyValuePairs.ToList();
          var od = new OrderedDictionary<TKey, TValue>(
            list.Select(t => t.Key),
            list.ToDictionary(k => k.Key, v => v.Value, keyComparer),
            keyComparer);
          keyValuePairs = od;
        }
      }

#if ReadOnlyDictionary
      var rd = keyValuePairs as IReadOnlyDictionary<TKey, TValue>;
      if (rd != null)
        return new DictionaryWrapper<TKey, TValue>(rd.Keys, rd.Count, rd.TryGetValue);
#endif
      var dx = keyValuePairs as IDictionary<TKey, TValue> ?? keyValuePairs.ToDictionary(k => k.Key, v => v.Value, keyComparer);
      return new DictionaryWrapper<TKey, TValue>(dx.Keys.AsEnumerable(), dx.Count, dx.TryGetValue);
    }

    private static Func<object, KeyValuePair<object, object>> FromTyped<TKey, TValue>()
    {
      if (typeof(TKey) == typeof(object) && typeof(TValue) == typeof(object))
        return e => (KeyValuePair<object, object>)e;

      return e =>
      {
        var de = (KeyValuePair<TKey, TValue>)e;
        return new KeyValuePair<object, object>(de.Key, de.Value);
      };
    }

    private static MethodInfo GetFromTypedMethod()
    {
      Expression<Func<object>> expression = () => FromTyped<string, string>();
      var mce = (MethodCallExpression)expression.Body;
      return mce.Method.GetGenericMethodDefinition();
    }

    private static readonly MethodInfo _FromTypedMethod = GetFromTypedMethod();

    public sealed class TypePair : IEquatable<TypePair>
    {
      public Type KeyType { get; private set; }
      public Type ValueType { get; private set; }

      public TypePair(Type keyType, Type valueType)
      {
        KeyType = keyType;
        ValueType = valueType;
      }

      public bool Equals(TypePair other)
      {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return KeyType == other.KeyType && ValueType == other.ValueType;
      }

      public override bool Equals(object obj)
      {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TypePair)obj);
      }

      public override int GetHashCode()
      {
        unchecked
        {
          return ((KeyType != null ? KeyType.GetHashCode() : 0) * 397) ^ (ValueType != null ? ValueType.GetHashCode() : 0);
        }
      }

      public static bool operator ==(TypePair left, TypePair right)
      {
        return Equals(left, right);
      }

      public static bool operator !=(TypePair left, TypePair right)
      {
        return !Equals(left, right);
      }
    }

    public Func<object, KeyValuePair<object, object>> FromUntyped(object dictionary)
    {
      if (dictionary == null) throw new ArgumentNullException("dictionary");
      var typePairs = (from t in dictionary.GetType().GetInterfaces()
                       where t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                       let args = t.GetGenericArguments()
                       select new TypePair(args[0], args[1])).SingleOrDefault();
      if (typePairs != null)
        return FromTyped(typePairs);

      var d = dictionary as IDictionary;
      if (d == null)
        throw new ArgumentOutOfRangeException("dictionary", String.Format("{0} is not assignable to IDictionary or IDictionary<,>.", dictionary.GetType()));

      return e =>
      {
        var de = (DictionaryEntry)e;
        return new KeyValuePair<object, object>(de.Key, de.Value);
      };
    }

    protected virtual Func<object, KeyValuePair<object, object>> FromTyped(TypePair typePairs)
    {
      lock (_EntryConvertersCache)
      {
        Func<object, KeyValuePair<object, object>> result;
        if (_EntryConvertersCache.TryGetValue(typePairs, out result))
          return result;

        var genericFromTyped = _FromTypedMethod.MakeGenericMethod(typePairs.KeyType, typePairs.ValueType);
        _EntryConvertersCache.Add(typePairs,
          result = (Func<object, KeyValuePair<object, object>>)genericFromTyped.Invoke(null, new object[0]));

        return result;
      }
    }
  }
}