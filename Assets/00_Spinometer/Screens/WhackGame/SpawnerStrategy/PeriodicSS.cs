using System;
using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class PeriodicSS : ISpawnerStrategy
  {
    private MoleManager _moleManager;
    private WhackGame _whackGame;

    public PeriodicSS(MoleManager moleManager)
    {
      _moleManager = moleManager;
      _whackGame = _moleManager.whackGame;
      _whackGame.OnCrossingSecondBoundary += OnCrossingSecondBoundary;
    }

    void IDisposable.Dispose()
    {
      _whackGame.OnCrossingSecondBoundary -= OnCrossingSecondBoundary;
    }

    public void NextTick(double currentTime, float deltaTime)
    {
    }

    private void OnCrossingSecondBoundary()
    {
      if (_whackGame.state == WhackGame.State.GettingReady &&
          _whackGame.timeRemaining <= _whackGame.initialTime + 0.01f)
        _moleManager.
          Spawn(Time.timeAsDouble);
      else if (_whackGame.state == WhackGame.State.GoingOn && _whackGame.timeRemaining >= 0.5f)
        _moleManager.Spawn(Time.timeAsDouble);
    }
  }
}
