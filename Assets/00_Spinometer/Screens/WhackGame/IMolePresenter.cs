using System;
using UnityEngine;

namespace GetBack.Spinometer.Screens.WhackGame
{
  public interface IMolePresenter : IDisposable
  {
    void NextTick(double currentTime, float deltaTime);
    void Whacked();
    GameObject moleGO();
  }
}
