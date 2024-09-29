using System;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class BurstSS : ISpawnerStrategy
  {
    private MoleManager _manager;

    public BurstSS(MoleManager manager)
    {
      _manager = manager;
    }

    void IDisposable.Dispose()
    {
    }

    void ISpawnerStrategy.NextTick()
    {
    }
  }
}
