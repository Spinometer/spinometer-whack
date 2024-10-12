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
    private double _spawnNextSpecialAt;

    public CompositeSS(Settings settings, MoleRowManager moleRowManager, double currentTime, Options options)
    {
      _options = options;
      _settings = settings;
      _moleRowManager = moleRowManager;
      _whackGame = _moleRowManager.whackGame;
      _spawnNextAt = currentTime;
      _spawnNextSpecialAt = currentTime + _whackGame.timeRemaining * 0.666f;
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
      bool special = _spawnNextSpecialAt < currentTime;
      TryPushNewSpawnerStrategy(currentTime, special);
    }

    private void TryPushNewSpawnerStrategy(double currentTime, bool special = false)
    {
      if (_whackGame.timeRemaining < 1f) {
        // there's not enough time left to answer
        return;
      }

      if (_whackGame.timeRemaining < 4f) {
        // likewise, but quick spawner strategy can be used here
        SpawnSingleShot(currentTime, special);
        return;
      }

      if (special) {
        SpawnBurst(currentTime, special);
        _spawnNextSpecialAt = currentTime + _whackGame.timeRemaining * 2f; // never spawn again
        return;
      }

      var difficulty = 1.0f - (_whackGame.timeRemaining / _whackGame.initialTime);
      var doSingleShot = difficulty < Mathf.Pow(Random.value, _options.difficulty);
      if (doSingleShot) {
        SpawnSingleShot(currentTime, special);
      } else {
        SpawnBurst(currentTime, special);
      }
    }

    private void SpawnSingleShot(double currentTime, bool special)
    {
      _moleRowManager.PushSpawnerStrategy(MoleRowManager.SpawnerStrategy.periodic, currentTime, special);
      _spawnNextAt = currentTime + Random.value * 1.2f * _settings.timeMultiplier;
    }

    private void SpawnBurst(double currentTime, bool special)
    {
      _moleRowManager.PushSpawnerStrategy(MoleRowManager.SpawnerStrategy.burst, currentTime, special);
      _spawnNextAt = currentTime + Random.value * 5.0f * _settings.timeMultiplier;
    }
  }
}
