using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace RGiesecke.PlainCsv.Tests
{
  [ExcludeFromCodeCoverage]
  public abstract class CsvOptionsTestsBase
  {
    protected void AssertThatOptionsAreCsv(CsvOptions actual)
    {
      Assert.That(actual.QuoteChar, Is.EqualTo('"'));
      Assert.That(actual.Delimiter, Is.EqualTo(','));
    }

    protected void AssertThatSameQuoteCharAndDelimiterThrows<T>(Func<char, char, T> factory)
      where T : CsvOptions
    {
      Func<char, char, Tuple<T, ArgumentOutOfRangeException>> createOptions = (q, d) =>
      {
        try
        {
          var r = factory(q, d);
          return new Tuple<T, ArgumentOutOfRangeException>(r, null);
        }
        catch (ArgumentOutOfRangeException ex)
        {
          return new Tuple<T, ArgumentOutOfRangeException>(null, ex);
        }
      };

      var actual = createOptions('"', ',');
      Assert.That(actual.Item1, Is.Not.Null);
      Assert.That(actual.Item2, Is.Null);
      actual = createOptions('"', '"');
      Assert.That(actual.Item1, Is.Null);
      Assert.That(actual.Item2, Is.Not.Null);
      actual = createOptions(' ', ' ');
      Assert.That(actual.Item1, Is.Null);
      Assert.That(actual.Item2, Is.Not.Null);
      actual = createOptions(',', ',');
      Assert.That(actual.Item1, Is.Null);
      Assert.That(actual.Item2, Is.Not.Null);
      actual = createOptions(',', ';');
      Assert.That(actual.Item1, Is.Not.Null);
      Assert.That(actual.Item2, Is.Null);
      actual = createOptions(' ', '\t');
      Assert.That(actual.Item1, Is.Not.Null);
      Assert.That(actual.Item2, Is.Null);
    }
  }
}