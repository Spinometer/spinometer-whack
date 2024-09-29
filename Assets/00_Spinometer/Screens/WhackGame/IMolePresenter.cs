using System;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public interface IMolePresenter : IDisposable
  {
    void NextTick(double currentTime, float deltaTime);
  }
}
