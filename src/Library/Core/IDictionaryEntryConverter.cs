using System;
using System.Collections.Generic;

namespace RGiesecke.PlainCsv.Core
{
  public interface IDictionaryEntryConverter
  {
    Func<object, KeyValuePair<object, object>> FromUntyped(object dictionary);
    DictionaryWrapper<TKey, TValue> WrapDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> keyComparer);
  }
}