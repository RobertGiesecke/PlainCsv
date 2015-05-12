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
      return CsvUtils.PersistedCultureInfo;
    }

    public TOptions CsvOptions { get; private set; }

    protected virtual string ConvertToString(object value, CultureInfo cultureInfo = null)
    {
      cultureInfo = cultureInfo ?? GetPersistentCultureInfo();
      return CsvUtils.ConvertToString(value, CsvOptions.CsvFlags, cultureInfo);
    }
  }
}