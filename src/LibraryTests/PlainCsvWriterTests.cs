﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGiesecke.PlainCsv;
using NUnit.Framework;
using RGiesecke.PlainCsv.Core;

namespace RGiesecke.PlainCsv.Tests
{
  [TestFixture(), ExcludeFromCodeCoverage]
  public class PlainCsvWriterTests
  {

    OrderedDictionary FromTyped<TKey, TValue>(OrderedDictionary<TKey, TValue> orderedDictionary)
    {
      return new OrderedDictionary(orderedDictionary.Select(t => new DictionaryEntry(t.Key, t.Value)), WrappedGenericEqualityComparer.FromTyped(orderedDictionary.KeyComparer));
    }

    [Test()]
    public void DictionariesToCsv_Handles_HashTable()
    {
      Action<bool> runCaseSensitive = i =>
      {
        var target = new PlainCsvWriter();
        var PlainTestData = GetPlainTestData().Select(FromTyped);
        var sb = target.DictionariesToCsvString(PlainTestData,
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
        var PlainTestData = GetPlainTestData().Select(FromTyped);
        var sb = target.DictionariesToCsvString(PlainTestData, i ? StringComparer.OrdinalIgnoreCase : null);
        using (var r = new StringReader(sb.ToString()))
          AssertPlainTestData(r, i);
      };

      runCaseSensitive(true);
      runCaseSensitive(false);
    }


    [Test()]
    public void DictionariesToCsv_QuoteFormulars_Handles_Starting_EqualsSign()
    {
      var target = new PlainCsvWriter(new CsvWriterOptions(CsvOptions.Default, csvFlags: CsvFlags.QuoteFormulars | CsvFlags.UseHeaderRow));
      string line;
      IEnumerable<IDictionary<string, object>> PlainTestData = GetPlainTestData();
      var sb = target.DictionariesToCsvString(PlainTestData, StringComparer.OrdinalIgnoreCase);
      using (var r = new StringReader(sb.ToString()))
      {
        r.ReadLine();
        line = r.ReadLine();
      }
      Assert.That(line, Is.EqualTo("\"=\"\"=1\"\"\",2.98,,"));

    }

    protected void AssertPlainTestData(TextReader reader, bool ignoreHeaderCase)
    {
      var headerRow = reader.ReadLine();
      var firstLine = reader.ReadLine();
      var secondLine = reader.ReadLine();
      var thirdLine = reader.ReadLine();
      var forthLine = reader.ReadLine();

      if (ignoreHeaderCase)
        Assert.That(headerRow, Is.EqualTo("a,\"b\"\"X\",D,c"));
      else
        Assert.That(headerRow, Is.EqualTo("a,\"b\"\"X\",D,c,\"B\"\"x\""));
      var suffix = ignoreHeaderCase ? "" : ",";
      Assert.That(firstLine, Is.EqualTo("=1,2.98,," + suffix));
      Assert.That(secondLine, Is.EqualTo("-727,,,1000" + suffix));

      if (ignoreHeaderCase)
        Assert.That(thirdLine, Is.EqualTo(",1922-03-07,,3.2"));
      else
        Assert.That(thirdLine, Is.EqualTo(",," + suffix + "3.2,1922-03-07"));

      if (ignoreHeaderCase)
        Assert.That(forthLine, Is.EqualTo(",1922-03-07T18:02:43,,"));
      else
        Assert.That(forthLine, Is.EqualTo(",," + suffix + ",1922-03-07T18:02:43"));

      Assert.That(reader.Peek(), Is.LessThan(0));
    }

    protected OrderedDictionary<string, object>[] GetPlainTestData()
    {
      return new[]
      {
        new OrderedDictionary<string, object>
        {
          {"a", "=1"},
          {"b\"X", 2.98m},
          {"D", ""},
        },
        new OrderedDictionary<string, object>
        {
          {"a", -727},
          {"c", 1000m},
        },
        new OrderedDictionary<string, object>
        {
          {"B\"x", new DateTime(1922, 3, 7)},
          {"c", 3.2f},
        },
        new OrderedDictionary<string, object>
        {
          {"B\"x", new DateTime(1922, 3, 7, 18, 02, 43)},
        },
      };
    }
  }
}
