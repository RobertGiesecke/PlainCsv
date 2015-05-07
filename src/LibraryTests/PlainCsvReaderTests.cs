using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGiesecke.PlainCsv;
using NUnit.Framework;
namespace RGiesecke.PlainCsv.Tests
{
  [TestFixture()]
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
        Assert.That(actual[0], Is.EqualTo(new[] { "a", "b", "c" }));
        Assert.That(actual[1], Is.EqualTo(new[] { "1", "2", " 3" }));
      };

      runWithLineDelimiter("\n");
      runWithLineDelimiter("\r");
      runWithLineDelimiter("\r\n");
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
  }
}
