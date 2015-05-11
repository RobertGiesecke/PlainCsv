using System.Collections;
using System.Collections.Generic;

namespace RGiesecke.PlainCsv.Core
{
  public class WrappedEqualityComparer : IEqualityComparer<object>
  {
    private readonly IEqualityComparer _Comparer;
    public virtual bool Equals(object x, object y)
    {
      return _Comparer.Equals(x, y);
    }

    public virtual int GetHashCode(object obj)
    {
      return _Comparer.GetHashCode(obj);
    }

    public static IEqualityComparer<object> FromUntyped(IEqualityComparer comparer)
    {
      if (comparer == null)
        return null;
      return new WrappedEqualityComparer(comparer);
    }

    public WrappedEqualityComparer(IEqualityComparer comparer)
    {
      _Comparer = comparer;
    }
  }

  public static class WrappedGenericEqualityComparer
  {
    public static IEqualityComparer FromTyped<T>(IEqualityComparer<T> comparer)
    {
      if (comparer == null)
        return null;
      return new WrappedGenericEqualityComparer<T>(comparer);
    }
  }

  public class WrappedGenericEqualityComparer<T> : IEqualityComparer
  {
    private readonly IEqualityComparer<T> _Comparer;
    public virtual bool Equals(object x, object y)
    {
      return _Comparer.Equals((T)x, (T)y);
    }

    public virtual int GetHashCode(object obj)
    {
      return _Comparer.GetHashCode((T)obj);
    }

    public WrappedGenericEqualityComparer(IEqualityComparer<T> comparer)
    {
      _Comparer = comparer;
    }
  }
}