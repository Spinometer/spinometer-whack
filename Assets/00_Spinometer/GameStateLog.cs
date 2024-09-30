using System;
using System.Collections.Generic;
using UnityEngine;

namespace GetBack.Spinometer
{
  [CreateAssetMenu(fileName = "gameStateLogInstance", menuName = "ScriptableObjects/GameStateLog", order = 1)]
  public class GameStateLog : ScriptableObject
  {
    public struct GameStateLogEntry
    {
      public float timestamp;
      public int whackingScore;
      public int possibleMaximumWhackingScore;
      public float alignmentScore;
      public float possibleMaximumAlignmentScore;
    }

    public List<GameStateLogEntry> entries = new();

    public void Clear()
    {
      entries.Clear();
    }
  }
}
