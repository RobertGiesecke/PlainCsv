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
  [TestFixture(), ExcludeFromCodeCoverage]
  public class CsvWriterOptionsTests : CsvOptionsTestsBase
  {
    [Test()]
    public void CsvWriterOptions_Defaults_To_Csv_Spec()
    {
      AssertThatOptionsAreCsv(CsvWriterOptions.Default);
      AssertThatOptionsAreCsv(CsvWriterOptions.Excel);
      AssertThatOptionsAreCsv(new CsvWriterOptions());
    }

    [Test()]
    public void CsvWriterOptions_Throws_AOORE_For_Same_QuoteChar_And_Delimiter()
    {
      AssertThatSameQuoteCharAndDelimiterThrows((q, d) => new CsvWriterOptions(quoteChar: q, delimiter: d));
    }
  }
}
