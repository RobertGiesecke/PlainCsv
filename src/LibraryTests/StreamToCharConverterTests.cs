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
  public class StreamToCharConverterTests
  {
    [Test()]
    public void ReaderToEnumerableTest()
    {
      var expected = "abc".PadRight(1000);
      using (var r = new StringReader(expected))
      {
        var actual = StreamToCharConverter.ReaderToEnumerable(r, 10).ToList();
        Assert.That(actual, Is.EqualTo(expected.ToCharArray()));
      }
    }
  }
}
