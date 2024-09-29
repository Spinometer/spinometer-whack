using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class BurstSS : ISpawnerStrategy
  {
    private MoleManager _moleManager;
    private WhackGame _whackGame;
    private double _nextBurstStartTime;
    private int _burstCountLeft;

    private CancellationTokenSource _cts;

    public BurstSS(MoleManager moleManager)
    {
      _moleManager = moleManager;
      _whackGame = _moleManager.whackGame;
      _nextBurstStartTime = Time.timeAsDouble;
      _burstCountLeft = 0;
      _cts = new();
    }

    void IDisposable.Dispose()
    {
      _cts.Cancel();
      _cts.Dispose();
      _cts = null;
    }

    void ISpawnerStrategy.NextTick(double currentTime, float deltaTime)
    {
      if (currentTime >= _nextBurstStartTime) {
        StartBurst(currentTime);
      }
    }

    private async void StartBurst(double currentTime)
    {
      _burstCountLeft = 5;
      _nextBurstStartTime = currentTime + 4.0;

      var options = _moleManager.options;
      options.spawnBoundary1 = new Vector3(options.spawnBoundary1.x - 0.8f, 0f, 0f);
      options.sizeMin = 0.15f;
      options.sizeMax = 0.2f;
      options.vulnerableTimeMin = 3f;
      options.vulnerableTimeMax = 4f;

      for (int i = 0; i < 5; i++) {
        var mole = _moleManager.Spawn(currentTime, options);
        options.spawnBoundary0 = mole.position + new Vector3(mole.size, 0f, 0f);
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
