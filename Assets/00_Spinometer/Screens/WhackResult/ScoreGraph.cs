using System.Collections.Generic;
using System.Linq;
using Drawing;
using GetBack.Spinometer.SpinometerCore;
using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackResult
{
  public class ScoreGraphRenderer
  {
    public GameStateLog gameStateLog;
    public ReplayBuffer replayBuffer; 
    public float width = 1f;
    public float height = 1f;
    public Vector3 center = Vector3.zero;

    public ScoreGraphRenderer()
    {
    }

    public ScoreGraphRenderer(GameStateLog gameStateLog_, ReplayBuffer replayBuffer_)
    {
      gameStateLog = gameStateLog_;
      replayBuffer = replayBuffer_;
    }

    public ScoreGraphRenderer(GameStateLog gameStateLog_, float width_, float height_, Vector3 center_)
    {
      gameStateLog = gameStateLog_;
      width = width_;
      height = height_;
      center = center_;
    }

    public float TimeRemainingToWorld()
    {
      return 0f;
    }

    public Matrix4x4 LocalToWorldMatrix()
    {
      var m0 = new Matrix4x4();
      var m1 = new Matrix4x4();
      var t0 = gameStateLog.timeRemainingMax;
      var t1 = gameStateLog.timeRemainingMin;
      var score0 = 0f;
      var score1 = 1f;
      float localCenterX = (t1 - t0) * 0.5f;
      float localCenterY = (score1 - score0) * -0.5f;
      m0.SetTRS(new Vector3(localCenterX, localCenterY, 0f), Quaternion.identity, Vector3.one);
      Vector3 localToGlobalScale =
        new Vector3(width / (t1 - t0),
                    height / (score1 - score0),
                    1f);
      m1.SetTRS(center, Quaternion.identity, localToGlobalScale);
      return m1 * m0;
    }

    public Vector3 WorldToLocal(Vector3 pointWorld)
    {
      var m = LocalToWorldMatrix();
      return m.inverse * pointWorld;
    }

    public void Render()
    {
      using (Draw.ingame.WithMatrix(LocalToWorldMatrix())) {
        using (Draw.ingame.WithLineWidth(1f)) {
          Draw.ingame.Line(new Vector3(gameStateLog.timeRemainingMin, 0f, 0f),
                           new Vector3(gameStateLog.timeRemainingMax, 0f, 0f));

          {
            using (Draw.ingame.WithColor(Color.white)) {
              var e0 = gameStateLog.entries[0];
              for (int i = 1; i < gameStateLog.entries.Count; i++) {
                var e1 = gameStateLog.entries[i];
                var deltaTime = -(e1.timeRemaining - e0.timeRemaining);
                var deltaScore = e1.alignmentScore - e0.alignmentScore;
                var scoreSlope = deltaScore / deltaTime;
                //Debug.Log($"{i}: t = {e1.timeRemaining}, score = {e1.alignmentScore}, scoreSlope = {scoreSlope}");
                var pos1 = new Vector3(e1.timeRemaining, scoreSlope, 0f);
                if (scoreSlope < -0.2f) {
                  var pos0 = new Vector3(pos1.x, 0f, pos1.z);
                  Draw.ingame.Line(pos0, pos1, new Color(1f, 0f, 0f, 0.2f));
                }
                Draw.ingame.Circle(pos1, Vector3.back, 0.01f);
                e0 = e1;
              }
            }
          }
          if (false) {
            // plot individual scores, for debug
            var ids = replayBuffer.entries[0].spinalAlignmentScore.absoluteAngleScores.Keys;
            var hue = 0f;
            foreach (var id in ids) {
              Debug.Log($"id = {id}, hue = {hue}");
              SpinalAlignment.AbsoluteAngleId[] group = { id };
              RenderGroup(group, Color.HSVToRGB(hue, 1f, 1f));
              hue += 0.16f;
            }
          }
          if (true) {
            SpinalAlignment.AbsoluteAngleId[] group0 = new[] {
              SpinalAlignment.AbsoluteAngleId.C2_C7,
              SpinalAlignment.AbsoluteAngleId.C7_T3
            };
            SpinalAlignment.AbsoluteAngleId[] group1 = new[] {
              SpinalAlignment.AbsoluteAngleId.T3_T8,
              SpinalAlignment.AbsoluteAngleId.T8_T12,
            };
            SpinalAlignment.AbsoluteAngleId[] group2 = new[] {
              SpinalAlignment.AbsoluteAngleId.L3_S,
            };

            RenderGroup(group0, new Color(1f, 0.3f, 0.3f));
            RenderGroup(group1, new Color(0.3f, 0.8f, 0.3f));
            RenderGroup(group2, new Color(0.3f, 0.3f, 1.0f));
          }
        }
      }
    }

    private void RenderGroup(SpinalAlignment.AbsoluteAngleId[] group0, Color color)
    {

      float Or0(Dictionary<SpinalAlignment.AbsoluteAngleId, float> scores, SpinalAlignment.AbsoluteAngleId id)
      {
        return scores.ContainsKey(id) ? scores[id] : 0f;
      }

      using (Draw.ingame.WithColor(color)) {
        var e0 = replayBuffer.entries[0];
        var scores0 = e0.spinalAlignmentScore.absoluteAngleScores;
        var s0 = group0.Select(id => Or0(scores0, id)).Average();
        for (int i = 1; i < replayBuffer.entries.Count; i++) {
          var e1 = replayBuffer.entries[i];
          var scores1 = e1.spinalAlignmentScore.absoluteAngleScores;
          var s1 = group0.Select(id => Or0(scores1, id)).Average();
          Draw.ingame.Circle(new Vector3(e1.timeRemaining, s1, 0f), Vector3.back, 0.01f);
          Draw.ingame.Line(new Vector3(e0.timeRemaining, s0, 0f), new Vector3(e1.timeRemaining, s1, 0f));
          e0 = e1;
          s0 = s1;
        }
      }
    }

    public void DrawCursor(float t)
    {
      using (Draw.ingame.WithMatrix(LocalToWorldMatrix())) {
        using (Draw.ingame.WithLineWidth(1f)) {
          Draw.ingame.Line(new Vector3(t, -1f, 0f),
                           new Vector3(t, 1f, 0f));
        }
      }
    }
  }
}
