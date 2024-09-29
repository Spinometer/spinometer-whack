using System;
using System.Collections.Generic;
using System.Linq;
using GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class MoleManager : IDisposable
  {
    public enum SpawnerStrategy {
      periodic,
      random,
      burst,
    }

    [Serializable]
    public struct Options
    {
      public SpawnerStrategy spawnerStrategy;
      public Vector3 spawnBoundary0; // = new Vector3(-2f, -1f, -1.4f);
      public Vector3 spawnBoundary1; // = new Vector3(2f, 1f, -1.2f);
      public float vulnerableTimeMin; // = 0.6f;
      public float vulnerableTimeMax; // = 1.0f;
    }

    WhackGame _whackGame;
    private Options _options;
    private MolePresenter.Options _presenterOptions;
    private List<Mole> _moles = new List<Mole>();
    private ISpawnerStrategy _spawnerStrategy = null;

    public MoleManager(WhackGame whackGame, Options options, MolePresenter.Options presenterOptions)
    {
      _whackGame = whackGame;
      _options = options;
      _presenterOptions = presenterOptions;
      _presenterOptions.moleManager = this;
      ChangeSpawnerStrategy(_options.spawnerStrategy);
    }

    public WhackGame whackGame => _whackGame;

    private void ChangeSpawnerStrategy(SpawnerStrategy strategyEnum)
    {
      if (_spawnerStrategy != null) {
        _spawnerStrategy.Dispose();
        _spawnerStrategy = null;
      }
      switch (strategyEnum) {
      case SpawnerStrategy.periodic:
        _spawnerStrategy = new PeriodicSS(this);
        break;
      case SpawnerStrategy.random:
        _spawnerStrategy = new RandomSS(this);
        break;
      case SpawnerStrategy.burst:
        _spawnerStrategy = new BurstSS(this);
        break;
      }
    }

    void IDisposable.Dispose()
    {
      _spawnerStrategy.Dispose();
      RemoveAllMoles();
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      if (_whackGame.state != WhackGame.State.GoingOn)
        return;

      _spawnerStrategy?.NextTick(currentTime, deltaTime);

      bool anyKeyPressedThisFrame = Keyboard.current.allKeys.Any(key => key.wasPressedThisFrame);
      // Keyboard.current.allKeys can not be used here as it does not handle roll over.
      if (anyKeyPressedThisFrame) {
        HandleKeyboardInput();
      }

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
        if (!mole.alive)
          continue;
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

    public Mole Spawn(double currentTime)
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
      _moles.Insert(0, mole); // Insert() instead of Add() to ensure whacking is applied to oldest moles first
      mole.presenter = new MolePresenter(_presenterOptions, mole);
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
