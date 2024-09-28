using Newtonsoft.Json;
using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class WhackGame : MonoBehaviour
  {
    [SerializeField] private ReplayBuffer _replayBuffer;
    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private WhackGameUiDataSource whackGameUiDataSource;
    [SerializeField] private TrackerNeuralNet _tracker;

    public App app; // injected by App
    private float _initialTime;
    private float _timeRemaining;

    public enum State
    {
      GettingReady,
      GoingOn,
      Finished,
      Closing,
    }

    private State _state;
    
    void Start()
    {
      _state = State.GettingReady;
      _initialTime = 2f;
      _timeRemaining = _initialTime + 3f;
      _replayBuffer.Clear();
    }

    void Update()
    {
      var oldTimeRemaining = _timeRemaining;
      _timeRemaining -= Time.deltaTime;
      switch (_state) {
      case State.GettingReady:
        whackGameUiDataSource.timeRemaining = _initialTime;
        if (_timeRemaining <= _initialTime) {
          _state = State.GoingOn;
        }
        break;
      case State.GoingOn:
        if (Mathf.Floor(oldTimeRemaining) != Mathf.Floor(_timeRemaining)) {
          AddReplayEntry();
        }
        if (_timeRemaining <= 0f) {
          _state = State.Finished;
          break;
        }
        whackGameUiDataSource.timeRemaining = _timeRemaining;
        break;
      case State.Finished:
        whackGameUiDataSource.timeRemaining = 0f;
        if (_timeRemaining <= -3f) {
          app.MoveFromGameToResult();
          _state = State.Closing;
        }
        break;
      case State.Closing:
        break;
      }
    }

    private void AddReplayEntry()
    {
      Debug.Log($"Adding replay entry at time {_timeRemaining}");
      var serialized = JsonConvert.SerializeObject(_tracker.spinalAlignment);
      var spinalAlignment_clone = JsonConvert.DeserializeObject<SpinalAlignment.SpinalAlignment>(serialized);
      var entry = new ReplayBuffer.ReplayEntry {
        distance = _uiDataSource.distance,
        pitch = _uiDataSource.pitch,
        spinalAlignment = spinalAlignment_clone
      };
      _replayBuffer.entries.Add(entry);
    }
  }
}
