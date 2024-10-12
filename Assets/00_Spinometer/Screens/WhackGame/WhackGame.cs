using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GetBack.Spinometer.Screens.WhackGame.Buff;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class WhackGame : MonoBehaviour
  {
    [SerializeField] private Settings _settings;
    [SerializeField] private GameStateLog _gameStateLog;
    [SerializeField] private ReplayBuffer _replayBuffer;
    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private WhackGameUiDataSource _whackGameUiDataSource;
    [SerializeField] private TrackerNeuralNet _tracker;
    [SerializeField] private WebCam _webcam;
    [SerializeField] private float _initialTime = 10f;
    [FormerlySerializedAs("_managerOptions")]
    [SerializeField] private MoleRowManager.Options _moleRowManagerOptions;
    [SerializeField] private MolePresenter.Options _presenterOptions;
    [SerializeField] private GameObject _moleScorePrefab;
    [SerializeField] private AudioClip _whackCountDownClip;
    [SerializeField] private AudioClip _whackSuccessClip;
    [SerializeField] private AudioClip _whackFailClip;
    [SerializeField] private AudioClip _timeoverClip;

    private AudioSource _audioSource;

    public float initialTime => _initialTime;

    public App app; // injected by App
    private float _timeRemaining;
    public float timeRemaining => _timeRemaining;

    private MoleRowManager _moleRowManager;

    private CompositeDisposable _disposables;

    public enum State
    {
      GettingReady,
      GoingOn,
      Finished,
      Closing,
    }

    private State _state;
    private int _whackingScore;
    private int _possibleMaximumWhackingScore;
    private float _alignmentScore;
    private float _possibleMaximumAlignmentScore;
    public float comboGuageValue;
    public readonly float maxComboGuageValue = 5f;
    private int _comboBonusMultiplier = 0;
    private List<IBuff> _buffs = new();
    private FlawlessCombo _flawlessComboBuff = null;
    public State state => _state;

    void Awake()
    {
      _audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
      _disposables = new CompositeDisposable();
      _presenterOptions.whackGame = this;
      _moleRowManager = new MoleRowManager(_settings, this, _audioSource, _moleRowManagerOptions, _presenterOptions);
      _moleRowManager.AddTo(_disposables);
    }

    private void OnDisable()
    {
      _disposables.Dispose();
      _moleRowManager = null;
    }

    void Start()
    {
      _whackingScore = 0;
      _possibleMaximumWhackingScore = 0;
      _alignmentScore = 0f;
      _possibleMaximumAlignmentScore = _initialTime;
      comboGuageValue = 0f;
      _comboBonusMultiplier = 0;
      _whackGameUiDataSource.whackingScore = _whackingScore;
      _whackGameUiDataSource.alignmentScore = _alignmentScore;
      _state = State.GettingReady;
      _timeRemaining = initialTime + 3f;
      _gameStateLog.Clear();
      _replayBuffer.Clear();
      _audioSource.PlayOneShot(_whackCountDownClip);

      _flawlessComboBuff = new FlawlessCombo(this);
      _buffs.Clear();
      _buffs.Add(_flawlessComboBuff);
    }

    void Update()
    {
      _moleRowManager.NextTick(Time.timeAsDouble, Time.deltaTime);
      foreach (var buff in _buffs) {
        buff.NextTick(Time.timeAsDouble, Time.deltaTime);
      }

      var oldTimeRemaining = timeRemaining;
      _timeRemaining -= Time.deltaTime;
      if (Mathf.Floor(oldTimeRemaining) != Mathf.Floor(timeRemaining)) {
        OnCrossingSecondBoundary?.Invoke();
      }
      switch (_state) {
      case State.GettingReady:
        UpdateComboBonusMultiplier();
        //whackGameUiDataSource.timeRemaining = initialTime;
        _whackGameUiDataSource.timeRemainingStr = $"{(Mathf.Floor(timeRemaining - initialTime) + 1):0}";
        if (timeRemaining <= initialTime) {
          _state = State.GoingOn;
          OnGameStarted?.Invoke();
        }
        break;
      case State.GoingOn:
        UpdateComboBonusMultiplier();
        float recordFramesPerSecond = 5f;
        if (Mathf.Floor(oldTimeRemaining * recordFramesPerSecond) != Mathf.Floor(timeRemaining * recordFramesPerSecond)) {
          AddReplayEntry();
        }
        if (timeRemaining <= 0f) {
          _state = State.Finished;
          RecordGameStateLog();
          _audioSource.PlayOneShot(_timeoverClip);
          OnGameFinished?.Invoke();
          break;
        }
        _whackGameUiDataSource.timeRemaining = timeRemaining;
        UpdateAlignmentScore();
        break;
      case State.Finished:
        _whackGameUiDataSource.timeRemaining = 0f;
        if (timeRemaining <= -3f) {
          app.MoveFromGameToResult();
          _state = State.Closing;
          _gameStateLog.UpdateTimeRemainingMinMax();
          _replayBuffer.UpdateTimeRemainingMinMax();
        }
        break;
      case State.Closing:
        break;
      }
    }

    private void UpdateAlignmentScore()
    {
      var scores = _tracker.spinalAlignmentScore.absoluteAngleScores;
      var average = scores.Count == 0 ? 0f : scores.Average(kv => kv.Value);
      _alignmentScore += average * Time.deltaTime;
      _whackGameUiDataSource.alignmentScore = _alignmentScore;
      RecordGameStateLog();
    }

    public delegate void OnGameStartedHandler();
    public event OnGameStartedHandler OnGameStarted;

    public delegate void OnGameFinishedHandler();
    public event OnGameFinishedHandler OnGameFinished;

    public delegate void OnCrossingSecondBoundaryHandler();
    public event OnCrossingSecondBoundaryHandler OnCrossingSecondBoundary;


    private void RecordGameStateLog()
    {
      var entry = new GameStateLog.GameStateLogEntry {
        timeRemaining = _timeRemaining,
        whackingScore = _whackingScore,
        possibleMaximumWhackingScore = _possibleMaximumWhackingScore,
        alignmentScore = _alignmentScore,
        possibleMaximumAlignmentScore = _possibleMaximumAlignmentScore,
      };
      _gameStateLog.entries.Add(entry);
    }

    private void AddReplayEntry()
    {
      // Debug.Log($"Adding replay entry at time {timeRemaining}");

      var inTex = _webcam.ColorRenderTexture;
      var textureCopy = new Texture2D(inTex.width, inTex.height, DefaultFormat.LDR, TextureCreationFlags.None);
      Graphics.CopyTexture(inTex, textureCopy);

      var entry = new ReplayBuffer.ReplayEntry {
        timeRemaining = timeRemaining,
        texture = textureCopy, 
        distance = _uiDataSource.distance,
        pitch = _uiDataSource.pitch,
        spinalAlignment = _tracker.spinalAlignment.Clone(),
        spinalAlignmentScore = _tracker.spinalAlignmentScore.Clone()
      };
      _replayBuffer.entries.Add(entry);
    }

    private void UpdateComboBonusMultiplier()
    {
      if (comboGuageValue >= 5.0f - Mathf.Epsilon) {
        _comboBonusMultiplier = 11;
      } else if (comboGuageValue >= 4.0f - Mathf.Epsilon) {
        _comboBonusMultiplier = 7;
      } else if (comboGuageValue >= 3.0f - Mathf.Epsilon) {
        _comboBonusMultiplier = 4;
      } else if (comboGuageValue >= 2.0f - Mathf.Epsilon) {
        _comboBonusMultiplier = 2;
      } else if (comboGuageValue >= 1.0f - Mathf.Epsilon) {
        _comboBonusMultiplier = 1;
      } else {
        _comboBonusMultiplier = 0;
      }
      _whackGameUiDataSource.comboBonusMultiplier = _comboBonusMultiplier;
    }

    public void AddWhackingScore(int value, Vector3 position, bool damageAnimationIfNeeded = true)
    {
      int bonusMultiplier = (value <= 0) ? 0 : _comboBonusMultiplier;
      int bonus = bonusMultiplier * value;
      _possibleMaximumWhackingScore += bonus;
      _whackingScore += value + bonus;
      _whackGameUiDataSource.whackingScore = _whackingScore;

      // FIXME:  visual stuff should not be here 
      {
        var go = Instantiate(_moleScorePrefab, position + new Vector3(0f, 0.1f, 0f), Quaternion.identity);
        go.transform.DOLocalMoveY(1f, 0.5f).SetRelative(true);
        var t = go.GetComponentInChildren<TextMeshProUGUI>();
        var score = value + bonus;
        var plus = score < 0 ? "" : "+";
        t.text = $"{plus}{score}";
        t.color =
          (value < 0) ? new Color(1f, 0.3f, 0.3f, 1f) :
          (value < 3) ? new Color(0.5f, 0.6f, 0.8f, 1f) :
          (value < 8) ? new Color(0.5f, 0.8f, 1.0f, 1f) :
          new Color(0.7f, 1.0f, 1.0f, 1f);

        float duration = 1.0f;
        go.transform.DOLocalMoveY(0.15f, duration).SetRelative(true).Play();
        DOTween.To(() => t.alpha,
                   x => { t.alpha = x; },
                   0f,
                   duration).SetLink(go).Play();
        Destroy(go, duration);
      }
      RecordGameStateLog();
      if (value < 0) {
        if (damageAnimationIfNeeded) {
          _audioSource.PlayOneShot(_whackFailClip);
          StartDamageAnimation();
        }
      } else {
        _audioSource.PlayOneShot(_whackSuccessClip);
      }
    }

    private async void StartDamageAnimation()
    {
      // FIXME:  should not be here
      _whackGameUiDataSource.damageOpacity = 0.4f;
      DOTween.To(() => _whackGameUiDataSource.damageOpacity,
                 x => _whackGameUiDataSource.damageOpacity = x,
                 0f, 0.1f).SetLink(gameObject).Play();
    }

    public void AddPossibleMaximumWhackingScore(int moleScore)
    {
      _possibleMaximumWhackingScore += moleScore;
      RecordGameStateLog();
    }

    public void WholeRowEliminated(MoleRow moleRow)
    {
      comboGuageValue = Mathf.Min(maxComboGuageValue, comboGuageValue + 0.5f);

      if (!moleRow.isSpecial)
        return;

      _flawlessComboBuff.Activate(Time.timeAsDouble);

      // FIXME:  visual stuff should not be here 
      {
        var go = Instantiate(_moleScorePrefab, moleRow.LastCenterPosition + new Vector3(0f, 0.1f, 0f), Quaternion.identity);
        go.transform.DOLocalMoveY(1f, 0.5f).SetRelative(true);
        var t = go.GetComponentInChildren<TextMeshProUGUI>();
        t.text = $"Combo +5";
        t.color = new Color(1.0f, 1.0f, 0.7f, 1f);
        float duration = 1.0f;
        go.transform.DOLocalMoveY(0.15f, duration).SetRelative(true).Play();
        DOTween.To(() => t.alpha,
                   x => { t.alpha = x; },
                   0f,
                   duration).SetLink(go).Play();
        Destroy(go, duration);
      }

    }

    public void MoleMissed()
    {
      comboGuageValue = Mathf.Max(0f, comboGuageValue - 3f);
    }
  }
}
