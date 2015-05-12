using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGiesecke.PlainCsv.Core;
using NUnit.Framework;
namespace RGiesecke.PlainCsv.Core.Tests
{
  [TestFixture()]
  public class OrderedDictionaryTests
  {
    [Test()]
    public void OrderedDictionaryTest()
    {
      var d = new OrderedDictionary<string, int>();
      d["a"] = 1;
      Assert.That(d.Count, Is.EqualTo(1));
      Assert.That(d.Keys, Is.EqualTo(new[] { "a" }));
      Assert.That(d.Values, Is.EqualTo(new[] { 1 }));

      d["b"] = 2;
      Assert.That(d.Count, Is.EqualTo(2));
      Assert.That(d.Keys, Is.EqualTo(new[] { "a", "b" }));
      Assert.That(d.Values, Is.EqualTo(new[] { 1, 2 }));
    }
  }
}
