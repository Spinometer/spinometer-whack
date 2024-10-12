using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class MoleRow : IDisposable
  {
    public struct SpawnOptions
    {
      public MolePresenter.Options presenterOptions;
      public string text;
      public Vector3 centerPosition;
      public Vector3 velocity;
      public float size;
      public float aspectRatio;
      public double vulnerableTime;
      public bool special;
    }

    private readonly Settings _settings;
    private readonly WhackGame _whackGame;
    private readonly MoleRowManager _moleRowManager;
    private readonly AudioSource _audioSource;
    private List<Mole> _moles = new();
    private int _numInitialMoles;
    private int _numWhackedMoles;
    private CancellationTokenSource _cts = new();

    public bool hasFocus;
    public bool hasExclusiveFocus;
    public bool IsEmpty => !_moles.Any(m => m.alive);
    public int FirstActiveMoleIndex => _moles.FindIndex(m => m.alive);
    public Mole? FirstActiveMole => _moles.Find(m => m.alive);
    public char FirstChar => FirstActiveMole?.text[0] ?? '\0';
    private bool _wholeRowEliminated;
    public bool WholeRowEliminated => _wholeRowEliminated;
    private Vector3 _lastCenterPosition;
    public Vector3 LastCenterPosition => _lastCenterPosition;
    public bool isSpecial = false;

    public MoleRow(Settings settings,
                   SpawnOptions options,
                   double currentTime,
                   WhackGame whackGame,
                   MoleRowManager moleRowManager,
                   AudioSource audioSource)
    {
      _settings = settings;
      _whackGame = whackGame;
      _moleRowManager = moleRowManager;
      _audioSource = audioSource;
      hasFocus = false;
      hasExclusiveFocus = false;
      isSpecial = options.special;
      SpawnMoles(currentTime, options);
    }

    void IDisposable.Dispose()
    {
      _cts.Cancel();
      _cts.Dispose();
      _cts = null;
      RemoveAllMoles();
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      {
        bool foundFirstActiveMole = !hasFocus;
        foreach (var mole in _moles) {
          if (mole.alive && !foundFirstActiveMole) {
            foundFirstActiveMole = true;
            mole.hasFocus = true;
          } else {
            mole.hasFocus = false;
          }
        }
      }

      for (int i = _moles.Count - 1; i >= 0; i--) {
        var mole = _moles[i];
        mole.presenter.NextTick(currentTime, deltaTime);
        if (mole.activeUntil < currentTime) {
          if (mole.alive) {
            _whackGame.MoleMissed();
          }
          RemoveMole(i);
        }
      }

      {
        var positions = _moles.Where(m => m.alive).Select(m => m.position).ToList();
        if (positions.Count >= 1) {
          var p = new Vector3(positions.Average(v => v.x),
                              positions.Average(v => v.y),
                              positions.Average(v => v.z));
          _lastCenterPosition = p;
        }
      }
    }

    private async void SpawnMoles(double currentTime, SpawnOptions options)
    {
      _numInitialMoles = options.text.Length;
      _numWhackedMoles = 0;
      options.presenterOptions.moleRow = this;
      float strideX = options.size * 0.65f;
      float offsetX0 = strideX * options.text.Length * -0.5f;
      Vector3 position = options.centerPosition + new Vector3(offsetX0, 0f, 0f);
      foreach (var ch in options.text) {
        var mole = new Mole {
          position = position,
          velocity = options.velocity,
          text = ch.ToString(),
          score = 1,
          size = options.size,
          aspectRatio = options.aspectRatio,
          alive = true,
          activeUntil = currentTime + options.vulnerableTime,
          special = options.special
        };
        _moles.Add(mole);
        mole.presenter = new MolePresenter(_settings, options.presenterOptions, mole, _audioSource);
        _whackGame.AddPossibleMaximumWhackingScore(mole.score);
        position += new Vector3(strideX, 0f, 0f);

        int interval_ms = 30;
        currentTime += interval_ms * 1e-3;
        var isCanceled = await UniTask.Delay(interval_ms, cancellationToken: _cts.Token).SuppressCancellationThrow();
        if (isCanceled)
          return;
      }
    }

    private void RemoveMole(int index)
    {
      var mole = _moles[index];
      mole.presenter.Dispose();
      _moles.RemoveAt(index);
    }

    public void RemoveMole(Mole mole)
    {
      int index = _moles.LastIndexOf(mole);
      if (index >= 0)
        RemoveMole(index);
    }

    private void RemoveAllMoles()
    {
      for (int i = _moles.Count - 1; i >= 0; i--) {
        RemoveMole(i);
      }
    }

    public void WhackFirstMole()
    {
      int i = FirstActiveMoleIndex;
      if (i < 0) {
        Debug.LogError("Whack():  assertion failed: No active mole found to whack.  This is a bug.  Contact a developer.");
      }

      Mole mole = _moles[i];
      _whackGame.AddWhackingScore(mole.score, mole.position);

      mole.alive = false;
      mole.presenter.Whacked();

      _numWhackedMoles++;

      if (IsEmpty) {
        hasFocus = false;
        hasExclusiveFocus = false;
        if (_numInitialMoles == _numWhackedMoles) {
          _wholeRowEliminated = true;
        }
      } else {
        hasFocus = true;
        hasExclusiveFocus = true;
      }
    }
  }
}
