using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;


namespace wpfInBefore404
{

  public class DispatchedObservableCollection<Titem> : System.Collections.ObjectModel.ObservableCollection<Titem>
  {
    private DispatchEvent collectionChanged;
    private DispatchEvent propertyChanged;

    public override event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged
    {
      add
      {
        this.collectionChanged.Add(value);
      }
      remove
      {
        this.collectionChanged.Remove(value);
      }
    }

    protected override event PropertyChangedEventHandler PropertyChanged
    {
      add
      {
        this.propertyChanged.Add(value);
      }
      remove
      {
        this.propertyChanged.Remove(value);
      }
    }

    public DispatchedObservableCollection()
    {
      this.collectionChanged = new DispatchEvent();
      this.propertyChanged = new DispatchEvent();
    }

    public DispatchedObservableCollection(List<Titem> list)
      : base(list)
    {
      this.collectionChanged = new DispatchEvent();
      this.propertyChanged = new DispatchEvent();
    }

    public DispatchedObservableCollection(Dispatcher dispatcher)
    {
      this.collectionChanged = new DispatchEvent();
      this.propertyChanged = new DispatchEvent();
    }

    protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      this.collectionChanged.Fire(this, e);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      this.propertyChanged.Fire(this, e);
    }
  }
}

