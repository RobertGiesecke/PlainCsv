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
using RGiesecke.PlainCsv.Core;

namespace RGiesecke.PlainCsv.Tests
{
  [TestFixture(), ExcludeFromCodeCoverage]
  public class PlainCsvWriterTests
  {

    OrderedDictionary FromTyped<TKey, TValue>(OrderedDictionary<TKey, TValue> orderedDictionary)
    {
      return new OrderedDictionary(orderedDictionary.Select(t => new DictionaryEntry(t.Key, t.Value)),
                                   WrappedGenericEqualityComparer.FromTyped(orderedDictionary.KeyComparer));
    }


    [Test]
    public void DictionariesToCsv_With_AssumeFixedColumnCount_Throws_For_Inconsitent_Columns()
    {
      var writer = new PlainCsvWriter(new CsvWriterOptions(CsvWriterOptions.Default, assumeFixedColumnCount: true));
      var unmatchedColumnCounts = new[]
      {
        new CsvDictionary<object>
        {
          {"a", 1},
          {"b", 2},
        },
        new CsvDictionary<object>
        {
          {"a", 1},
          {"c", 3},
        },
      };
      var matchedColumnCounts = new[]
      {
        new CsvDictionary<object>
        {
          {"a", 1},
          {"b", 2},
        },
        new CsvDictionary<object>
        {
          {"a", 1},
          {"B", 3}, // the same using OrdinalIgnoreCase
        },
      };

      try
      {
        writer.DictionariesToCsvString(unmatchedColumnCounts);
        Assert.Fail();
      }
      catch (IncorrectCsvColumnCountException)
      {
      }

      try
      {
        writer.DictionariesToCsvString(matchedColumnCounts, StringComparer.OrdinalIgnoreCase);
      }
      catch (IncorrectCsvColumnCountException ex)
      {
        Assert.Fail(ex.Message);
      }

      int selectCount = 0;

      var uniqueRows = matchedColumnCounts.Take(0).ToList();

      writer.DictionariesToCsvString(matchedColumnCounts.Select(t =>
      {
        selectCount += 1;
        lock (uniqueRows)
          if (!uniqueRows.Contains(t))
            uniqueRows.Add(t);
        return t;
      }), StringComparer.OrdinalIgnoreCase);

      Assert.That(selectCount, Is.LessThanOrEqualTo(matchedColumnCounts.Length), string.Format("DictionariesToCsv enumerated the list of rows more than once."));
      Assert.That(uniqueRows.Count, Is.EqualTo(matchedColumnCounts.Length), string.Format("DictionariesToCsv did not read all rows."));
    }


    [Test()]
    public void DictionariesToCsv_Write_Example_Merged_ColumnOrder()
    {
      var writer = new PlainCsvWriter(new CsvWriterOptions(CsvWriterOptions.Default, delimiter: '@'));
      var sb = writer.DictionariesToCsvString(new[]
      {
        new OrderedDictionary // I use this class to ensure the column order
        {
          {"a@a", 1},
          {"b", new DateTime(2009, 1, 20, 15, 2, 0)},
        },
        new OrderedDictionary
        {
          {"y", 23.440m},
          {"b", DateTime.MinValue.AddHours(16).AddMinutes(5).AddSeconds(19)},
        },
      });


      using (var r = new StringReader(sb.ToString()))
      {
        var headerRow = r.ReadLine();
        var rows = new[]
        {
          r.ReadLine(),
          r.ReadLine(),
        };

        Assert.That(headerRow, Is.EqualTo("\"a@a\"@b@y")); // first name is quoted
        Assert.That(rows[0], Is.EqualTo("1@2009-01-20T15:02:00@"));
        Assert.That(rows[1], Is.EqualTo("@T16:05:19@23.44")); // date-part is considered unkown
      }
    }

    [Test()]
    public void DictionariesToCsv_Write_Example_SortedColumns()
    {
      var writer = new PlainCsvWriter(new CsvWriterOptions(CsvWriterOptions.Default,
                                                            delimiter: '#',
                                                            quoteChar: '|',
                                                            sortedColumnNames: new[]
                                                      {
                                                        "y",
                                                        "a#a",
                                                        "b",
                                                        "xyz" // names in this list can be missing from actual data
                                                      }));
      var sb = writer.DictionariesToCsvString(new IDictionary<string, object>[]
      {
        new Dictionary<string, object>
        {
          {"a#a", 1},
          {"b", new DateTime(2009, 1, 20, 15, 2, 0)},
        },
        new Dictionary<string, object>
        {
          {"y", 23.440m},
          {"b", DateTime.MinValue.AddHours(16).AddMinutes(5).AddSeconds(19)},
        },
      });


      using (var r = new StringReader(sb.ToString()))
      {
        var headerRow = r.ReadLine();
        var rows = new[]
        {
          r.ReadLine(),
          r.ReadLine(),
        };

        Assert.That(headerRow, Is.EqualTo("y#|a#a|#b")); // according to sortedColumnNames y is first, b is last
        Assert.That(rows[0], Is.EqualTo("#1#2009-01-20T15:02:00"));
        Assert.That(rows[1], Is.EqualTo("23.44##T16:05:19")); // date-part is considered unkown
      }
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
    public void DictionariesToCsv_Uses_SortColumnNames()
    {
      Action<IEnumerable<string>, Action<StringReader>> m = (sortedColumnNames, assertions) =>
      {
        var target = new PlainCsvWriter(new CsvWriterOptions(sortedColumnNames: sortedColumnNames));
        var sb = target.DictionariesToCsvString(new []
        {
          new CsvDictionary<object>
          {
            {"a", 1},
            {"c", 3},
          },
          new CsvDictionary<object>
          {
            {"a", 1},
            {"b", 2},
          },
        });

        using (var r = new StringReader(sb.ToString()))
        {
          assertions(r);
        }
      };

      m(new[] { "a", "b", "c" }, r =>
      {
        var headerRow = r.ReadLine();
        var rows = new[]
          {
            r.ReadLine(),
            r.ReadLine(),
          };
        Assert.That(headerRow, Is.EqualTo("a,b,c"));
        Assert.That(rows[0], Is.EqualTo("1,,3"));
        Assert.That(rows[1], Is.EqualTo("1,2,"));
      });

      m(new[] { "d", "b", "a", "c" }, r =>
      {
        var headerRow = r.ReadLine();
        var rows = new[]
          {
            r.ReadLine(),
            r.ReadLine(),
          };
        Assert.That(headerRow, Is.EqualTo("b,a,c"));
        Assert.That(rows[0], Is.EqualTo(",1,3"));
        Assert.That(rows[1], Is.EqualTo("2,1,"));
      });
    }


    [Test()]
    public void DictionariesToCsv_QuoteFormulars_Handles_Starting_EqualsSign()
    {
      var target = new PlainCsvWriter(new CsvWriterOptions(csvFlags: CsvFlags.QuoteFormulars | CsvFlags.UseHeaderRow));
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
