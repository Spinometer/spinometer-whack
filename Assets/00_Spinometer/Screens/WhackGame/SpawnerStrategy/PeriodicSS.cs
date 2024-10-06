using System;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class PeriodicSS : ISpawnerStrategy
  {
    private MoleRowManager _moleRowManager;
    private WhackGame _whackGame;
    private double _startedAt = 0f;
    private double _endsAt = 0f;
    private bool _isDone = false;

    public PeriodicSS(MoleRowManager moleRowManager, double currentTime)
    {
      _moleRowManager = moleRowManager;
      _whackGame = _moleRowManager.whackGame;
      Spawn(currentTime);
    }

    void IDisposable.Dispose()
    {
    }

    bool ISpawnerStrategy.IsDone()
    {
      return true;
    }

    public void NextTick(double currentTime, float deltaTime)
    {
    }

    public void Spawn(double currentTime)
    {
      if (_whackGame.state == WhackGame.State.GoingOn && _whackGame.timeRemaining >= 0.5f)
        _moleRowManager.SpawnMoleRaw(currentTime, _moleRowManager.options);
    }
  }
}
