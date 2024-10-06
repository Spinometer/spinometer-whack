using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Drawing;
using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class ComboGuageView : MonoBehaviour
  {
    [SerializeField] WhackGame _whackGame;

    private Transform _tr;
    private float _value = 0f;
    private float _delayedValue = 0f;
    TweenerCore<float, float, FloatOptions> _tween = null;

    void Awake()
    {
      _tr = transform;
    }

    void Start()
    {
      Reset(_whackGame.comboGuageValue);
    }

    void Update()
    {
      float newValue = _whackGame.comboGuageValue;
      if (_value != newValue) {
        KillTween();
        _delayedValue = _value;
        _value = newValue;
        _tween = DOTween.To(() => _delayedValue, x => _delayedValue = x, _value, 0.5f).SetLink(gameObject);
        _tween.onComplete += () => _tween = null;
        _tween.Play();
      }

      float normalizedValue = _value / _whackGame.maxComboGuageValue;

      float y0 = _tr.position.y - _tr.localScale.y * 0.5f;
      float y1 = _tr.position.y + _tr.localScale.y * 0.5f;
      using (Draw.ingame.WithColor(Color.gray)) {
        DrawRegion(y0, normalizedValue, 1f);
      }

      {
        float v1 = normalizedValue;
        if (_delayedValue < _value) {
          v1 = _delayedValue / _whackGame.maxComboGuageValue;
          using (Draw.ingame.WithColor(Color.green)) {
            DrawRegion(y0, v1, normalizedValue);
          }
        }
        if (_delayedValue > _value) {
          float v1_ = _delayedValue / _whackGame.maxComboGuageValue;
          using (Draw.ingame.WithColor(Color.red)) {
            DrawRegion(y0, normalizedValue, v1_);
          }
        }
        using (Draw.ingame.WithColor(Color.cyan)) {
          DrawRegion(y0, 0f, v1);
        }
      }
      using (Draw.ingame.WithColor(Color.black)) {
        for (int i = 1; i <= Mathf.Floor(_whackGame.maxComboGuageValue - 0.001f); i++) {
          float v = i / _whackGame.maxComboGuageValue;
          float x = NormalizedValueToX(v);
          Draw.ingame.Line(new Vector3(x, y0, 0f), new Vector3(x, y1, 0f));
        }
      }
      using (Draw.ingame.WithColor(Color.white)) {
        float width = _tr.localScale.x;
        float height = _tr.localScale.y;
        Draw.ingame.WireRectangle(new Rect(_tr.position - new Vector3(width * 0.5f, height * 0.5f, 0f),
                                           new Vector2(width, height)));
      }
    }

    private void DrawRegion(float y0, float v0, float v1)
    {
      float x0 = NormalizedValueToX(v0);
      float x1 = NormalizedValueToX(v1);
      Vector2 origin = new(x0, y0);
      Vector2 size = new(x1 - x0, _tr.localScale.y);
      Draw.ingame.SolidRectangle(new Rect(origin, size));
    }

    private float NormalizedValueToX(float v)
    {
      return _tr.position.x + _tr.localScale.x * (v - 0.5f);
    }

    public void Reset(float value)
    {
      KillTween();
      _value = value;
      _delayedValue = _value;
    }

    private void KillTween()
    {
      if (_tween == null)
        return;
      if (_tween.active)
        _tween.Complete();
      _tween = null;
    }
  }
}
