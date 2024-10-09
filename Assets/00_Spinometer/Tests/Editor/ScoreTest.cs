using GetBack.Spinometer.SpinalAlignmentAux;
using NUnit.Framework;
using UnityEngine.TestTools.Utils;

public class ScoreTest
{
  [Test]
  public void TestFallOff()
  {
    {
      var actual = SpinalAlignmentScoreCalculator.FallOff(10f, 10f, 2f);
      var expected = 1f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.FallOff(9f, 10f, 2f);
      var expected = 0.75f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.FallOff(11f, 10f, 2f);
      var expected = 0.75f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.FallOff(8f, 10f, 2f);
      var expected = 0f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.FallOff(12f, 10f, 2f);
      var expected = 0f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
  }

  [Test]
  public void TestNeckScore()
  {
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateNeckAnglesScore(10f, 10f, 2f);
      var expected = 1f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateNeckAnglesScore(9f, 10f, 2f);
      var expected = 1f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateNeckAnglesScore(11f, 10f, 2f);
      var expected = 0.75f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateNeckAnglesScore(8f, 10f, 2f);
      var expected = 1f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateNeckAnglesScore(12f, 10f, 2f);
      var expected = 0f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateNeckAnglesScore(7f, 10f, 2f);
      var expected = 1f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateNeckAnglesScore(13f, 10f, 2f);
      var expected = 0f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
  }

  [Test]
  public void TestWaistScore()
  {
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateWaistAnglesScore(10f, 10f, 2f);
      var expected = 1f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateWaistAnglesScore(9f, 10f, 2f);
      var expected = 0.75f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateWaistAnglesScore(11f, 10f, 2f);
      var expected = 0.75f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateWaistAnglesScore(8f, 10f, 2f);
      var expected = 0f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateWaistAnglesScore(12f, 10f, 2f);
      var expected = 0f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateWaistAnglesScore(7f, 10f, 2f);
      var expected = 0f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
    {
      var actual = SpinalAlignmentScoreCalculator.CalculateWaistAnglesScore(13f, 10f, 2f);
      var expected = 0f;
      Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
    }
  }
}
