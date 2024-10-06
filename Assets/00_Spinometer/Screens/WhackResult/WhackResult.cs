using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GetBack.Spinometer.SpinalAlignmentVisualizer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace GetBack.Spinometer.Screens.WhackResult
{
  public class WhackResultScreen : MonoBehaviour
  {
    [SerializeField] private GameStateLog _gameStateLog;
    [SerializeField] private ReplayBuffer _replayBuffer;
    [SerializeField] private ScoreGraphRenderer _scoreGraphRenderer;
    [SerializeField] private Transform _scoreGraphTopLeft;
    [SerializeField] private Transform _scoreGraphBottomRight;
    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private WhackResultUiDataSource _whackResultUiDataSource;
    [SerializeField] private Renderer _webcamPlane;
    [SerializeField] private AudioClip _bang0Clip;
    [SerializeField] private AudioClip _bang1Clip;
 
    private App _app; // injected by App
    private AudioSource _audioSource;

    public enum State
    {
      Playing,
      Stopped,
      Closing,
    }

    private SpinalAlignmentVisualizerSkeleton _visualizerSkeleton = null;
    private SpinalAlignmentVisualizerStickFigure _visualizerStickFigure = null;
    State _state;
    private float _playbackSpeed;
    private float _seekPosition = 0f;
    public App app { set => _app = value; }

    private void Awake()
    {
      _visualizerSkeleton = GetComponent<SpinalAlignmentVisualizerSkeleton>();
      _visualizerStickFigure = GetComponent<SpinalAlignmentVisualizerStickFigure>();
      _scoreGraphRenderer = new(_gameStateLog, _replayBuffer);
      {
        var p0 = _scoreGraphTopLeft.position;
        var p1 = _scoreGraphBottomRight.position;
        _scoreGraphRenderer.center = (p0 + p1) * 0.5f;
        _scoreGraphRenderer.width = p1.x - p0.x;
        _scoreGraphRenderer.height = p0.y - p1.y;
      }
      _audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
      _visualizerStickFigure.ShowAlignmentValues = true;
      _seekPosition = _gameStateLog.timeRemainingMax;
      StartPlaying();

      var root = GameObject.Find("/WhackResultUIDocument").GetComponent<UIDocument>().rootVisualElement;
      StartAnimation(root);

      {
        var e = _gameStateLog.entries.Last();
        float whackingScore = e.possibleMaximumWhackingScore <= 0 ? 0f : 50f * e.whackingScore / e.possibleMaximumWhackingScore;
        float alignmentScore = e.possibleMaximumAlignmentScore <= 0f ? 0f : 50f * e.alignmentScore / e.possibleMaximumAlignmentScore;
        _whackResultUiDataSource.whackingScore = whackingScore;
        _whackResultUiDataSource.alignmentScore = alignmentScore;
        _whackResultUiDataSource.totalScore = whackingScore + alignmentScore;
      }
    }

    private async void StartAnimation(VisualElement root)
    {
      var el_scores = root.Q<VisualElement>("scores");
      var el_whackingScore = root.Q<VisualElement>("whacking-score");
      var el_alignmentScore = root.Q<VisualElement>("alignment-score");
      var el_totalScore = root.Q<VisualElement>("total-score");
      el_whackingScore.visible = false;
      el_alignmentScore.visible = false;
      el_totalScore.visible = false;

      try {
        await UniTask.Delay(800, cancellationToken: this.GetCancellationTokenOnDestroy());
        el_whackingScore.visible = true;
        StartShakeAnimation(el_scores, 10);
        _audioSource.PlayOneShot(_bang0Clip);
        await UniTask.Delay(800, cancellationToken: this.GetCancellationTokenOnDestroy());
        el_alignmentScore.visible = true;
        StartShakeAnimation(el_scores, 10);
        _audioSource.PlayOneShot(_bang0Clip);
        await UniTask.Delay(1500, cancellationToken: this.GetCancellationTokenOnDestroy());
        el_totalScore.visible = true;
        StartShakeAnimation(el_scores, 30);
        _audioSource.PlayOneShot(_bang1Clip);
        await UniTask.Delay(500, cancellationToken: this.GetCancellationTokenOnDestroy());
      }
      catch (OperationCanceledException) {
        // do nothing
      }
    }

    private async void StartShakeAnimation(VisualElement el, int steps)
    {
      try {
        var tl = el.style.translate.value;
        for (int i = 0; i < steps; i++) {
          el.style.translate = new Translate(tl.x.value + Random.Range(-5, 5), tl.y.value + Random.Range(-5, 5));
          await UniTask.Delay(10, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        el.style.translate = tl;
      }
      catch (OperationCanceledException) {
        // do nothing
      }
    }

    public void StartPlaying()
    {
      _state = State.Playing;
      _playbackSpeed = 1.0f;
    }

    public void StopPlaying()
    {
      _state = State.Stopped;
      _playbackSpeed = 0f;
    }

    public void TogglePlaying()
    {
      switch (_state) {
      case State.Playing:
        StopPlaying();
        break;
      case State.Stopped:
        StartPlaying();
        break;
      case State.Closing:
        break;
      }
    }

    public float Seek(float t, bool repeat = false)
    {
      if (t > _gameStateLog.timeRemainingMax) {
        t = repeat ? _gameStateLog.timeRemainingMin : _gameStateLog.timeRemainingMax;
      }
      if (t < _gameStateLog.timeRemainingMin) {
        t = repeat ? _gameStateLog.timeRemainingMax : _gameStateLog.timeRemainingMin;
      }
      if (t < 0) {
        t = 0;
      }
      _seekPosition = t;
      return _seekPosition;
    }

    private void ManualSeek(float seekPosition)
    {
      StopPlaying();
      Seek(seekPosition, false);
    }

    void Update()
    {
      _scoreGraphRenderer.Render();
      _scoreGraphRenderer.DrawCursor(_seekPosition);

      if (Keyboard.current.rKey.wasPressedThisFrame) {
        _app.MoveFromResultToTitle();
      }

      if (Keyboard.current.enterKey.wasPressedThisFrame ||
          Keyboard.current.spaceKey.wasPressedThisFrame) {
        TogglePlaying();
      }

      if (Mouse.current.leftButton.isPressed) {
        var pointWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
        var pointLocal = _scoreGraphRenderer.WorldToLocal(pointWorld);
        ManualSeek(pointLocal.x + _gameStateLog.timeRemainingMax * 0.5f);
      }

      switch (_state) {
      case State.Playing:
        _seekPosition -= Time.deltaTime * _playbackSpeed;
        break;
      case State.Stopped:
        break;
      case State.Closing:
        break;
      }

      Seek(_seekPosition, true);

      if (_seekPosition < _replayBuffer.timeRemainingMin || _seekPosition > _replayBuffer.timeRemainingMax)
        return;

      var entry = _replayBuffer.FindEntry(_seekPosition);
      _webcamPlane.material.mainTexture = entry.texture;
      _uiDataSource.distance = entry.distance;
      _uiDataSource.pitch = entry.pitch;
      _visualizerSkeleton.UpdateAvatarPose(entry.spinalAlignment);
      _visualizerStickFigure.DrawAlignment(entry.spinalAlignment, entry.spinalAlignmentScore, true, false, entry.distance, entry.pitch);
    }
  }
}
