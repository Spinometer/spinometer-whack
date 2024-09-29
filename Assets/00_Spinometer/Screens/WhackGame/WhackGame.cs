using Newtonsoft.Json;
using R3;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class WhackGame : MonoBehaviour
  {
    [SerializeField] private ReplayBuffer _replayBuffer;
    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private WhackGameUiDataSource whackGameUiDataSource;
    [SerializeField] private TrackerNeuralNet _tracker;
    [SerializeField] private WebCam _webcam;
    [SerializeField] private float _initialTime = 10f;
    [SerializeField] private Vector3 _spawnBoundary0 = new Vector3(-2f, -1f, 0f);
    [SerializeField] private Vector3 _spawnBoundary1 = new Vector3(2f, 1f, 0f);
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
      _moleManager = new MoleManager(this, _spawnBoundary0, _spawnBoundary1, _presenterOptions);
      _moleManager.AddTo(_disposables);
    }

    private void OnDisable()
    {
      _disposables.Dispose();
      _moleManager = null;
    }

    void Start()
    {
      _state = State.GettingReady;
      //_initialTime = 10f;
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
        whackGameUiDataSource.timeRemaining = initialTime;
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
        whackGameUiDataSource.timeRemaining = timeRemaining;
        break;
      case State.Finished:
        whackGameUiDataSource.timeRemaining = 0f;
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

      var serialized = JsonConvert.SerializeObject(_tracker.spinalAlignment);
      var spinalAlignment_clone = JsonConvert.DeserializeObject<SpinalAlignment.SpinalAlignment>(serialized);
      var entry = new ReplayBuffer.ReplayEntry {
        texture = textureCopy, 
        distance = _uiDataSource.distance,
        pitch = _uiDataSource.pitch,
        spinalAlignment = spinalAlignment_clone
      };
      _replayBuffer.entries.Add(entry);
    }

    public void AddScore(int value)
    {
      _score += value;
    }
  }
}
