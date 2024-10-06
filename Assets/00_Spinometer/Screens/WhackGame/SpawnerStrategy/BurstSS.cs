using System;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class BurstSS : ISpawnerStrategy
  {
    private MoleRowManager _moleRowManager;

    private CancellationTokenSource _cts;


    public BurstSS(MoleRowManager moleRowManager, double currentTime)
    {
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
      options.textLengthMin = 5;
      options.textLengthMax = 8;
      options.forceVelocity = true;
      //options.velocity = Random.insideUnitCircle.normalized * 0.03f;
      options.velocity = Random.insideUnitCircle.normalized * 0.12f;
      options.sizeMin = 0.070f;
      options.sizeMax = 0.080f;
      options.vulnerableTimeMin = 3f;
      options.vulnerableTimeMax = 4f;

      _moleRowManager.SpawnMoleRaw(currentTime, options);
    }
  }
}
