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
      public bool special; // = false;
    }

    private Settings _settings;
    private WhackGame _whackGame;
    private AudioSource _audioSource;
    private Options _options;
    private MolePresenter.Options _presenterOptions;
    private List<MoleRow> _moleRows = new();
    private SpawnerStrategyStack _spawnerStrategyStack = new();
    private Dictionary<char, bool> _occupiedHeadChars = new();

    public MoleRowManager(Settings settings, WhackGame whackGame, AudioSource audioSource, Options options, MolePresenter.Options presenterOptions)
    {
      _settings = settings;
      _whackGame = whackGame;
      _audioSource = audioSource;
      _options = options;
      _presenterOptions = presenterOptions;
      _presenterOptions.moleRowManager = this;
    }

    public WhackGame whackGame => _whackGame;

    public Options options => _options;

    public SpawnerStrategyStack strategyStack => _spawnerStrategyStack;

    public void PushSpawnerStrategy(SpawnerStrategy strategyEnum, double currentTime, bool special = false)
    {
      ISpawnerStrategy ss = strategyEnum switch {
        SpawnerStrategy.periodic => new PeriodicSS(_settings, this, currentTime, special),
        SpawnerStrategy.random => new RandomSS(_settings, this),
        SpawnerStrategy.burst => new BurstSS(_settings, this, currentTime, special),
        SpawnerStrategy.composite => new CompositeSS(_settings, this, currentTime, _options.compositeSSOptions)
      };
      _spawnerStrategyStack.Push(ss);
    }

    private void ChangeSpawnerStrategy(SpawnerStrategy strategyEnum, double currentTime)
    {
      _spawnerStrategyStack.Clear();
      PushSpawnerStrategy(strategyEnum, currentTime);
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
        ChangeSpawnerStrategy(_options.spawnerStrategy, currentTime);
      }

      _spawnerStrategyStack.NextTick(currentTime, deltaTime);

      bool anyKeyPressedThisFrame = Keyboard.current.allKeys.Any(key => key.wasPressedThisFrame);
      // Keyboard.current.allKeys can not be used here as it does not handle roll over.
      if (anyKeyPressedThisFrame) {
        HandleKeyboardInput();
      }

      // update occupied head chars
      {
        // FIXME:  improve performance
        _occupiedHeadChars.Clear();
        foreach (var moleRow in _moleRows) {
          if (moleRow.hasExclusiveFocus)
            continue;
          char ch = moleRow.FirstChar;
          _occupiedHeadChars[Char.ToLower(ch)] = true;
        }
      }

      // update focus
      if (AnyMoleRowHasExclusiveFocus()) {
        foreach (var moleRow in _moleRows) {
          if (moleRow.hasExclusiveFocus) {
            moleRow.hasFocus = true;
          } else {
            moleRow.hasFocus = false;
            moleRow.hasExclusiveFocus = false;
          }
        }
      } else {
        foreach (var moleRow in _moleRows) {
          moleRow.hasFocus = !moleRow.IsEmpty;
        }
      }

      foreach (var moleRow in _moleRows) {
        moleRow.NextTick(currentTime, deltaTime);
      }

      for (int i = _moleRows.Count - 1; i >= 0; i--) {
        var moleRow = _moleRows[i];
        if (moleRow.IsEmpty) {
          if (moleRow.WholeRowEliminated) {
            _whackGame.WholeRowEliminated(moleRow);
          } else {
            _whackGame.AddWhackingScore(-1, moleRow.LastCenterPosition, false);
          } 
          RemoveMoleRow(i);
        }
      }
    }

    private void HandleKeyboardInput()
    {
      bool hit = false;
      bool anyExclusive = AnyMoleRowHasExclusiveFocus();
      foreach (var moleRow in _moleRows) {
        bool hasFocus = (anyExclusive && moleRow.hasExclusiveFocus) ||
                        (!anyExclusive && moleRow.hasFocus);
        if (!hasFocus)
          continue;
        char ch = moleRow.FirstChar;
        if (((KeyControl)Keyboard.current[$"{ch}"]).wasPressedThisFrame) { 
          moleRow.WhackFirstMole();
          return;
        }
      }
      _whackGame.MoleMissed();
      _whackGame.AddWhackingScore(-1, new Vector3(1.42f, 0.34f, 0f));
    }


    public void SpawnMoleRow(double currentTime, Options options)
    {
      if (_occupiedHeadChars.Count >= 26)
        return;

      int textLength = Random.Range(options.textLengthMin, options.textLengthMax);
      char RandomChar()
      {
        bool uppercase = Random.Range(0, 2) == 0;
        return (char)(uppercase ? Random.Range('A', 'Z' + 1) : Random.Range('a', 'z' + 1));
      }
      char RandomFirstChar()
      {
        for (;;) {
          var ch = RandomChar();
          if (!_occupiedHeadChars.ContainsKey(Char.ToLower(ch)))
            return ch;
        }
      }

      var text = RandomFirstChar() + String.Join("", Enumerable.Range(1, textLength).Select(i => RandomChar()));
      MoleRow.SpawnOptions spawnOptions = new MoleRow.SpawnOptions {
        presenterOptions = _presenterOptions,
        text = text,
        centerPosition = new Vector3(Random.Range(options.spawnBoundary0.x, options.spawnBoundary1.x),
                                     Random.Range(options.spawnBoundary0.y, options.spawnBoundary1.y),
                                     Random.Range(options.spawnBoundary0.z, options.spawnBoundary1.z)),
        velocity = options.forceVelocity ? options.velocity : Random.insideUnitSphere.normalized * 0.3f,
        size = Random.Range(options.sizeMin, options.sizeMax),
        aspectRatio = Random.Range(options.aspectRatioMin, options.aspectRatioMax),
        vulnerableTime = Random.Range(options.vulnerableTimeMin, options.vulnerableTimeMax),
        special = options.special
      };
      var moleRow = new MoleRow(_settings, spawnOptions, currentTime, _whackGame, this, _audioSource);
      moleRow.hasFocus = !AnyMoleRowHasExclusiveFocus();
      moleRow.hasExclusiveFocus = false;
      _moleRows.Add(moleRow);
    }

    public bool AnyMoleRowHasExclusiveFocus()
    {
      return _moleRows.Any(moleRow => moleRow.hasExclusiveFocus);
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

    public int CountActiveMoleRows()
    {
      return _moleRows.Count(moleRow => !moleRow.IsEmpty);
    }
  }
}
