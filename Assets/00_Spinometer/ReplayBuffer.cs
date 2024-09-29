using System.Collections.Generic;
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
      public Texture2D texture;
      public float distance;
      public float pitch;
      public SpinalAlignment spinalAlignment;
      public SpinalAlignmentScore spinalAlignmentScore;
    }

    public List<ReplayEntry> entries = new();

    public void Clear()
    {
      entries.Clear();
    }
  }
}
