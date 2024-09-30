using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackResult
{
  [CreateAssetMenu(fileName = "whackResultUiDataSource", menuName = "ScriptableObjects/WhackResultUiDataSource", order = 1)]
  public class WhackResultUiDataSource : ScriptableObject
  {
    public string timeRemainingStr;
    public float timeRemaining
    {
      set { timeRemainingStr = $"{value:0.0}"; }
    }

    public int seekMax;
    public int seekPosition;

    public string whackingScoreStr;
    public float whackingScore
    {
      set { whackingScoreStr = $"{value,6:F1}"; }
    }
    public string alignmentScoreStr;
    public float alignmentScore
    {
      set { alignmentScoreStr = $"{value,6:F1}"; }
    }
    public string totalScoreStr;
    public float totalScore
    {
      set { totalScoreStr = $"{value,6:F1}"; }
    }
  }
}
