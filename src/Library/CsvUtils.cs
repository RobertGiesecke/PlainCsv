using System;
using System.Globalization;
using System.Linq;

namespace RGiesecke.PlainCsv
{
  public static class CsvUtils
  {
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

    internal static bool IsNullOrWhiteSpace(this string text)
    {
#if ReadOnlyDictionary
      return string.IsNullOrWhiteSpace(text);
#else
      return string.IsNullOrEmpty(text) || text.All(Char.IsWhiteSpace);
#endif
    }

    public static DateTime? ParseDateTime(string text, CultureInfo cultureInfo)
    {
      if (text.IsNullOrWhiteSpace())
        return null;
      DateTime dt;
      if (!DateTime.TryParse(text, cultureInfo, DateTimeStyles.NoCurrentDateDefault, out dt))
        return null;
      return dt;
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