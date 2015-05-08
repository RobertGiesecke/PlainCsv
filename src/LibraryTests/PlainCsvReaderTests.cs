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
  [TestFixture, ExcludeFromCodeCoverage]
  public class PlainCsvReaderTests
  {
    [Test()]
    public void ReadCsvRows_Separates_By_NonString_LF_and_CR()
    {
      var target = new PlainCsvReader();

      Action<string> runWithLineDelimiter = l =>
      {
        var actual = target.ReadCsvRows("a,b,c" + l + "1,2, 3").ToList().AsReadOnly();
        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0], Is.EqualTo(new[] {"a", "b", "c"}));
        Assert.That(actual[1], Is.EqualTo(new[] {"1", "2", " 3"}));
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
        Assert.That(actual[0], Is.EqualTo(new[] {"a", "b", "c"}));
        Assert.That(actual[1], Is.EqualTo(new[] {"1", null, " "}));
        Assert.That(actual[1], Is.Not.EqualTo(new[] {"1", "", " "}));
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
        Assert.That(actual[0].Keys, Is.EqualTo(new[] {"a", "b", "c" + l, "s"}));
        Assert.That(actual[0].Values, Is.EqualTo(new[] {"1", "2", " 3", string.Format(" {1}{0}ä ", d, q)}));
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
  }
}
