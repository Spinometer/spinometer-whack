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
  }
}
