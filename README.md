This is an implementation of the [http://en.wikipedia.org/wiki/Delimiter-separated_values](http://en.wikipedia.org/wiki/Delimiter-separated_values "Delimiter-separated values") standard.

I got fed up with libs that did all kinds of extra stuff, but didn't adhere to the spec. So I started this little library.

## Examples:

### Reading


#### Read into a list of dictionaries 
```c#
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

// CsvUtils.ParseDateTime helps you to get datetimes from CSV files
Assert.That(CsvUtils.ParseDateTime(secondRow["b"]), Is.EqualTo(new DateTime(2010, 1, 1)));
Assert.That(secondRow["c"], Is.EqualTo("y\r\ns")); // cariage return and line feed are preserved
Assert.That(CsvUtils.ParseDateTime(secondRow["d"]), Is.EqualTo(DateTime.MinValue.Add(new TimeSpan(15, 30, 0))));
Assert.That(secondRow["e"], Is.EqualTo(" "));
```

CsvToDictionaries and ReadCsvRows work on any IEnumerable&lt;char&gt;, Streams and TextReaders.

#### Just get a list of strings per row

You can also use the lower level function ReadCsvRows to get separated values according to quotation and delimiter rules.
ReadCsvRows does not require rows to have the same number of cells. 

```c#
var target = new PlainCsvReader();
var actual = target.ReadCsvRows("a,b,\"c\na\"\nb c").ToList();
Assert.That(actual.Count, Is.EqualTo(2));
Assert.That(actual[0], Is.EqualTo(new[] {"a", "b", "c\na"}));
Assert.That(actual[1], Is.EqualTo(new[] {"b c"}));
```

### Writing

#### Inferred column order:
In this example, the column order of the output file depends on when a column name is seen for the first time.
You can provide a keyComparer so that differently cased keys will end up in the same column.

```c#
var writer = new PlainCsvWriter(new CsvWriterOptions(CsvWriterOptions.Default, delimiter: '@'));
var sb = writer.DictionariesToCsvString(new[]
{
	new CsvDictionary<object> // I use this class to ensure the column order
	{
		{"a@a", 1},
		{"b", new DateTime(2009, 1, 20, 15, 2, 0)},
	},
	new CsvDictionary<object>
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

```

#### User provided column order:

You can provide a list of column names.
PlainCsvWriter will use that ordering for output columns. Every column that is not in that list will be added at the end.
If a column has been


``` c#
var writer = new PlainCsvWriter(new CsvWriterOptions(CsvWriterOptions.Default,
                                                     delimiter: '#',
                                                     quoteChar:'|',
                                                     sortedColumnNames: new[]
                                {
	                                "y",
	                                "a#a",
	                                "b",
                                	"xyz" // names in this list can be missing from actual data
                                }));
var sb = writer.DictionariesToCsvString(new IDictionary<string, object>[]
{
	new CsvDictionary<object>
	{
		{"a#a", 1},
		{"b", new DateTime(2009, 1, 20, 15, 2, 0)},
	},
	new CsvDictionary<object>
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
```

### Converting

When you enable CsvWriterOptions.AssumeFixedColumnCount, PlainCsvWriter doesn't have to read all rows before writing the first one.
That means, it can stream rows directly from a PlainCsvReader. Because only one row is in memory at a given time, it is possible to convert or process very large files.

This example converts a semicolon separated file into an utf8-encoded real CSV:

``` c#

var ssvReader = new PlainCsvReader(new CsvOptions(delimiter: ';'));
var csvWriter = new PlainCsvWriter(new CsvWriterOptions(assumeFixedColumnCount: true));
using(var textReader = new StreamReader(@"sourcefile.txt"))
using(var textWriter = new StreamWriter(@"outputfile.csv", false, Encoding.UTF8))
	csvWriter.DictionariesToCsv(textWriter, ssvReader.CsvToDictionaries(textReader)); 

```
CsvToDictionaries and ReadCsvRows only read as long as they have to. So when you take only the first 10 rows, they will return very quickly and use very little memory: 

``` c#
IList<CsvDictionary> rows;
var ssvReader = new PlainCsvReader(new CsvOptions(delimiter: ';'));
using(var textReader = new StreamReader(@"sourcefile.txt"))
	rows = ssvReader.CsvToDictionaries(textReader).Take(10).ToList(); 

```