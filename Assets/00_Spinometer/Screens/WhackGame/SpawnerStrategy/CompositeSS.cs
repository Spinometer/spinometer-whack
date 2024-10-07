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

    private Settings _settings;
    private MoleRowManager _moleRowManager;
    private WhackGame _whackGame;
    private Options _options;
    private double _spawnNextAt;

    public CompositeSS(Settings settings, MoleRowManager moleRowManager, double currentTime, Options options)
    {
      _options = options;
      _settings = settings;
      _moleRowManager = moleRowManager;
      _whackGame = _moleRowManager.whackGame;
      _spawnNextAt = currentTime;
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
      int count = _moleRowManager.CountActiveMoleRows();
      if (count == 0)
        _spawnNextAt = Math.Min(_spawnNextAt, currentTime + 0.2);
      if (_spawnNextAt > currentTime)
        return;
      TryPushNewSpawnerStrategy(currentTime);
    }

    private void TryPushNewSpawnerStrategy(double currentTime)
    {
      if (_whackGame.timeRemaining < 1f) {
        // there's not enough time left to answer
        return;
      }

      if (_whackGame.timeRemaining < 4f) {
        // likewise, but quick spawner strategy can be used here
        SpawnSingleShot(currentTime);
        return;
      }

      var difficulty = 1.0f - (_whackGame.timeRemaining / _whackGame.initialTime);
      var doSingleShot = difficulty < Mathf.Pow(Random.value, _options.difficulty);
      if (doSingleShot) {
        SpawnSingleShot(currentTime);
      } else {
        SpawnBurst(currentTime);
      }
    }

    private void SpawnSingleShot(double currentTime)
    {
      _moleRowManager.PushSpawnerStrategy(MoleRowManager.SpawnerStrategy.periodic, currentTime);
      _spawnNextAt = currentTime + Random.value * 1.2f * _settings.timeMultiplier;
    }

    private void SpawnBurst(double currentTime)
    {
      _moleRowManager.PushSpawnerStrategy(MoleRowManager.SpawnerStrategy.burst, currentTime);
      _spawnNextAt = currentTime + Random.value * 5.0f * _settings.timeMultiplier;
    }
  }
}
