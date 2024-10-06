using System;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class RandomSS : ISpawnerStrategy
  {
    private MoleRowManager _rowManager;

    public RandomSS(MoleRowManager rowManager)
    {
      _rowManager = rowManager;
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
        _rowManager.Spawn(currentTime);
      }
    }
  }
}
