using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class MolePresenter : IMolePresenter
  {
    [Serializable]
    public struct Options
    {
      public WhackGame whackGame;
      public MoleManager moleManager;
      public GameObject molePrefab;
    }

    private Options _options;
    private GameObject _moleGO;
    private Mole _mole;

    internal MolePresenter(Options options, Mole mole)
    {
      _options = options;
      _mole = mole;
      _mole.presenter = this;
      _moleGO = Object.Instantiate(_options.molePrefab, _mole.position, Quaternion.identity);
      _moleGO.transform.localScale = new Vector3(_mole.size / _mole.aspectRatio, _mole.size * _mole.aspectRatio, _mole.size);
      var t = _moleGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
      t.text = _mole.text;
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      var whackGame = _options.whackGame;
      switch (whackGame.state) {
      case WhackGame.State.GettingReady:
        break;
      case WhackGame.State.GoingOn:
        break;
      case WhackGame.State.Finished:
        break;
      default:
        _options.moleManager.RemoveMole(_mole);
        break;
      }
    }

    void IDisposable.Dispose()
    {
      Object.Destroy(_moleGO);
      _moleGO = null;
    }
  }
}
