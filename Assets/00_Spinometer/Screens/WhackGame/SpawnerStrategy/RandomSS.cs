using System;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class RandomSS : ISpawnerStrategy
  {
    private MoleManager _manager;

    public RandomSS(MoleManager manager)
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
