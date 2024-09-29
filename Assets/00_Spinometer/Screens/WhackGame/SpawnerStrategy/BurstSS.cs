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
      _nextBurstStartTime = currentTime + 2.0;

      for (int i = 0; i < 5; i++) {
        _moleManager.Spawn(currentTime);
        var isCanceled = await UniTask.Delay(30, cancellationToken: _cts.Token).SuppressCancellationThrow();
        if (isCanceled) {
          Debug.Log("canceled");
          return;
        }
      }
    }
  }
}
