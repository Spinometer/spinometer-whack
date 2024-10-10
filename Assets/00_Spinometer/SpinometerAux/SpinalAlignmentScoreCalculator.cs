using GetBack.Spinometer.SpinometerCore;
using UnityEngine;

namespace GetBack.Spinometer.SpinometerAux
{
  public class SpinalAlignmentScoreCalculator
  {
    public static void CalculateScore(SpinalAlignment alignment_in, SpinalAlignmentScore score_out)
    {
      CalculateNormalizedAngles(alignment_in, score_out);
      CalculateRelativeAngleScores(score_out);
      CalculateAbsoluteAngleScores(alignment_in, score_out);
    }

    private static float NormalizedAngle(float angle, float normalCenter, float normalHalfWidth)
    {
      return (angle - normalCenter) / normalHalfWidth;
    }

    private static void CalculateNormalizedAngles(SpinalAlignment alignmentIn, SpinalAlignmentScore scoreOut)
    {
      void calc(float normalCenter, float normalHalfWidth, SpinalAlignment.RelativeAngleId id)
      {
        if (!alignmentIn.relativeAngles.ContainsKey(id))
          return;
        var angle = alignmentIn.relativeAngles[id];
        scoreOut.normalizedRelativeAngles[id] = NormalizedAngle(angle, normalCenter, normalHalfWidth);
      }

      calc(31.3f, 5f,
           SpinalAlignment.RelativeAngleId.C2_C7_vert_new);
      calc(41.7f, 10f,
           SpinalAlignment.RelativeAngleId.C7_T3_vert_new);
      calc(158.4f, 8f,
           SpinalAlignment.RelativeAngleId.C7_T3_T8);
      calc(155.0f, 1f,
           SpinalAlignment.RelativeAngleId.T3_T8_T12);
      calc(178.3f, 2.5f,
           SpinalAlignment.RelativeAngleId.T8_T12_L3);
      calc(172.0f, 1.5f,
           SpinalAlignment.RelativeAngleId.T12_L3_S);
    }

    private static void CalculateRelativeAngleScores(SpinalAlignmentScore scoreOut)
    {
      foreach (var kv in scoreOut.normalizedRelativeAngles) {
        var score = -(Mathf.Abs(kv.Value) - 1.0f);
        scoreOut.relativeAngleScores[kv.Key] = score;
      }
    }

    public static void CalculateAbsoluteAngleScores(SpinalAlignment alignmentIn, SpinalAlignmentScore scoreOut)
    {
      CalculateNeckAnglesScore(alignmentIn, scoreOut, SpinalAlignment.AbsoluteAngleId.C2_C7, 25f, 17f);
      CalculateNeckAnglesScore(alignmentIn, scoreOut, SpinalAlignment.AbsoluteAngleId.C7_T3, 45f, 15f);
      CalculateWaistAnglesScore(alignmentIn, scoreOut, SpinalAlignment.AbsoluteAngleId.T3_T8, 15f, 15f);
      CalculateWaistAnglesScore(alignmentIn, scoreOut, SpinalAlignment.AbsoluteAngleId.T8_T12, 15f, 15f);
      CalculateWaistAnglesScore(alignmentIn, scoreOut, SpinalAlignment.AbsoluteAngleId.L3_S, 0f, 16f);
    }

    /*
     * 1 ******
     *         **
     *           *
     *            *
     * 0 ---------*****
     *        |   |
     *        |  \halfWidth
     *      center
     * NOTE:  center is relative angle [deg] to the vertical line.
     */
    private static void CalculateNeckAnglesScore(SpinalAlignment alignmentIn,
                                                 SpinalAlignmentScore scoreOut,
                                                 SpinalAlignment.AbsoluteAngleId id,
                                                 float center, float halfWidth)
    {
      var angleRelativeToVertical = 90f - alignmentIn.absoluteAngles[id];
      var score = CalculateNeckAnglesScore(angleRelativeToVertical, center, halfWidth);
      scoreOut.absoluteAngleScores[id] = score;
    }

    public static float CalculateNeckAnglesScore(float angleRelativeToVertical,
                                                 float center, float halfWidth)
    {
      var score =
        angleRelativeToVertical <= center ? 1.0f :
        angleRelativeToVertical >= center + halfWidth ? 0.0f :
        FallOff(angleRelativeToVertical, center, halfWidth);
      return score;
    }

    /*
     * 1         ***
     *         **   **
     *        *       *
     *       *         *
     * 0 *****---------*****
     *            |    |
     *            |  \halfWidth
     *          center
     * NOTE:  center is relative angle [deg] to the vertical line.
     */
    private static void CalculateWaistAnglesScore(SpinalAlignment alignmentIn,
                                                  SpinalAlignmentScore scoreOut,
                                                  SpinalAlignment.AbsoluteAngleId id, float center, float halfWidth)
    {
      var angleRelativeToVertical = 90f - alignmentIn.absoluteAngles[id];
      var score = CalculateWaistAnglesScore(angleRelativeToVertical, center, halfWidth);
      scoreOut.absoluteAngleScores[id] = score;
    }

    public static float CalculateWaistAnglesScore(float angleRelativeToVertical, float center, float halfWidth)
    {
      var score =
        angleRelativeToVertical <= center - halfWidth ? 0.0f :
        angleRelativeToVertical >= center + halfWidth ? 0.0f :
        FallOff(angleRelativeToVertical, center, halfWidth);
      return score;
    }

    public static float FallOff(float angle, float center, float halfWidth)
    {
      var a = (angle - center) / halfWidth;
      return 1.0f - a * a;
    }
  }
}
