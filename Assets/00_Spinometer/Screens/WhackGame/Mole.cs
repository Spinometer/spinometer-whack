using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public class Mole
  {
    public Vector3 position;
    public string text;
    public int score;
    public float size;
    public float aspectRatio;
    public bool alive;
    public double activeUntil;
    public IMolePresenter presenter;
  }
}
