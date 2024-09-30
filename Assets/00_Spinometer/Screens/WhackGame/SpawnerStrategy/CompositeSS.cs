using System;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class CompositeSS : ISpawnerStrategy
  {
    private MoleManager _moleManager;
    private WhackGame _whackGame;

    public CompositeSS(MoleManager moleManager)
    {
      _moleManager = moleManager;
      _whackGame = _moleManager.whackGame;
    }

    void IDisposable.Dispose()
    {
    }

    bool ISpawnerStrategy.IsDone()
    {
      return false;
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      TryPushNewSpawnerStrategy();
    }

    private void TryPushNewSpawnerStrategy()
    {
      if (this != _moleManager.strategyStack.Peek()) {
        // previous strategy is running, skip this tick
        return;
      }

      if (_whackGame.timeRemaining < 1f) {
        // there's not enough time left to answer
        return;
      }

      if (_whackGame.timeRemaining < 4f) {
        // likewise, but quick spawner strategy can be used here
        _moleManager.PushSpawnerStrategy(MoleManager.SpawnerStrategy.periodic);
        return;
      }

      var difficulty = 1.0f - (_whackGame.timeRemaining / _whackGame.initialTime);
      var strategy = (difficulty * difficulty * difficulty) < Random.value ? MoleManager.SpawnerStrategy.periodic : MoleManager.SpawnerStrategy.burst;
      _moleManager.PushSpawnerStrategy(strategy);
    }
  }
}
