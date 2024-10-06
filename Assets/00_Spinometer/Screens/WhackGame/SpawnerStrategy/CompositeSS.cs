using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class CompositeSS : ISpawnerStrategy
  {
    [Serializable]
    public struct Options
    {
      public float difficulty;
    }

    private MoleRowManager _moleRowManager;
    private WhackGame _whackGame;
    private Options _options;

    public CompositeSS(MoleRowManager moleRowManager, Options options)
    {
      _options = options;
      _moleRowManager = moleRowManager;
      _whackGame = _moleRowManager.whackGame;
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
      if (this != _moleRowManager.strategyStack.Peek()) {
        // previous strategy is running, skip this tick
        return;
      }

      if (_whackGame.timeRemaining < 1f) {
        // there's not enough time left to answer
        return;
      }

      if (_whackGame.timeRemaining < 4f) {
        // likewise, but quick spawner strategy can be used here
        _moleRowManager.PushSpawnerStrategy(MoleRowManager.SpawnerStrategy.periodic);
        return;
      }

      var difficulty = 1.0f - (_whackGame.timeRemaining / _whackGame.initialTime);
      var strategy = difficulty < Mathf.Pow(Random.value, _options.difficulty) ? MoleRowManager.SpawnerStrategy.periodic : MoleRowManager.SpawnerStrategy.burst;
      _moleRowManager.PushSpawnerStrategy(strategy);
    }
  }
}
