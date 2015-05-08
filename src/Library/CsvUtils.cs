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

    public static CultureInfo GetPersistedCultureInfo()
    {
      var cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
      var df = cultureInfo.DateTimeFormat;
      df.LongDatePattern = "s";
      df.ShortDatePattern = "yyyy'-'MM'-'dd";
      df.DateSeparator = "-";
      df.TimeSeparator = ":";
      df.ShortTimePattern = "HH':'mm':'ss";
      df.FullDateTimePattern = "s";
      return cultureInfo;
    }

    public static string ConvertToString(object value, CultureInfo cultureInfo)
    {
      if (value == null)
        return null;

      var asDt = value as DateTime?;
      if (asDt != null)
      {
        DateTime dateTime = asDt.Value;
        if (dateTime != DateTime.MinValue)
        {
          if (dateTime.Date == DateTime.MinValue)
            return dateTime.ToString("T", cultureInfo);
          if (dateTime.TimeOfDay == TimeSpan.Zero)
            return dateTime.ToString("d", cultureInfo);
        }
      }

      var asDecimal = value as decimal?;
      if (asDecimal != null)
        return asDecimal.Value.ToString("0.################################", cultureInfo);

      return Convert.ToString(value, cultureInfo);
    }
  }
}