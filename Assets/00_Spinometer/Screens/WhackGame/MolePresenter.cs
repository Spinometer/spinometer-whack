using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class MolePresenter : IMolePresenter
  {
    [Serializable]
    public struct Options
    {
      public WhackGame whackGame;
      public MoleRowManager MoleRowManager;
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
      var t = _moleGO.GetComponentInChildren<TextMeshProUGUI>();
      t.text = _mole.text;
      float duration = (float)(mole.activeUntil - Time.timeAsDouble);
      {
        var tw = DOTween.To(() => t.color,
                            x => { t.color = x; },
                            new Color(1f, 1f, 1f, 0f),
                            0.3f)
          .SetDelay(duration - 0.3f).SetLink(_moleGO);
        tw.Play();
      }
      {
        var tr = _moleGO.transform;
        var pos0 = tr.localPosition;
        var pos1 = pos0 + mole.velocity * duration;
        var tw = DOTween.To(() => tr.localPosition,
                            x => { tr.localPosition = x; },
                            pos1,
                            duration).SetLink(_moleGO);
        tw.Play();
      }
    }

    public GameObject moleGO()
    {
      return _moleGO;
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
        _options.MoleRowManager.RemoveMole(_mole);
        break;
      }
    }

    public void Whacked()
    {
      var t = _moleGO.GetComponentInChildren<TextMeshProUGUI>();
      t.color = new Color(1f, 0.3f, 0.3f, 1f);
      var tw = DOTween.To(() => t.color,
                          x => { t.color = x; },
                          new Color(1f, 0f, 0f, 0f),
                          1f).SetLink(_moleGO);
      tw.Play();
      _mole.activeUntil = Time.timeAsDouble + 1.0;
    }

    void IDisposable.Dispose()
    {
      Object.Destroy(_moleGO);
      _moleGO = null;
    }
  }
}
