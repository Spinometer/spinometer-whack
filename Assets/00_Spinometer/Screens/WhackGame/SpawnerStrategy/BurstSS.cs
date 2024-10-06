using System;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class BurstSS : ISpawnerStrategy
  {
    private Settings _settings;
    private MoleRowManager _moleRowManager;

    private CancellationTokenSource _cts;


    public BurstSS(Settings settings, MoleRowManager moleRowManager, double currentTime)
    {
      _settings = settings;
      _moleRowManager = moleRowManager;
      StartBurst(currentTime);
    }

    void IDisposable.Dispose()
    {
    }

    bool ISpawnerStrategy.IsDone()
    {
      return true;
    }

    void ISpawnerStrategy.NextTick(double currentTime, float deltaTime)
    {
    }

    private void StartBurst(double currentTime)
    {
      var options = _moleRowManager.options;
      options.spawnBoundary1 = new Vector3(options.spawnBoundary1.x - 0.8f, 0f, 0f);
      options.textLengthMin = (int)(5 * _settings.textLengthMultiplier);
      options.textLengthMax = (int)(8 * _settings.textLengthMultiplier);
      options.textLengthMin = options.textLengthMin <= 0 ? 1 : options.textLengthMin;
      options.textLengthMax = options.textLengthMax <= 0 ? 1 : options.textLengthMax;
      options.forceVelocity = true;
      options.velocity = Random.insideUnitCircle.normalized * (0.12f * _settings.velocityMultiplier);
      options.sizeMin = 0.070f * _settings.textSizeMultiplier;
      options.sizeMax = 0.080f * _settings.textSizeMultiplier;
      options.vulnerableTimeMin = 3f * _settings.timeMultiplier;
      options.vulnerableTimeMax = 4f * _settings.timeMultiplier;

      _moleRowManager.SpawnMoleRow(currentTime, options);
    }
  }
}
