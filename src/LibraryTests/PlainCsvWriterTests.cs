using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGiesecke.PlainCsv;
using NUnit.Framework;
namespace RGiesecke.PlainCsv.Tests
{
  [TestFixture(), ExcludeFromCodeCoverage]
  public class PlainCsvWriterTests
  {
    [Test()]
    public void DictionariesToCsv_Merges_Columns_From_all_Entries()
    {
      var target = new PlainCsvWriter();
      using (var w = new StringWriter())
      {
        target.DictionariesToCsv(w, new[]
        {
          new Dictionary<string, decimal?>
          {
            {"a", 1},
            {"b", 2.98m},
          },
          new Dictionary<string, decimal?>
          {
            {"a", -727},
            {"c", 1000m},
          },
        });
        var asString = w.ToString();

        using (var r = new StringReader(asString))
        {
          var headerRow = r.ReadLine();
          var firstLine = r.ReadLine();
          var secondLine = r.ReadLine();
          Assert.That(headerRow, Is.EqualTo("a,b,c"));
          Assert.That(firstLine, Is.EqualTo("1,2.98,"));
          Assert.That(secondLine, Is.EqualTo("-727,,1000"));
          Assert.That(r.Peek(), Is.LessThan(0));
        }
      }
    }
  }
}
