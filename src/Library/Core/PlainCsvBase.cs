using System.Globalization;

namespace RGiesecke.PlainCsv.Core
{
  public abstract class PlainCsvBase<TOptions>
  where TOptions : CsvOptions
  {
    public PlainCsvBase(TOptions csvOptions = null)
    {
      CsvOptions = csvOptions ?? GetDefaultOptions();
    }

    protected abstract TOptions GetDefaultOptions();

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

    public TOptions CsvOptions { get; private set; }
  }
}