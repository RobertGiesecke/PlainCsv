using System;

namespace RGiesecke.PlainCsv
{
  public static class CsvUtils
  {

    public static Func<TResult> Lambda<TResult>(Func<TResult> lambda)
    {
      return lambda;
    }

    public static Func<TValue1, TResult> Lambda<TValue1, TResult>(Func<TValue1, TResult> lambda)
    {
      return lambda;
    }

    public static Func<TValue1, TValue2, TResult> Lambda<TValue1, TValue2, TResult>(Func<TValue1, TValue2, TResult> lambda)
    {
      return lambda;
    }

    public static string QuoteText(string text, char quoteChar = '"')
    {
      var asString = quoteChar.ToString();
      if (String.IsNullOrEmpty(text))
        return asString + asString;

      return asString + text.Replace(asString, asString + asString) + asString;
    }
  }
}