using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame.Buff
{
  public class FlawlessCombo : IBuff
  {
    private double _activeUntil = 0.0;
    public double activeUntil { get => _activeUntil; }
    private WhackGame _whackGame;

    public FlawlessCombo(WhackGame whackGame)
    {
      _whackGame = whackGame;
    }

    public void Activate(double currentTime)
    {
      _activeUntil = currentTime + 1.0;
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      if (currentTime >= _activeUntil)
        return;
      _whackGame.comboGuageValue = Mathf.Min(_whackGame.maxComboGuageValue, _whackGame.comboGuageValue + deltaTime * 5f);
    }
  }
}
