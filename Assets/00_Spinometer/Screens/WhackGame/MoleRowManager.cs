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
  public class MoleRowManager : IDisposable
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
      public int textLengthMin; // = 1;
      public int textLengthMax; // = 1;
      public bool forceVelocity; // = false;
      public Vector3 velocity;
      public float sizeMin; // = 0.5f;
      public float sizeMax; // = 1.0f;
      public float aspectRatioMin; // = 1.0f / 1.1f;
      public float aspectRatioMax; // = 1.1f;
      public float vulnerableTimeMin; // = 0.6f;
      public float vulnerableTimeMax; // = 1.0f;
    }

    private WhackGame _whackGame;
    private AudioSource _audioSource;
    private Options _options;
    private MolePresenter.Options _presenterOptions;
    private List<MoleRow> _moleRows = new();
    private SpawnerStrategyStack _spawnerStrategyStack = new();

    public MoleRowManager(WhackGame whackGame, AudioSource audioSource, Options options, MolePresenter.Options presenterOptions)
    {
      _whackGame = whackGame;
      _audioSource = audioSource;
      _options = options;
      _presenterOptions = presenterOptions;
      _presenterOptions.moleRowManager = this;
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
      RemoveAllMoleRows();
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

      foreach (var moleRow in _moleRows) {
        moleRow.NextTick(currentTime, deltaTime);
      }

      for (int i = _moleRows.Count - 1; i >= 0; i--) {
        var moleRow = _moleRows[i];
        if (moleRow.IsEmpty) {
          RemoveMoleRow(i);
        }
      }
    }

    private void HandleKeyboardInput()
    {
      bool hit = false;
      foreach (var moleRow in _moleRows) {
        char ch = moleRow.FirstChar;
        if (((KeyControl)Keyboard.current[$"{ch}"]).wasPressedThisFrame) { 
          moleRow.WhackFirstMole();
          return;
        }
      }
      _whackGame.AddWhackingScore(-1);
    }


    public void SpawnMoleRaw(double currentTime, Options options)
    {
      int textLength = Random.Range(options.textLengthMin, options.textLengthMax);
      char RandomChar()
      {
        bool uppercase = Random.Range(0, 2) == 0;
        return (char)(uppercase ? Random.Range('A', 'Z') : Random.Range('a', 'z'));
      }
      var text = String.Join("", Enumerable.Range(0, textLength).Select(i => RandomChar()));
      MoleRow.SpawnOptions spawnOptions = new MoleRow.SpawnOptions {
        presenterOptions = _presenterOptions,
        text = text,
        centerPosition = new Vector3(Random.Range(options.spawnBoundary0.x, options.spawnBoundary1.x),
                                     Random.Range(options.spawnBoundary0.y, options.spawnBoundary1.y),
                                     Random.Range(options.spawnBoundary0.z, options.spawnBoundary1.z)),
        velocity = options.forceVelocity ? options.velocity : Random.insideUnitSphere.normalized * 0.3f,
        size = Random.Range(options.sizeMin, options.sizeMax),
        aspectRatio = Random.Range(options.aspectRatioMin, options.aspectRatioMax),
        vulnerableTime = Random.Range(options.vulnerableTimeMin, options.vulnerableTimeMax)
      };
      var moleRow = new MoleRow(spawnOptions, currentTime, _whackGame, this, _audioSource);
      _moleRows.Add(moleRow);
    }

    private void RemoveMoleRow(int index)
    {
      var moleRow = _moleRows[index];
      ((IDisposable)moleRow).Dispose();
      _moleRows.RemoveAt(index);
    }

    public void RemoveMoleRow(MoleRow moleRow)
    {
      int index = _moleRows.LastIndexOf(moleRow);
      if (index >= 0)
        RemoveMoleRow(index);
    }

    private void RemoveAllMoleRows()
    {
      for (int i = _moleRows.Count - 1; i >= 0; i--) {
        RemoveMoleRow(i);
      }
    }
  }
}
