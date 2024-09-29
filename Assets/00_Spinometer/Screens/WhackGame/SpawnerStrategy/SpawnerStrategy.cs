using System;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public interface ISpawnerStrategy : IDisposable
  {
    public void NextTick();
  }
}
