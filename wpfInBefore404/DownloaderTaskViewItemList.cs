using System;
using System.Windows.Threading;

namespace wpfInBefore404
{

  public class DownloaderTaskViewItemList : ObservableObject
  {
    private Dispatcher dispatcherUIThread;
    private DispatchedObservableCollection<DownloaderTaskViewItem> thedltvitems;

    public DownloaderTaskViewItemList(Dispatcher dispatcher)
    {
      this.dispatcherUIThread = dispatcher;
      this.thedltvitems = new DispatchedObservableCollection<DownloaderTaskViewItem>();
    }

    public void AddItem(DownloaderTaskViewItem argdltvi)
    {
      this.thedltvitems.Add(argdltvi);
      base.OnPropertyChanged("TheListItems");
    }

    public DispatchedObservableCollection<DownloaderTaskViewItem> TheListItems
    {
      get { return this.thedltvitems; }
    }
  }
}

