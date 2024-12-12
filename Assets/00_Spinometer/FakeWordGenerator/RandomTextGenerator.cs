using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.FakeWordGenerator
{
  public static class RandomTextGenerator
  {
    public static char RandomChar()
    {
      bool uppercase = Random.Range(0, 2) == 0;
      return (char)(uppercase ? Random.Range('A', 'Z' + 1) : Random.Range('a', 'z' + 1));
    }
    public static char RandomFirstChar(Dictionary<char, bool> occupiedHeadChars)
    {
      for (;;) {
        var ch = RandomChar();
        if (!occupiedHeadChars.ContainsKey(Char.ToLower(ch)))
          return ch;
      }
    }

    public static string Generate(int textLength, Dictionary<char, bool> occupiedHeadChars)
    {
      var text = RandomFirstChar(occupiedHeadChars) + String.Join("", Enumerable.Range(1, textLength).Select(i => RandomChar()));
      return text;
    }
  }
}
