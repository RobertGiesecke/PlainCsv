using System.Globalization;

namespace RGiesecke.PlainCsv
{
  public abstract class PlainCsvBase
  {
    public PlainCsvBase(CsvOptions csvOptions = null)
    {
      CsvOptions = csvOptions ?? CsvOptions.Default;
    }

    protected virtual CultureInfo GetPersistentCultureInfo()
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

    public CsvOptions CsvOptions { get; private set; }
  }
}