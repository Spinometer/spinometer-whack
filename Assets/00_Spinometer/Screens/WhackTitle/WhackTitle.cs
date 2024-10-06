using GetBack.Spinometer.Screens.WhackGame;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GetBack.Spinometer.Screens.WhackTitle
{
  public class WhackTitle : MonoBehaviour
  {
    [SerializeField] private Settings _settings;
    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private WhackGameUiDataSource _whackGameUiDataSource;
    [SerializeField] private TrackerNeuralNet _tracker;
    [SerializeField] private WebCam _webcam;
    
    private AudioSource _audioSource;

    public App _app; // injected by App
    public App app { set => _app = value; }

    private CompositeDisposable _disposables;

    void Awake()
    {
      _audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
      _disposables = new CompositeDisposable();
    }

    void OnDisable()
    {
      _disposables.Dispose();
    }

    void Update()
    {
      if (Keyboard.current.spaceKey.wasPressedThisFrame) {
        _app.MoveFromTitleToGame();
      }
    }
  }
}
