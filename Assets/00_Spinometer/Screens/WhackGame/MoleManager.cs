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
      composite,
    }

    [Serializable]
    public struct Options
    {
      public SpawnerStrategy spawnerStrategy;
      public CompositeSS.Options compositeSSOptions;
      public Vector3 spawnBoundary0; // = new Vector3(-2f, -1f, -1.4f);
      public Vector3 spawnBoundary1; // = new Vector3(2f, 1f, -1.2f);
      public float sizeMin; // = 0.5f;
      public float sizeMax; // = 1.0f;
      public float aspectRatioMin; // = 1.0f / 1.1f;
      public float aspectRatioMax; // = 1.1f;
      public float vulnerableTimeMin; // = 0.6f;
      public float vulnerableTimeMax; // = 1.0f;
      public AudioClip spawningClip;
    }

    private WhackGame _whackGame;
    private AudioSource _audioSource;
    private Options _options;
    private MolePresenter.Options _presenterOptions;
    private List<Mole> _moles = new List<Mole>();
    private SpawnerStrategyStack _spawnerStrategyStack = new();

    public MoleManager(WhackGame whackGame, AudioSource audioSource, Options options, MolePresenter.Options presenterOptions)
    {
      _whackGame = whackGame;
      _audioSource = audioSource;
      _options = options;
      _presenterOptions = presenterOptions;
      _presenterOptions.moleManager = this;
    }

    public WhackGame whackGame => _whackGame;

    public Options options => _options;

    public SpawnerStrategyStack strategyStack => _spawnerStrategyStack;

    public void PushSpawnerStrategy(SpawnerStrategy strategyEnum)
    {
      ISpawnerStrategy ss = strategyEnum switch {
        SpawnerStrategy.periodic => new PeriodicSS(this),
        SpawnerStrategy.random => new RandomSS(this),
        SpawnerStrategy.burst => new BurstSS(this),
        SpawnerStrategy.composite => new CompositeSS(this, _options.compositeSSOptions)
      };
      _spawnerStrategyStack.Push(ss);
    }

    private void ChangeSpawnerStrategy(SpawnerStrategy strategyEnum)
    {
      _spawnerStrategyStack.Clear();
      PushSpawnerStrategy(strategyEnum);
    }

    void IDisposable.Dispose()
    {
      ((IDisposable)_spawnerStrategyStack).Dispose();
      RemoveAllMoles();
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      if (_whackGame.state != WhackGame.State.GoingOn)
        return;

      if (_spawnerStrategyStack.IsEmpty()) {
        ChangeSpawnerStrategy(_options.spawnerStrategy);
      }

      _spawnerStrategyStack.NextTick(currentTime, deltaTime);

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
          _whackGame.AddWhackingScore(mole.score);
          WhackMole(i);
          return; // only one mole can be hit at a time
        }
      }

      if (!hit) {
        _whackGame.AddWhackingScore(-1);
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
      return Spawn(currentTime, _options);
    }

    public Mole Spawn(double currentTime, Options options)
    {
      bool uppercase = Random.Range(0, 2) == 0;
      var text = ((char)(uppercase ? Random.Range('A', 'Z') : Random.Range('a', 'z'))).ToString();
      var mole = new Mole {
        position = new Vector3(Random.Range(options.spawnBoundary0.x, options.spawnBoundary1.x),
                               Random.Range(options.spawnBoundary0.y, options.spawnBoundary1.y),
                               Random.Range(options.spawnBoundary0.z, options.spawnBoundary1.z)),
        text = text,
        score = 1,
        size = Random.Range(options.sizeMin, options.sizeMax),
        aspectRatio = Random.Range(options.aspectRatioMin, options.aspectRatioMax),
        alive = true,
        activeUntil = currentTime + Random.Range(options.vulnerableTimeMin, options.vulnerableTimeMax)
      };
      _moles.Insert(0, mole); // Insert() instead of Add() to ensure whacking is applied to oldest moles first
      mole.presenter = new MolePresenter(_presenterOptions, mole);
      _whackGame.AddPossibleMaximumWhackingScore(mole.score);
      _audioSource.PlayOneShot(_options.spawningClip, 0.5f);
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
