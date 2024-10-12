namespace GetBack.Spinometer.Screens.WhackGame.Buff
{
  public interface IBuff
  {
    public double activeUntil { get; }
    public void NextTick(double currentTime, float deltaTime);
  }
}
