using System;
using System.Collections;
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
    public void DictionariesToCsv_Handles_HashTable()
    {
      Action<bool> runCaseSensitive = i =>
      {
        var target = new PlainCsvWriter();
        var sb = target.DictionariesToCsvString(GetPlainTestData().Select(t => new Hashtable(t)), 
                                                i ? StringComparer.OrdinalIgnoreCase : null);
        using (var r = new StringReader(sb.ToString()))
          AssertPlainTestData(r, i);
      };

      runCaseSensitive(true);
      runCaseSensitive(false);
    }

    [Test()]
    public void DictionariesToCsv_Merges_Columns_From_all_Entries()
    {
      Action<bool> runCaseSensitive = i =>
      {
        var target = new PlainCsvWriter();
        var sb = target.DictionariesToCsvString(GetPlainTestData(), i ? StringComparer.OrdinalIgnoreCase : null);
        using (var r = new StringReader(sb.ToString()))
          AssertPlainTestData(r, i);
      };

      runCaseSensitive(true);
      runCaseSensitive(false);
    }

    protected void AssertPlainTestData(TextReader reader, bool ignoreHeaderCase)
    {
      var headerRow = reader.ReadLine();
      var firstLine = reader.ReadLine();
      var secondLine = reader.ReadLine();
      var thirdLine = reader.ReadLine();
      if (ignoreHeaderCase)
        Assert.That(headerRow, Is.EqualTo("a,b,c"));
      else
        Assert.That(headerRow, Is.EqualTo("a,b,c,B"));
      var suffix = ignoreHeaderCase ? "" : ",";
      Assert.That(firstLine, Is.EqualTo("1,2.98," + suffix));
      Assert.That(secondLine, Is.EqualTo("-727,,1000" + suffix));

      if (ignoreHeaderCase)
        Assert.That(thirdLine, Is.EqualTo(",1922-03-07,3.2"));
      else
        Assert.That(thirdLine, Is.EqualTo(suffix + ",3.2,1922-03-07"));

      Assert.That(reader.Peek(), Is.LessThan(0));
    }

    protected Dictionary<string, object>[] GetPlainTestData()
    {
      return new[]
      {
        new Dictionary<string, object>
        {
          {"a", 1},
          {"b", 2.98m},
        },
        new Dictionary<string, object>
        {
          {"a", -727},
          {"c", 1000m},
        },
        new Dictionary<string, object>
        {
          {"B", new DateTime(1922, 3, 7)},
          {"c", 3.2f},
        },
      };
    }
  }
}
