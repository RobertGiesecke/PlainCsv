using System;
using System.Globalization;
using System.IO;
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

    public static readonly CultureInfo PersistedCultureInfo = GetPersistedCultureInfo();

    private static CultureInfo GetPersistedCultureInfo()
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

    public static DateTime? ParseDateTime(string text, CultureInfo cultureInfo = null)
    {
      if (text.IsNullOrWhiteSpace())
        return null;

      if (!PrepareDateString(text, out text))
        return null;

      return DateTime.Parse(text, cultureInfo ?? PersistedCultureInfo, DateTimeStyles.NoCurrentDateDefault);
    }

    private static bool PrepareDateString(string text, out string result)
    {
      if (text.IsNullOrWhiteSpace())
      {
        result = text;
        return false;
      }

      result = text.Trim();
      if (result.StartsWith("T"))
        result = "0001-01-01" + result;
      else if (result.EndsWith("T"))
      {
        result = result.Remove(result.Length - 1);
        if (result.IsNullOrWhiteSpace())
        {
          result = text;
          return false;
        }
      }
      return true;
    }

    public static bool TryParseDateTime(string text, out DateTime? value)
    {
      return TryParseDateTime(text, PersistedCultureInfo, out value);
    }

    public static bool TryParseDateTime(string text, CultureInfo cultureInfo, out DateTime? value)
    {
      if (!PrepareDateString(text, out text) || text.IsNullOrWhiteSpace())
      {
        value = null;
        return false;
      }

      DateTime dt;
      var result = DateTime.TryParse(text, cultureInfo, DateTimeStyles.NoCurrentDateDefault, out dt);
      if (result)
        value = dt;
      else
        value = null;

      return result;
    }

    public static string ConvertToString(object value, CultureInfo cultureInfo)
    {
      return ConvertToString(value, CsvOptions.Default.CsvFlags, cultureInfo);
    }

    public static string ConvertToString(object value, CsvFlags csvFlags, CultureInfo cultureInfo)
    {
      if (value == null)
        return null;

      var asDt = value as DateTime?;
      if (asDt != null)
      {
        DateTime dateTime = asDt.Value;
        if (dateTime != DateTime.MinValue)
        {
          var useIso8601Dates = (csvFlags & CsvFlags.Iso8601Dates) == CsvFlags.Iso8601Dates;

          if (dateTime.Date == DateTime.MinValue)
          {
            var result = dateTime.ToString("T", cultureInfo);
            if (useIso8601Dates)
              result = "T" + result;
            return result;
          }
          if (dateTime.TimeOfDay == TimeSpan.Zero)
            return dateTime.ToString(useIso8601Dates && !HasIso8601DatePattern(cultureInfo) 
                                       ? PersistedCultureInfo.DateTimeFormat.ShortDatePattern
                                       : "d", 
                                     cultureInfo);

          if (useIso8601Dates)
            return dateTime.ToString(dateTime.Kind == DateTimeKind.Local ? "O" : "s", cultureInfo);
        }
      }

      var asDecimal = value as decimal?;
      if (asDecimal != null)
        return asDecimal.Value.ToString("0.################################", cultureInfo);

      return Convert.ToString(value, cultureInfo);
    }

    private static bool HasIso8601DatePattern(CultureInfo cultureInfo)
    {
      var shortDatePattern = cultureInfo.DateTimeFormat.ShortDatePattern;
      return shortDatePattern == "yyyy'-'MM'-'dd" || 
              (shortDatePattern == "yyyy-MM-dd" && 
               cultureInfo.DateTimeFormat.DateSeparator == "-");
    }
  }
}