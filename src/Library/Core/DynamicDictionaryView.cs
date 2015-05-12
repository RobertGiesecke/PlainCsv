#if DynamicObject

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace RGiesecke.PlainCsv.Core
{
  public class DynamicDictionaryView<TValue> : DynamicObject
  {
    private readonly IDictionary<string, TValue> _Dictionary;

    protected IDictionary<string, TValue> Dictionary
    {
      get { return _Dictionary; }
    }

    public DynamicDictionaryView(IDictionary<string, TValue> dictionary)
    {
      if (dictionary == null) throw new ArgumentNullException("dictionary");
      _Dictionary = dictionary;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      TValue value;
      var found = _Dictionary.TryGetValue(binder.Name, out value);
      result = value;
      return found;
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
      _Dictionary[binder.Name] = (TValue)value;
      return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
      return _Dictionary.Keys;
    }
  }
}

#endif