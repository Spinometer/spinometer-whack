using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GetBack.Spinometer
{
  [CreateAssetMenu(fileName = "gameStateLogInstance", menuName = "ScriptableObjects/GameStateLog", order = 1)]
  public class GameStateLog : ScriptableObject
  {
    public struct GameStateLogEntry
    {
      public float timeRemaining;
      public int whackingScore;
      public int possibleMaximumWhackingScore;
      public float alignmentScore;
      public float possibleMaximumAlignmentScore;
    }

    public List<GameStateLogEntry> entries = new();
    public float timeRemainingMin = 0f;
    public float timeRemainingMax = 0f;

    public void Clear()
    {
      entries.Clear();
      timeRemainingMin = 0f;
      timeRemainingMax = 0f;
    }

    public void UpdateTimeRemainingMinMax()
    {
      var ts = entries.Select(e => e.timeRemaining).ToList();
      if (ts.Count == 0) {
        timeRemainingMin = 0f;
        timeRemainingMax = 0f;
      } else {
        timeRemainingMin = ts.Min();
        timeRemainingMax = ts.Max();
      }
    }
  }
}
