using DG.DemiEditor.DeGUINodeSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class WhackGameEffects : MonoBehaviour
  {
    [SerializeField] private WhackGameUiDataSource _whackGameUiDataSource;
    [SerializeField] private UIDocument _uidoc;

    private WhackGame _whackGame;
    void Awake()
    {
      _whackGame = GetComponent<WhackGame>();
    }

    void OnEnable()
    {
      _whackGame.OnCrossingSecondBoundary += HandleCrossingSecondBoundary;
    }

    void OnDisable()
    {
      _whackGame.OnCrossingSecondBoundary += HandleCrossingSecondBoundary;
    }

    void Start()
    {
    }

    private void HandleCrossingSecondBoundary()
    {
      switch (_whackGame.state) {
      case WhackGame.State.GettingReady:
        if (_whackGame.timeRemaining > _whackGame.initialTime + 0.3f) {
          _whackGameUiDataSource.text_ready_opacity = 1.0f;
          _whackGameUiDataSource.text_ready_scale = Vector3.one;
          var tw = DOTween.To(() => _whackGameUiDataSource.text_ready_opacity,
                              x => _whackGameUiDataSource.text_ready_opacity = x, 0.3f, 0.2f);
          tw.Play();
        } else {
          _whackGameUiDataSource.text_ready_opacity = 1.0f;
          _whackGameUiDataSource.text_ready_scale = Vector3.one;
          {
            var tw = DOTween.To(() => _whackGameUiDataSource.text_ready_opacity,
                                x => _whackGameUiDataSource.text_ready_opacity = x, 0.0f, 1.0f);
            tw.Play();
          }
          {
            var tw = DOTween.To(() => _whackGameUiDataSource.text_ready_scale,
                                x => _whackGameUiDataSource.text_ready_scale = x, new Vector3(10f, 10f, 10f), 1.0f);
            tw.Play();
          }
        }
        break;
      default:
        break;
      }
    }
  }
}
