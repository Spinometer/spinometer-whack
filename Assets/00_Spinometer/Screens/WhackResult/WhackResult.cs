using System;
using System.Collections.Generic;
using GetBack.Spinometer.Screens.WhackGame;
using GetBack.Spinometer.SpinalAlignmentVisualizer;
using R3;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace GetBack.Spinometer.Screens.WhackResult
{
  public class WhackResultScreen : MonoBehaviour
  {
    [SerializeField] private ReplayBuffer _replayBuffer;
    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private WhackResultUiDataSource _whackResultUiDataSource;

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
      _whackResultUiDataSource.seekMax = _replayBuffer.entries.Count - 1;
      _whackResultUiDataSource.seekPosition = 0;
      StartPlaying();
    }

    public void StartPlaying()
    {
      _state = State.Playing;
      _playbackTimer = 1f;
      _playbackSpeed = 1f;
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

      if (seekPosition < 0 || seekPosition >= _replayBuffer.entries.Count) {
        seekPosition = 0;
      }

      _whackResultUiDataSource.seekPosition = seekPosition;
      var entry = _replayBuffer.entries[seekPosition];
      var distance = entry.distance;
      var pitch = entry.pitch;
      var spinalAlignment = entry.spinalAlignment;
      _uiDataSource.distance = distance;
      _uiDataSource.pitch = pitch;
      _visualizerSkeleton.UpdateAvatarPose(spinalAlignment);
      _visualizerStickFigure.DrawAlignment(spinalAlignment, true, false, distance, pitch);
    }
  }
}
