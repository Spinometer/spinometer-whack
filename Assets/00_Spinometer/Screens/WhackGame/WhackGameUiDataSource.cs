using UnityEngine;

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

    public int score = 0;
  }
}
