using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGiesecke.PlainCsv;
using NUnit.Framework;
namespace RGiesecke.PlainCsv.Tests
{
  [TestFixture(),ExcludeFromCodeCoverage]
  public class CsvOptionsTests : CsvOptionsTestsBase
  {
    [Test()]
    public void CsvOptions_Defaults_To_Csv_Spec()
    {
      AssertThatOptionsAreCsv(CsvOptions.Default);
      AssertThatOptionsAreCsv(new CsvOptions());
      AssertThatOptionsAreCsv(CsvOptions.Excel);
    }

    [Test()]
    public void CsvOptions_Throws_AOORE_For_Same_QuoteChar_And_Delimiter()
    {
      AssertThatSameQuoteCharAndDelimiterThrows((q, d) => new CsvOptions(quoteChar: q, delimiter: d));
    }
  }
}
