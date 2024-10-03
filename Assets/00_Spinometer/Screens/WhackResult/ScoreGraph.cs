using System.Linq;
using Drawing;
using GetBack.Spinometer.SpinalAlignmentCore;
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
      float localCenterX = (t1 - t0) * 0.5f;
      m0.SetTRS(new Vector3(localCenterX, 0f, 0f), Quaternion.identity, Vector3.one);
      Vector3 localToGlobalScale =
        new Vector3(width / (t1 - t0),
                    height / 2f,
                    1f);
      m1.SetTRS(center, Quaternion.identity, localToGlobalScale);
      return m1 * m0;
    }

    public void Render()
    {
      using (Draw.ingame.WithMatrix(LocalToWorldMatrix())) {
        using (Draw.ingame.WithLineWidth(1f)) {
          Draw.ingame.Line(new Vector3(gameStateLog.timeRemainingMin, 0f, 0f),
                           new Vector3(gameStateLog.timeRemainingMax, 0f, 0f));
          Draw.ingame.Circle(new Vector3(gameStateLog.timeRemainingMin, 1f, 0f), Vector3.back, 0.1f);

          {
            using (Draw.ingame.WithColor(Color.white)) {
              var e0 = gameStateLog.entries[0];
              for (int i = 1; i < gameStateLog.entries.Count; i++) {
                var e1 = gameStateLog.entries[i];
                var deltaTime = -(e1.timeRemaining - e0.timeRemaining);
                var deltaScore = e1.alignmentScore - e0.alignmentScore;
                var scoreSlope = deltaScore / deltaTime;
                Debug.Log($"{i}: t = {e1.timeRemaining}, score = {e1.alignmentScore}, scoreSlope = {scoreSlope}");
                Draw.ingame.Circle(new Vector3(e1.timeRemaining, scoreSlope, 0f), Vector3.back, 0.01f);
                e0 = e1;
              }
            }
          }
          if (true) {
            SpinalAlignment.RelativeAngleId[] group0 = new[] {
              SpinalAlignment.RelativeAngleId.C2_C7_vert_new,
              SpinalAlignment.RelativeAngleId.C7_T3_vert_new,
            };
            SpinalAlignment.RelativeAngleId[] group1 = new[] {
              SpinalAlignment.RelativeAngleId.C7_T3_T8,
              SpinalAlignment.RelativeAngleId.T3_T8_T12,
            };
            SpinalAlignment.RelativeAngleId[] group2 = new[] {
              SpinalAlignment.RelativeAngleId.T8_T12_L3,
              SpinalAlignment.RelativeAngleId.T12_L3_S,
            };

            RenderGroup(group0, Color.red);
            RenderGroup(group1, Color.green);
            RenderGroup(group2, Color.blue);
          }
        }
      }
    }

    private void RenderGroup(SpinalAlignment.RelativeAngleId[] group0, Color color)
    {
      using (Draw.ingame.WithColor(color)) {
        var e0 = replayBuffer.entries[0];
        var s0 = group0.Select(id => e0.spinalAlignmentScore.scores[id]).Average();
        for (int i = 1; i < replayBuffer.entries.Count; i++) {
          var e1 = replayBuffer.entries[i];
          var s1 = group0.Select(id => e1.spinalAlignmentScore.scores[id]).Average();
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
