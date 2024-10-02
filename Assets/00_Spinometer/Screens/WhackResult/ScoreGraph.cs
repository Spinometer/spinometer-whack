using Drawing;
using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackResult
{
  public class ScoreGraphRenderer
  {
    public GameStateLog gameStateLog;
    public float width = 1f;
    public float height = 1f;
    public Vector3 center = Vector3.zero;

    public ScoreGraphRenderer()
    {
    }

    public ScoreGraphRenderer(GameStateLog gameStateLog_)
    {
      gameStateLog = gameStateLog_;
    }

    public ScoreGraphRenderer(GameStateLog gameStateLog_, float width_, float height_, Vector3 center_)
    {
      gameStateLog = gameStateLog_;
      width = width_;
      height = height_;
      center = center_;
    }
    
    public void Render()
    {
      float halfWidth = width * 0.5f;
      float halfHeight = height * 0.5f;
      using (Draw.ingame.WithLineWidth(1f)) {
        Draw.ingame.Line(center + new Vector3(-halfWidth, -halfHeight, 0),
                         center + new Vector3(halfWidth, -halfHeight, 0));
        Draw.ingame.Line(center + new Vector3(-halfWidth, halfHeight, 0),
                         center + new Vector3(halfWidth, halfHeight, 0));
      }
    }
  }
}
