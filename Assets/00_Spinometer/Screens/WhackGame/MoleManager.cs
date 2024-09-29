using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class MoleManager : IDisposable
  {
    WhackGame _whackGame;
    private Vector3 _spawnBoundary0;
    private Vector3 _spawnBoundary1;
    private MolePresenter.Options _presenterOptions;
    private List<Mole> _moles = new List<Mole>();

    public MoleManager(WhackGame whackGame, Vector3 spawnBoundary0, Vector3 spawnBoundary1, MolePresenter.Options presenterOptions)
    {
      _whackGame = whackGame;
      _spawnBoundary0 = spawnBoundary0;
      _spawnBoundary1 = spawnBoundary1;
      _presenterOptions = presenterOptions;
      _presenterOptions.moleManager = this;
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      if (_whackGame.state != WhackGame.State.GoingOn)
        return;

      if (Random.value < deltaTime) {
        Spawn(currentTime);
      }

      if (Keyboard.current.anyKey.IsPressed())
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
      foreach (var mole in _moles) {
        if (Keyboard.current[mole.text].IsPressed()) {
          hit = true;
          _whackGame.AddScore(mole.score);
          RemoveMole(mole);
        }
      }

      if (!hit) {
        _whackGame.AddScore(-1);
      }
    }

    private Mole Spawn(double currentTime)
    {
      bool uppercase = Random.Range(0, 2) == 0;
      var text = ((char)(uppercase ? Random.Range('A', 'Z') : Random.Range('a', 'z'))).ToString();
      var mole = new Mole {
        position = new Vector3(Random.Range(_spawnBoundary0.x, _spawnBoundary1.x),
                               Random.Range(_spawnBoundary0.y, _spawnBoundary1.y),
                               Random.Range(_spawnBoundary0.z, _spawnBoundary1.z)),
        text = text,
        score = 1,
        size = Random.Range(0.5f, 1f),
        aspectRatio = Random.Range(1.0f / 1.2f, 1.2f),
        alive = true,
        activeUntil = currentTime + Random.Range(.4f, .5f)
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

    void IDisposable.Dispose()
    {
      RemoveAllMoles();
    }
  }
}
