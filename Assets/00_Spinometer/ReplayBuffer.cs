using System.Collections.Generic;
using System.Linq;
using GetBack.Spinometer.SpinalAlignmentAux;
using GetBack.Spinometer.SpinalAlignmentCore;
using UnityEngine;

namespace GetBack.Spinometer
{
  [CreateAssetMenu(fileName = "replayBufferInstance", menuName = "ScriptableObjects/ReplayBuffer", order = 1)]
  public class ReplayBuffer : ScriptableObject
  {
    public struct ReplayEntry
    {
      public float timeRemaining;
      public Texture2D texture;
      public float distance;
      public float pitch;
      public SpinalAlignment spinalAlignment;
      public SpinalAlignmentScore spinalAlignmentScore;
    }

    public List<ReplayEntry> entries = new();
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
