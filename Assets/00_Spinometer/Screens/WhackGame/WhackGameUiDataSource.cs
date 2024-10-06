using UnityEngine;
using UnityEngine.Serialization;

namespace GetBack.Spinometer.Screens.WhackGame
{
  [CreateAssetMenu(fileName = "whackGameUiDataSource", menuName = "ScriptableObjects/WhackGameUiDataSource", order = 1)]
  public class WhackGameUiDataSource : ScriptableObject
  {
    public string timeRemainingStr = "12.3";
    public float timeRemaining
    {
      set { timeRemainingStr = $"{value:0.0}"; }
    }

    public float text_ready_opacity = 1f;
    public Vector3 text_ready_scale = Vector3.one;

    public int whackingScore = 0;
    public string alignmentScoreStr = "0";
    public float alignmentScore
    {
      set { alignmentScoreStr = $"{value:0.0}"; }
    }
    public string comboBonusMultiplierStr = "";
    public int comboBonusMultiplier
    {
      set { comboBonusMultiplierStr = value <= 0 ? "" : $"Combo Bonus x{value}"; }
    }

    [FormerlySerializedAs("damageBackgroundOpacity")] public float damageOpacity = 0f;
  }
}
