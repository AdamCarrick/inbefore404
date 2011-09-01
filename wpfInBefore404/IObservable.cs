using System;

namespace wpfInBefore404
{

  public interface IObservable
  {
    void Register(IObserver anObserver);
    void UnRegister(IObserver anObserver);
  }
}

