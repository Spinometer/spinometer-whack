using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class MoleManager : IDisposable
  {
    [Serializable]
    public struct Options
    {
      public Vector3 spawnBoundary0; // = new Vector3(-2f, -1f, -1.4f);
      public Vector3 spawnBoundary1; // = new Vector3(2f, 1f, -1.2f);
      public float vulnerableTimeMin; // = 0.6f;
      public float vulnerableTimeMax; // = 1.0f;
    }

    WhackGame _whackGame;
    private Options _options;
    private MolePresenter.Options _presenterOptions;
    private List<Mole> _moles = new List<Mole>();

    public MoleManager(WhackGame whackGame, Options options, MolePresenter.Options presenterOptions)
    {
      _whackGame = whackGame;
      _options = options;
      _presenterOptions = presenterOptions;
      _presenterOptions.moleManager = this;
      _whackGame.OnCrossingSecondBoundary += OnCrossingSecondBoundary;
    }

    private void OnCrossingSecondBoundary()
    {
      if (_whackGame.state == WhackGame.State.GettingReady &&
          _whackGame.timeRemaining <= _whackGame.initialTime + 0.01f)
        Spawn(Time.timeAsDouble);
      else if (_whackGame.state == WhackGame.State.GoingOn && _whackGame.timeRemaining >= 0.5f)
        Spawn(Time.timeAsDouble);
    }

    void IDisposable.Dispose()
    {
      _whackGame.OnCrossingSecondBoundary -= OnCrossingSecondBoundary;
      RemoveAllMoles();
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      if (_whackGame.state != WhackGame.State.GoingOn)
        return;

      if (Keyboard.current.anyKey.wasPressedThisFrame)
        HandleKeyboardInput();

      for (int i = _moles.Count - 1; i >= 0; i--) {
        var mole = _moles[i];
        mole.presenter.NextTick(currentTime, deltaTime);
        if (mole.activeUntil < currentTime) {
          RemoveMole(i);
        }
      }
    }

    private void HandleKeyboardInput()
    {
      bool hit = false;
      for (int i = _moles.Count - 1; i >= 0; i--) {
        var mole = _moles[i];
        if (((KeyControl)(Keyboard.current[mole.text])).wasPressedThisFrame) {
          hit = true;
          _whackGame.AddScore(mole.score);
          WhackMole(i);
        }
      }

      if (!hit) {
        _whackGame.AddScore(-1);
      }
    }

    private void WhackMole(int index)
    {
      var mole = _moles[index];
      mole.alive = false;
      mole.presenter.Whacked();
    }

    private Mole Spawn(double currentTime)
    {
      bool uppercase = Random.Range(0, 2) == 0;
      var text = ((char)(uppercase ? Random.Range('A', 'Z') : Random.Range('a', 'z'))).ToString();
      var mole = new Mole {
        position = new Vector3(Random.Range(_options.spawnBoundary0.x, _options.spawnBoundary1.x),
                               Random.Range(_options.spawnBoundary0.y, _options.spawnBoundary1.y),
                               Random.Range(_options.spawnBoundary0.z, _options.spawnBoundary1.z)),
        text = text,
        score = 1,
        size = Random.Range(0.5f, 1f),
        aspectRatio = Random.Range(1.0f / 1.2f, 1.2f),
        alive = true,
        activeUntil = currentTime + Random.Range(_options.vulnerableTimeMin, _options.vulnerableTimeMax)
      };
      _moles.Add(mole);
      mole.presenter = new MolePresenter(_presenterOptions, mole);
      Debug.Log(_moles.Count);
      return mole;
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
  }
}
