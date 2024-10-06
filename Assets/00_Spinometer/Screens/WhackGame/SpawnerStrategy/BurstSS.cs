using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class BurstSS : ISpawnerStrategy
  {
    private MoleRowManager _moleRowManager;
    private double _burstStartedAt;
    private double _burstEndsAt;
    private bool _isDone;
    private int _burstCountLeft;

    private CancellationTokenSource _cts;

    private const double durationPerBurst = 4.0;

    public BurstSS(MoleRowManager moleRowManager)
    {
      _moleRowManager = moleRowManager;
      _burstStartedAt = 0;
      _burstEndsAt = 0;
      _burstCountLeft = 0;
      _isDone = false;
      _cts = new();
    }

    void IDisposable.Dispose()
    {
      _cts.Cancel();
      _cts.Dispose();
      _cts = null;
    }

    bool ISpawnerStrategy.IsDone()
    {
      return _isDone;
    }

    void ISpawnerStrategy.NextTick(double currentTime, float deltaTime)
    {
      if (_burstStartedAt == 0) {
        StartBurst(currentTime);
      }

      _isDone = _isDone || currentTime >= _burstEndsAt;
    }

    private async void StartBurst(double currentTime)
    {
      _burstCountLeft = 5;
      _burstStartedAt = currentTime;
      _burstEndsAt = currentTime + durationPerBurst;
      _isDone = false;

      var options = _moleRowManager.options;
      options.spawnBoundary1 = new Vector3(options.spawnBoundary1.x - 0.8f, 0f, 0f);
      options.forceVelocity = true;
      //options.velocity = Random.insideUnitCircle.normalized * 0.03f;
      options.velocity = Random.insideUnitCircle.normalized * 0.12f;
      options.sizeMin = 0.070f;
      options.sizeMax = 0.080f;
      options.vulnerableTimeMin = 3f;
      options.vulnerableTimeMax = 4f;

      for (int i = 0; i < 5; i++) {
        var mole = _moleRowManager.Spawn(currentTime, options);
        options.spawnBoundary0 = mole.position + new Vector3(mole.size, 0f, 0f) * 0.65f;
        options.spawnBoundary1 = options.spawnBoundary0;
        options.vulnerableTimeMin = (float)(mole.activeUntil - currentTime);
        options.vulnerableTimeMax = options.vulnerableTimeMin;
        int interval_ms = 30;
        currentTime += interval_ms * 1e-3;
        var isCanceled = await UniTask.Delay(interval_ms, cancellationToken: _cts.Token).SuppressCancellationThrow();
        if (isCanceled)
          return;
      }
    }
  }
}
