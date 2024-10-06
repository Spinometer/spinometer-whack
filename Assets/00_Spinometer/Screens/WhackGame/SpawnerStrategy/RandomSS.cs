using System;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class RandomSS : ISpawnerStrategy
  {
    private MoleRowManager _moleRowManager;

    public RandomSS(Settings settings, MoleRowManager moleRowManager)
    {
      _moleRowManager = moleRowManager;
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
        _moleRowManager.SpawnMoleRow(currentTime, _moleRowManager.options);
      }
    }
  }
}
