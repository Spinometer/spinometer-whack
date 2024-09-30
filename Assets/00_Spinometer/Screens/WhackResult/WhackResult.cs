using System.Linq;
using GetBack.Spinometer.SpinalAlignmentVisualizer;
using UnityEngine;
using UnityEngine.UIElements;

namespace GetBack.Spinometer.Screens.WhackResult
{
  public class WhackResultScreen : MonoBehaviour
  {
    [SerializeField] private GameStateLog _gameStateLog;
    [SerializeField] private ReplayBuffer _replayBuffer;
    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private WhackResultUiDataSource _whackResultUiDataSource;
    [SerializeField] private Renderer _webcamPlane;

    public enum State
    {
      Playing,
      Stopped,
      Closing,
    }

    private SpinalAlignmentVisualizerSkeleton _visualizerSkeleton = null;
    private SpinalAlignmentVisualizerStickFigure _visualizerStickFigure = null;
    State _state;
    private float _playbackTimer;
    private float _playbackSpeed;

    private void Awake()
    {
      _visualizerSkeleton = GetComponent<SpinalAlignmentVisualizerSkeleton>();
      _visualizerStickFigure = GetComponent<SpinalAlignmentVisualizerStickFigure>();
    }

    void Start()
    {
      _visualizerStickFigure.ShowAlignmentValues = true;
      _whackResultUiDataSource.seekMax = _replayBuffer.entries.Count - 1;
      _whackResultUiDataSource.seekPosition = 0;
      StartPlaying();

      var root = GameObject.Find("/WhackResultUIDocument").GetComponent<UIDocument>().rootVisualElement;
      root.Q<SliderInt>("seek-position").RegisterValueChangedCallback(evt => ManualSeek(evt.newValue));

      {
        var e = _gameStateLog.entries.Last();
        float whackingScore = e.possibleMaximumWhackingScore <= 0 ? 0f : 50f * e.whackingScore / e.possibleMaximumWhackingScore;
        float alignmentScore = e.possibleMaximumAlignmentScore <= 0f ? 0f : 50f * e.alignmentScore / e.possibleMaximumAlignmentScore;
        _whackResultUiDataSource.whackingScore = whackingScore;
        _whackResultUiDataSource.alignmentScore = alignmentScore;
        _whackResultUiDataSource.totalScore = whackingScore + alignmentScore;
      }
    }

    public void StartPlaying()
    {
      _state = State.Playing;
      _playbackTimer = 1f;
      _playbackSpeed = 1f;
    }

    public void StopPlaying()
    {
      _state = State.Stopped;
      _playbackTimer = 1f;
      _playbackSpeed = 0f;
    }

    public int Seek(int seekPosition, bool repeat = false)
    {
      if (seekPosition >= _replayBuffer.entries.Count) {
        seekPosition = repeat ? 0 : _replayBuffer.entries.Count - 1;
      }
      if (seekPosition < 0) {
        seekPosition = 0;
      }
      _whackResultUiDataSource.seekPosition = seekPosition;
      return seekPosition;
    }

    private void ManualSeek(int seekPosition)
    {
      if (_whackResultUiDataSource.seekPosition == seekPosition)
        return;
      StopPlaying();
      Seek(seekPosition);
    }

    void Update()
    {
      int seekPosition = _whackResultUiDataSource.seekPosition;

      switch (_state) {
      case State.Playing:
        _playbackTimer -= Time.deltaTime * _playbackSpeed;
        if (_playbackTimer <= 0f) {
          _playbackTimer += 1f;
          seekPosition++;
        }
        break;
      case State.Stopped:
        break;
      case State.Closing:
        break;
      }

      seekPosition = Seek(seekPosition, true);

      if (seekPosition > _replayBuffer.entries.Count)
        return;

      var entry = _replayBuffer.entries[seekPosition];
      _webcamPlane.material.mainTexture = entry.texture;
      _uiDataSource.distance = entry.distance;
      _uiDataSource.pitch = entry.pitch;
      _visualizerSkeleton.UpdateAvatarPose(entry.spinalAlignment);
      _visualizerStickFigure.DrawAlignment(entry.spinalAlignment, entry.spinalAlignmentScore, true, false, entry.distance, entry.pitch);
    }
  }
}
