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
  [TestFixture, ExcludeFromCodeCoverage]
  public class PlainCsvReaderTests
  {
    [Test]
    public void CsvToDictionaries_ReadExample()
    {
      var reader = new PlainCsvReader();
      var text = "a,b,c,d,e" +
                 "\n,2, d\t,\"y\ns\"," +
                 "\r\n1,2010-01-01,\"y\r\ns\",15:30, ";
      var result = reader.CsvToDictionaries(text).ToList();

      var firstRow = result[0];
      var secondRow = result[1];
      Assert.That(firstRow["a"], Is.EqualTo(null));
      Assert.That(firstRow["b"], Is.EqualTo("2"));
      Assert.That(firstRow["c"], Is.EqualTo(" d\t")); // leading space and trailing tab are preserved
      Assert.That(firstRow["d"], Is.EqualTo("y\ns")); // line feed is preserved
      Assert.That(firstRow["e"], Is.EqualTo(null));

      Assert.That(secondRow["a"], Is.EqualTo("1"));
      Assert.That(CsvUtils.ParseDateTime(secondRow["b"]), Is.EqualTo(new DateTime(2010, 1, 1)));
      Assert.That(secondRow["c"], Is.EqualTo("y\r\ns")); // cariage return and line feed are preserved
      Assert.That(CsvUtils.ParseDateTime(secondRow["d"]), Is.EqualTo(DateTime.MinValue.Add(new TimeSpan(15, 30, 0))));
      Assert.That(secondRow["e"], Is.EqualTo(" "));
    }

    [Test()]
    public void ReadCsvRows_Separates_By_NonString_LF_and_CR()
    {
      var target = new PlainCsvReader();

      Action<string> runWithLineDelimiter = l =>
      {
        var actual = target.ReadCsvRows("a,b,c" + l + "1,2, 3").ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0], Is.EqualTo(new[] { "a", "b", "c" }));
        Assert.That(actual[1], Is.EqualTo(new[] { "1", "2", " 3" }));

        actual = target.ReadCsvRows("a ,b" + l + " ,").ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0], Is.EqualTo(new[] { "a ", "b" }));
        Assert.That(actual[1], Is.EqualTo(new[] { " ", null }));
      };

      runWithLineDelimiter("\n");
      runWithLineDelimiter("\r");
      runWithLineDelimiter("\r\n");
    }

    [Test()]
    public void ReadCsvRows_Returns_Null_For_empty_Last_Value()
    {
      var target = new PlainCsvReader();

      Action<string> runWithLineDelimiter = l =>
      {
        var actual = target.ReadCsvRows("a,b,c,d" + l + "1,2, 3,").ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0], Is.EqualTo(new[] { "a", "b", "c", "d" }));
        Assert.That(actual[1], Is.EqualTo(new[] { "1", "2", " 3", null }));
      };

      runWithLineDelimiter("\n");
      runWithLineDelimiter("\r");
      runWithLineDelimiter("\r\n");
    }

    [Test()]
    public void ReadCsvRows_Returns_Null_for_Empty_Cells()
    {
      var target = new PlainCsvReader();

      Action<string> runWithLineDelimiter = l =>
      {
        var actual = target.ReadCsvRows("a,b,c" + l + "1,, ").ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0], Is.EqualTo(new[] { "a", "b", "c" }));
        Assert.That(actual[1], Is.EqualTo(new[] { "1", null, " " }));
        Assert.That(actual[1], Is.Not.EqualTo(new[] { "1", "", " " }));
      };

      runWithLineDelimiter("\n");
      runWithLineDelimiter("\r");
      runWithLineDelimiter("\r\n");
    }

    [Test()]
    public void ReadCsvRows_Wont_Split_For_Last_LineBreak()
    {
      var target = new PlainCsvReader();

      Action<string> runWithLineDelimiter = l =>
      {
        var actual = target.ReadCsvRows(string.Format("a,b,c{0}1,, {0}", l)).ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(2));
      };

      runWithLineDelimiter("\n");
      runWithLineDelimiter("\r");
      runWithLineDelimiter("\r\n");
    }

    [Test()]
    public void ReadCsvRows_Empty_List_For_Empty_Text()
    {
      var target = new PlainCsvReader();

      Action<string, int> runWithLineDelimiter = (text, count) =>
      {
        var actual = target.ReadCsvRows(text).ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(count));
      };

      runWithLineDelimiter("", 0);
      runWithLineDelimiter("\n", 1);
      runWithLineDelimiter("\r\n", 1);
    }

    [Test()]
    public void ReadCsvRows_Example()
    {
      var target = new PlainCsvReader();
      var actual = target.ReadCsvRows("a,b,\"c\na\"\nb c").ToList();
      Assert.That(actual.Count, Is.EqualTo(2));
      Assert.That(actual[0], Is.EqualTo(new[] { "a", "b", "c\na" }));
      Assert.That(actual[1], Is.EqualTo(new[] { "b c" }));
    }

    [Test()]
    public void ReadCsvRows_Doesnt_Separate_By_String_LF_and_CR()
    {
      var target = new PlainCsvReader();

      Action<string> runWithLineDelimiter = l =>
      {
        var actual = target.ReadCsvRows("a,b,\"c" + l + "\",s\n1,2, 3").ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0], Is.EqualTo(new[] { "a", "b", "c" + l, "s" }));
        Assert.That(actual[1], Is.EqualTo(new[] { "1", "2", " 3" }));
      };

      runWithLineDelimiter("\n");
      runWithLineDelimiter("\r");
      runWithLineDelimiter("\r\n");
    }

    [Test()]
    public void ReadCsvRows_Separates_By_Delimiter_And_Quote()
    {
      Action<string, char, char> runWithDelimiter = (l, d, q) =>
      {
        var target = new PlainCsvReader(new CsvOptions(q, d));
        var characters = string.Format("a{0}b{0}{1}c{2}{1}{0}s{2}1{0}2{0} 3", d, q, l);
        var actual = target.ReadCsvRows(characters).ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0], Is.EqualTo(new[] { "a", "b", "c" + l, "s" }));
        Assert.That(actual[1], Is.EqualTo(new[] { "1", "2", " 3" }));
      };

      runWithDelimiter("\n", ',', '"');
      runWithDelimiter("\r", '|', '~');
      runWithDelimiter("\r\n", ',', '-');
    }

    [Test()]
    public void CsvToDictionaries_Separates_By_Delimiter_And_Quote()
    {
      Action<string, char, char> runWithDelimiter = (l, d, q) =>
      {
        var target = new PlainCsvReader(new CsvOptions(q, d));
        var characters = string.Format("a{0}b{0}{1}c{2}{1}{0}s{2}1{0}2{0} 3{0}{1} {1}{1}{0}ä {1}", d, q, l);
        var actual = target.CsvToDictionaries(characters).ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(1));
        Assert.That(actual[0].Keys, Is.EqualTo(new[] { "a", "b", "c" + l, "s" }));
        Assert.That(actual[0].Values, Is.EqualTo(new[] { "1", "2", " 3", string.Format(" {1}{0}ä ", d, q) }));
      };

      runWithDelimiter("\n", ',', '"');
      runWithDelimiter("\r", '|', '~');
      runWithDelimiter("\r\n", ',', '-');
    }

    [Test()]
    public void CsvToDictionaries_Respects_UseHeaderRow()
    {
      Action<string, char, char> runWithDelimiter = (l, d, q) =>
      {
        var target = new PlainCsvReader(new CsvOptions(q, d, CsvFlags.None));
        var characters = string.Format("a{0}b{0}{1}c{2}{1}{0}s{2}1{0}2{0} 3{0}{1} {1}{1}{0}ä {1}", d, q, l);
        var actual = target.CsvToDictionaries(characters).ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0].Keys, Is.EqualTo(new[] { "Field1", "Field2", "Field3", "Field4" }));
        Assert.That(actual[0].Values, Is.EqualTo(new[] { "a", "b", "c" + l, "s" }));
        Assert.That(actual[1].Values, Is.EqualTo(new[] { "1", "2", " 3", string.Format(" {1}{0}ä ", d, q) }));
      };

      runWithDelimiter("\n", ',', '"');
      runWithDelimiter("\r", '|', '~');
      runWithDelimiter("\r\n", ',', '-');
    }

    [Test()]
    public void CsvToDictionaries_Throws_IncorrectCsvColumnCountException_for_Differencing_ColumnCounts()
    {
      Action<string, char, char, CsvFlags, int> runWithDelimiter = (l, d, q, f, r) =>
      {
        var target = new PlainCsvReader(new CsvOptions(q, d, f));
        var characters = string.Format("a{0}b{0}{1}c{2}{1}{0}s{2}1{0}2{0} 3{0}{1} {1}{1}{0}ä {1}{0}x", d, q, l);

        try
        {
          target.CsvToDictionaries(characters).ToList().AsReadOnly();
          Assert.Fail("Did not throw {0}.", typeof(IncorrectCsvColumnCountException));
        }
        catch (IncorrectCsvColumnCountException ex)
        {
          // did throw the exception
          Assert.That(ex.RowIndex, Is.EqualTo(r));
          IList<string> expectedHeaders = new[] { "a", "b", "c" + l, "s" };
          if ((f & CsvFlags.UseHeaderRow) != CsvFlags.UseHeaderRow)
            expectedHeaders = expectedHeaders.Select((s, i) => "Field" + (i + 1)).ToList();
          Assert.That(ex.HeaderNames, Is.EqualTo(expectedHeaders));
          Assert.That(ex.CurrentValues, Is.EqualTo(new[] { "1", "2", " 3", string.Format(" {1}{0}ä ", d, q), "x" }));
        }
      };

      runWithDelimiter("\n", ',', '"', CsvFlags.UseHeaderRow, 0); // the header row will not be added to the rowindex 
      runWithDelimiter("\r", '|', '~', CsvFlags.None, 1);
      runWithDelimiter("\r\n", ',', '-', CsvFlags.None, 1);
    }
  }
}
