using System;
using DG.Tweening;
using Drawing;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class MolePresenter : IMolePresenter
  {
    [Serializable]
    public struct Options
    {
      public WhackGame whackGame;
      public MoleRowManager moleRowManager;
      public MoleRow moleRow;
      public GameObject molePrefab;
      public AudioClip spawningClip;
      public AudioClip spawningSpecialClip;
      public GameObject whackedVfx;
      public GameObject specialMoleVfx;
    }

    private Options _options;
    private GameObject _moleGO;
    private GameObject _specialMoleVfxGo;
    private Mole _mole;
    private AudioSource _audioSource;
    private readonly Settings _settings;

    internal MolePresenter(Settings settings, Options options, Mole mole, AudioSource audioSource)
    {
      _settings = settings;
      _options = options;
      _mole = mole;
      _mole.presenter = this;
      _audioSource = audioSource;
      _moleGO = Object.Instantiate(_options.molePrefab, _mole.position, Quaternion.identity);
      _specialMoleVfxGo = null;
      _moleGO.transform.localScale = new Vector3(_mole.size / _mole.aspectRatio, _mole.size * _mole.aspectRatio, _mole.size);
      var t = _moleGO.GetComponentInChildren<TextMeshProUGUI>();
      t.text = _mole.text;
      if (_mole.special) {
        t.color = new Color(1f, 1f, 0.5f, 1f);
      }

      float duration = (float)(mole.activeUntil - Time.timeAsDouble);
      {
        // fade out
        var tw = DOTween.To(() => t.color,
                            x => { t.color = x; },
                            new Color(1f, 1f, 1f, 0f),
                            0.3f)
          .SetDelay(duration - 0.3f).SetLink(_moleGO);
        tw.Play();
      }
      {
        // translation
        var tr = _moleGO.transform;
        var pos0 = tr.localPosition;
        var pos1 = pos0 + mole.velocity * duration;
        var tw = DOTween.To(() => tr.localPosition,
                            x => {
                              _mole.position = x;
                              tr.localPosition = x;
                            },
                            pos1,
                            duration).SetLink(_moleGO);
        tw.Play();
      }
      {
        // wave
        var tr = _moleGO.transform;
        _mole.wavePhase = 0f;
        float speed = 1.4f * 360f * Mathf.Sqrt(_settings.velocityMultiplier); // degree per second
        float amplitude = 0.05f * Mathf.Sqrt(_settings.velocityMultiplier);
        var tw = DOTween.To(() => _mole.wavePhase,
                            deg => {
                              _mole.wavePhase = deg;
                              var pos = _mole.position;
                              tr.localPosition = pos + new Vector3(0f, Mathf.Sin(deg * Mathf.Deg2Rad) * amplitude, 0f);;
                            },
                            speed * duration, duration).SetLink(_moleGO);
        tw.Play();
      }
      if (_mole.special) {
        {
          // special moles spawining effect
          var tr = _moleGO.transform;
          float a = 0f;
          var tw = DOTween.To(() => a,
                              value =>
                              {
                                a = value;
                                using (Draw.ingame.WithColor(new Color(1f, 1f, 1f, a))) {
                                  Draw.ingame.Circle(tr.position, Vector3.back, ((1.1f - a) * 4f) * Mathf.Sqrt(_mole.size));
                                }
                              },
                              1f, 0.5f).SetLink(_moleGO);
          tw.Play();
        }
        {
          // special moles vfx
          var vfxOffset = Vector3.down * _mole.size * 0.3f;
          var pos0 = _mole.presenter.moleGO().transform.position;
          _specialMoleVfxGo = Object.Instantiate(_options.specialMoleVfx, pos0 + vfxOffset, Quaternion.identity);
          var vfx = _specialMoleVfxGo.GetComponent<VisualEffect>();
          vfx.SetFloat("moleSize", Mathf.Sqrt(_mole.size));
        }
      }

      if (!_mole.special) {
        _audioSource.PlayOneShot(_options.spawningClip, 0.5f);
      } else {
        _audioSource.PlayOneShot(_options.spawningSpecialClip, 0.5f);
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
        if (_mole.hasFocus) {
          DrawCursor(_mole);
        }
        break;
      case WhackGame.State.Finished:
        break;
      default:
        _options.moleRow.RemoveMole(_mole);
        break;
      }

      if (_specialMoleVfxGo) {
        var vfxOffset = Vector3.down * _mole.size * 0.3f;
        var pos0 = _moleGO.transform.position;
        _specialMoleVfxGo.transform.localPosition = pos0 + vfxOffset;
      }
    }

    private void DrawCursor(Mole mole)
    {
      Vector3 center = mole.position;
      float width = mole.size * 0.6f;
      float height = mole.size * 1.0f;
      using (Draw.ingame.WithColor(Color.white)) {
        Draw.ingame.WireRectangle((float3)center,
                                  Quaternion.AngleAxis(90f, Vector3.right),
                                  new Vector2(width, height));
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

      {
        // vfx stuff
        var vfxOffset = Vector3.down * _mole.size * 0.3f;
        var pos0 = _mole.presenter.moleGO().transform.position;
        var go = Object.Instantiate(_options.whackedVfx, pos0 + vfxOffset, Quaternion.identity);
        var vfx = go.GetComponent<VisualEffect>();
        vfx.SetFloat("moleSize", Mathf.Sqrt(_mole.size));
        Object.Destroy(go, 2.0f);
      }
    }

    void IDisposable.Dispose()
    {
      Object.Destroy(_moleGO);
      if (_specialMoleVfxGo != null) {
        Object.Destroy(_specialMoleVfxGo);
        _specialMoleVfxGo = null;
      }
      _moleGO = null;
    }
  }
}
