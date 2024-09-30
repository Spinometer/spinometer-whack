using System;
using Random = UnityEngine.Random;

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

    bool ISpawnerStrategy.IsDone()
    {
      return false;
    }

    void ISpawnerStrategy.NextTick(double currentTime, float deltaTime)
    {
      if (Random.value < deltaTime) {
        _manager.Spawn(currentTime);
      }
    }
  }
}
