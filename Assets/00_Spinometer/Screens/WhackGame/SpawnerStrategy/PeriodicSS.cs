using System;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class PeriodicSS : ISpawnerStrategy
  {
    private MoleManager _moleManager;
    private WhackGame _whackGame;
    private double _startedAt = 0f;
    private double _endsAt = 0f;
    private bool _isDone = false;

    public PeriodicSS(MoleManager moleManager)
    {
      _moleManager = moleManager;
      _whackGame = _moleManager.whackGame;
    }

    void IDisposable.Dispose()
    {
    }

    bool ISpawnerStrategy.IsDone()
    {
      return _isDone;
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      if (_isDone)
        return;

      if (_startedAt == 0f) {
        _startedAt = currentTime;
        _endsAt = _startedAt + 1.0;

        if (_whackGame.state == WhackGame.State.GoingOn && _whackGame.timeRemaining >= 0.5f)
          _moleManager.Spawn(currentTime);
      }

      _isDone = _isDone || currentTime >= _endsAt;
    }
  }
}
