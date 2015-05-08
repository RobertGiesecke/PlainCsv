using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGiesecke.PlainCsv;
using NUnit.Framework;
namespace RGiesecke.PlainCsv.Tests
{
  [TestFixture(), ExcludeFromCodeCoverage]
  public class CsvUtilsTests
  {
    [Test()]
    public void ConvertToStringTest()
    {
      var invariantCulture = CsvUtils.GetPersistedCultureInfo();
      var de = CultureInfo.GetCultureInfo("de");

      Assert.That(CsvUtils.ConvertToString(1.50m, invariantCulture), Is.EqualTo("1.5"));
      Assert.That(CsvUtils.ConvertToString(1.50m, de), Is.EqualTo("1,5"));
      Assert.That(CsvUtils.ConvertToString(1.00m, de), Is.EqualTo("1"));

      Assert.That(CsvUtils.ConvertToString(new DateTime(1952, 9, 23), invariantCulture), Is.EqualTo("1952-09-23"));
      Assert.That(CsvUtils.ConvertToString(new DateTime(1952, 9, 23, 9, 2, 0), invariantCulture), Is.EqualTo("1952-09-23 09:02:00"));
      Assert.That(CsvUtils.ConvertToString(new DateTime(1, 1, 1, 9, 2, 0), invariantCulture), Is.EqualTo("09:02:00"));
    }

    [Test()]
    public void ParseDateTimeTest()
    {
      var invariantCulture = CsvUtils.GetPersistedCultureInfo();
      var de = CultureInfo.GetCultureInfo("de");
      var en = CultureInfo.GetCultureInfo("en");

      Assert.That(CsvUtils.ParseDateTime("1952-09-23", invariantCulture), Is.EqualTo(new DateTime(1952, 9, 23)));
      Assert.That(CsvUtils.ParseDateTime("1952-09-23 10:02", invariantCulture), Is.EqualTo(new DateTime(1952, 9, 23, 10, 2, 0)));

      Assert.That(CsvUtils.ParseDateTime("23.09.1952 14:02:12", de), Is.EqualTo(new DateTime(1952, 9, 23, 14, 2, 12)));
      Assert.That(CsvUtils.ParseDateTime("14:02:12", de), Is.EqualTo(new DateTime(1, 1, 1, 14, 2, 12)));
      Assert.That(CsvUtils.ParseDateTime("02:02:12 pm", en), Is.EqualTo(new DateTime(1, 1, 1, 14, 2, 12)));

      Assert.That(CsvUtils.ParseDateTime("", en), Is.Null);
      Assert.That(CsvUtils.ParseDateTime(" ", en), Is.Null);
      Assert.That(CsvUtils.ParseDateTime(null, en), Is.Null);
      Assert.That(CsvUtils.ParseDateTime("sometext!", en), Is.Null);
    }

    [Test()]
    public void QuoteText_Escapes_QuoteChar()
    {
      Assert.That(CsvUtils.QuoteText("abc"), Is.EqualTo("\"abc\""));
      Assert.That(CsvUtils.QuoteText(""), Is.EqualTo("\"\""));
      Assert.That(CsvUtils.QuoteText(null), Is.EqualTo("\"\""));
      Assert.That(CsvUtils.QuoteText("a b \tc"), Is.EqualTo("\"a b \tc\""));
      Assert.That(CsvUtils.QuoteText("a b \tc", '\t'), Is.EqualTo("\ta b \t\tc\t"));
    }
  }
}
