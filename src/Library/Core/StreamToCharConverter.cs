using System;
using System.Collections.Generic;
using System.IO;

namespace RGiesecke.PlainCsv.Core
{
  public class StreamToCharConverter
  {
    public static IEnumerable<char> ReaderToEnumerable(TextReader reader, int? bufferSize = null)
    {
      var usedBufferSize = bufferSize ?? 100;

      var chars = new char[usedBufferSize];
      for (int readLength; (readLength = reader.Read(chars, 0, usedBufferSize)) > 0; )
      {
        if (readLength == 0)
          yield break;

        for (int i = 0; i < readLength; i++)
          yield return chars[i];

        if (readLength < usedBufferSize)
          yield break;
      }
    }

    public static IEnumerable<Char> StreamToEnumerable(Stream stream, int? bufferSize = null)
    {
      using (var r = new StreamReader(stream, true))
        foreach (var c in ReaderToEnumerable(r, bufferSize))
          yield return c;
    }
  }
}