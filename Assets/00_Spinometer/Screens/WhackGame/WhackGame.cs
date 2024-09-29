using R3;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class WhackGame : MonoBehaviour
  {
    [SerializeField] private ReplayBuffer _replayBuffer;
    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private WhackGameUiDataSource _whackGameUiDataSource;
    [SerializeField] private TrackerNeuralNet _tracker;
    [SerializeField] private WebCam _webcam;
    [SerializeField] private float _initialTime = 10f;
    [SerializeField] private MoleManager.Options _managerOptions;
    [SerializeField] private MolePresenter.Options _presenterOptions;

    public float initialTime => _initialTime;

    public App app; // injected by App
    private float _timeRemaining;
    public float timeRemaining => _timeRemaining;

    private MoleManager _moleManager;

    private CompositeDisposable _disposables;

    public enum State
    {
      GettingReady,
      GoingOn,
      Finished,
      Closing,
    }

    private State _state;
    private int _score;
    public State state => _state;

    void OnEnable()
    {
      _disposables = new CompositeDisposable();
      _presenterOptions.whackGame = this;
      _moleManager = new MoleManager(this, _managerOptions, _presenterOptions);
      _moleManager.AddTo(_disposables);
    }

    private void OnDisable()
    {
      _disposables.Dispose();
      _moleManager = null;
    }

    void Start()
    {
      _score = 0;
      _whackGameUiDataSource.score = _score;
      _state = State.GettingReady;
      _timeRemaining = initialTime + 3f;
      _replayBuffer.Clear();
    }

    void Update()
    {
      _moleManager.NextTick(Time.timeAsDouble, Time.deltaTime);

      var oldTimeRemaining = timeRemaining;
      _timeRemaining -= Time.deltaTime;
      if (Mathf.Floor(oldTimeRemaining) != Mathf.Floor(timeRemaining)) {
        OnCrossingSecondBoundary?.Invoke();
      }
      switch (_state) {
      case State.GettingReady:
        //whackGameUiDataSource.timeRemaining = initialTime;
        _whackGameUiDataSource.timeRemainingStr = $"{(Mathf.Floor(timeRemaining - initialTime) + 1):0}";
        if (timeRemaining <= initialTime) {
          _state = State.GoingOn;
          OnGameStarted?.Invoke();
        }
        break;
      case State.GoingOn:
        if (Mathf.Floor(oldTimeRemaining) != Mathf.Floor(timeRemaining)) {
          AddReplayEntry();
        }
        if (timeRemaining <= 0f) {
          _state = State.Finished;
          OnGameFinished?.Invoke();
          break;
        }
        _whackGameUiDataSource.timeRemaining = timeRemaining;
        break;
      case State.Finished:
        _whackGameUiDataSource.timeRemaining = 0f;
        if (timeRemaining <= -3f) {
          app.MoveFromGameToResult();
          _state = State.Closing;
        }
        break;
      case State.Closing:
        break;
      }
    }

    public delegate void OnGameStartedHandler();
    public event OnGameStartedHandler OnGameStarted;

    public delegate void OnGameFinishedHandler();
    public event OnGameFinishedHandler OnGameFinished;

    public delegate void OnCrossingSecondBoundaryHandler();
    public event OnCrossingSecondBoundaryHandler OnCrossingSecondBoundary;

    private void AddReplayEntry()
    {
      Debug.Log($"Adding replay entry at time {timeRemaining}");

      var inTex = _webcam.ColorRenderTexture;
      var textureCopy = new Texture2D(inTex.width, inTex.height, DefaultFormat.LDR, TextureCreationFlags.None);
      Graphics.CopyTexture(inTex, textureCopy);

      var entry = new ReplayBuffer.ReplayEntry {
        texture = textureCopy, 
        distance = _uiDataSource.distance,
        pitch = _uiDataSource.pitch,
        spinalAlignment = _tracker.spinalAlignment.Clone(),
        spinalAlignmentScore = _tracker.spinalAlignmentScore.Clone()
      };
      _replayBuffer.entries.Add(entry);
    }

    public void AddScore(int value)
    {
      _score += value;
      _whackGameUiDataSource.score = _score;
    }
  }
}
