using Drawing;
using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class ComboGuageView : MonoBehaviour
  {
    [SerializeField] WhackGame _whackGame;

    private Transform _tr;
    private float _value = 0f;

    void Awake()
    {
      _tr = transform;
    }

    void Start()
    {
      _value = _whackGame.comboGuageValue;
    }

    void Update()
    {
      float newValue = _whackGame.comboGuageValue;
      if (_value != newValue)
        _value = newValue;

      float normalizedValue = _value / _whackGame.maxComboGuageValue;

      float y0 = _tr.position.y - _tr.localScale.y * 0.5f;
      float y1 = _tr.position.y + _tr.localScale.y * 0.5f;
      using (Draw.ingame.WithColor(Color.gray)) {
        DrawRegion(y0, normalizedValue, 1f);
      }
      using (Draw.ingame.WithColor(Color.cyan)) {
        DrawRegion(y0, 0f, normalizedValue);
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
      _value = value;
    }
  }
}
