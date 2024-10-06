using System;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class PeriodicSS : ISpawnerStrategy
  {
    private readonly Settings _settings;
    private MoleRowManager _moleRowManager;
    private WhackGame _whackGame;
    private double _startedAt = 0f;
    private double _endsAt = 0f;
    private bool _isDone = false;

    public PeriodicSS(Settings settings, MoleRowManager moleRowManager, double currentTime)
    {
      _settings = settings;
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
      if (_whackGame.state == WhackGame.State.GoingOn && _whackGame.timeRemaining >= 0.5f) {
        var options = _moleRowManager.options;
        options.vulnerableTimeMin *= _settings.timeMultiplier;
        options.vulnerableTimeMax *= _settings.timeMultiplier;
        options.forceVelocity = true;
        options.velocity = Random.insideUnitSphere.normalized * (0.3f * _settings.velocityMultiplier);
        options.vulnerableTimeMax *= _settings.timeMultiplier;
        _moleRowManager.SpawnMoleRow(currentTime, options);
      }
    }
  }
}
