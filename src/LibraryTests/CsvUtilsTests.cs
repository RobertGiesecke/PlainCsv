using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
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

      Action<string, CultureInfo, DateTime?> compare = (text, ci, expected) =>
      {
        var expectation = expected != null ? (IResolveConstraint)Is.EqualTo(expected) : Is.Null;
        Assert.That(CsvUtils.ParseDateTime(text, ci), expectation);
        var pass = expected != null;
        DateTime? result;
        Assert.That(CsvUtils.TryParseDateTime(text, ci, out result), Is.EqualTo(pass));
        Assert.That(result, expectation);
      };

      compare("1952-09-23", invariantCulture, new DateTime(1952, 9, 23));
      compare("1952-09-23 10:02", invariantCulture, new DateTime(1952, 9, 23, 10, 2, 0));

      compare("23.09.1952 14:02:12", de, new DateTime(1952, 9, 23, 14, 2, 12));
      compare("23.Okt.1952 14:02:12", de, new DateTime(1952, 10, 23, 14, 2, 12));
      compare("14:02:12", de, new DateTime(1, 1, 1, 14, 2, 12));
      compare("02:02:12 pm", en, new DateTime(1, 1, 1, 14, 2, 12));

      compare("", en, null);
      compare(" ", en, null);
      compare(null, en, null);

      Action<string, CultureInfo> triggerFormatException = (text, ci) =>
      {

        try
        {
          var r = CsvUtils.ParseDateTime(text, ci);
          Assert.Fail("Invalid texts should trigger a FormatException");
        }
        catch (FormatException)
        {
        }
      };

      triggerFormatException("sometext!", en);
      triggerFormatException("21.02.2010", en);
      triggerFormatException("11-22-2010", de);
      triggerFormatException("März-01-2010", en);
      triggerFormatException("01-Okt-2010", en);

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
