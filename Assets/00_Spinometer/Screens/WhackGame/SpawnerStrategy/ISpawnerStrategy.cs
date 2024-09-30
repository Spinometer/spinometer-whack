using System;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public interface ISpawnerStrategy : IDisposable
  {
    bool IsDone();
    public void NextTick(double currentTime, float deltaTime);
  }
}
