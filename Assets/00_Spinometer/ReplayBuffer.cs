using System.Collections.Generic;
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
      public SpinalAlignmentCore.SpinalAlignment spinalAlignment;
    }

    public List<ReplayEntry> entries = new();

    public void Clear()
    {
      entries.Clear();
    }
  }
}
