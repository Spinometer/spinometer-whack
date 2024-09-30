using System;
using System.Collections.Generic;

namespace GetBack.Spinometer.Screens.WhackGame.SpawnerStrategy
{
  public class SpawnerStrategyStack : IDisposable
  {
    private Stack<ISpawnerStrategy> _stack = new Stack<ISpawnerStrategy>(8);

    public SpawnerStrategyStack()
    {
    }

    void IDisposable.Dispose()
    {
      Clear();
    }

    public bool IsEmpty()
    {
      return _stack.Count == 0;
    }

    public void Clear()
    {
      foreach (var ss in _stack) {
        ss.Dispose();
      }
      _stack.Clear();
    }

    public void Push(ISpawnerStrategy ss)
    {
      _stack.Push(ss);
    }

    public ISpawnerStrategy Peek()
    {
      return _stack.Peek();
    }

    public void NextTick(double currentTime, float deltaTime)
    {
      var ss = _stack.Peek();
      if (ss == null)
        return;
      if (!ss.IsDone()) {
        ss.NextTick(currentTime, deltaTime);
      }

      if (ss.IsDone()) {
        _stack.Pop();
        ss.Dispose();
      }
    }
  }
}
