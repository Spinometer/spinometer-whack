using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.FakeWordGenerator
{
  public static class FakeWordGenerator
  {
    private static Dictionary<string, List<(string, string, string)>> _wordDefinitions = null;
    private static List<string> _wordDefinitionsKeys = null;

    private static void LoadWordDefinitions()
    {
      if (_wordDefinitions != null)
        return;

      _wordDefinitions = new();
      _wordDefinitionsKeys = new();
      var textFile = Resources.Load<TextAsset>("dict");
      foreach (var line in textFile.text.Split('\n')) {
        var atoms = line.Split(' ');
        if (!_wordDefinitions.ContainsKey(atoms[0])) {
          _wordDefinitions.Add(atoms[0], new List<(string, string, string)> {});
          _wordDefinitionsKeys.Add(atoms[0]);
        }
        _wordDefinitions[atoms[0]].Add((atoms[0], atoms[1], atoms[2]));
      }
    }

    public static string Generate(int textLength, Dictionary<char, bool> occupiedHeadChars)
    {
      LoadWordDefinitions();

      var (text, lastTail) = FirstElement(occupiedHeadChars);
      text = AddRestElements(text, lastTail, textLength, occupiedHeadChars);
      return text.Substring(0, Math.Min(textLength, text.Length)).ToUpper();;
    }

    private static (string, string) FirstElement(Dictionary<char, bool> occupiedHeadChars)
    {
      var numDefs = _wordDefinitions.Count;
      for (;;) {
        var key = _wordDefinitionsKeys[Random.Range(0, numDefs - 1)];
        var ch = Char.ToLower(key[0]);
        if (!occupiedHeadChars.ContainsKey(Char.ToLower(ch))) {
          var count = _wordDefinitions[key].Count;
          var (s0, s1, s2) = _wordDefinitions[key][Random.Range(0, count - 1)];
          return (s0 + s1 + s2, s2);
        }
      }
    }

    private static string AddRestElements(string text, string lastTail, int textLength, Dictionary<char, bool> occupiedHeadChars)
    {
      if (text.Length >= textLength)
        return text;

      if (!_wordDefinitions.ContainsKey(lastTail))
        return text;

      var count = _wordDefinitions[lastTail].Count;
      var (s0, s1, s2) = _wordDefinitions[lastTail][Random.Range(0, count - 1)];
      var newTail = s2;
      text += s1 + s2;
      return AddRestElements(text, newTail, textLength, occupiedHeadChars);
    }
  }
}
