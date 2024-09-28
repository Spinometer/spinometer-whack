using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame
{
  [CreateAssetMenu(fileName = "whackGameUiDataSource", menuName = "ScriptableObjects/WhackGameUiDataSource", order = 1)]
  public class WhackGameUiDataSource : ScriptableObject
  {
    public string timeRemainingStr;  
    public float timeRemaining
    {
      set { timeRemainingStr = $"{value:0.0}"; }
    }
  }
}
