using System;

namespace wpfInBefore404
{

  public interface ThreadManagerObservable
  {
    void RegisterWatchManagerObserver(WatchManagerObserver anObserver);
    void UnRegisterWatchManagerObserver(WatchManagerObserver anObserver);
  }
}

