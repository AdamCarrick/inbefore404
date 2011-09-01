using System;

namespace wpfInBefore404
{

  public class DebugLog : ObservableObject
  {
    private string debuglogText = ("Log Started " + DateTime.Now.ToString());

    public void AddLine(string argLine)
    {
      this.debuglogText = this.debuglogText + Environment.NewLine + argLine;
      base.OnPropertyChanged("TheLog");
    }

    public void Clear()
    {
      this.debuglogText = "";
      base.OnPropertyChanged("TheLog");
    }

    public string TheLog
    {
      get
      {
        return this.debuglogText;
      }
      set
      {
        if (value != this.debuglogText) {
          this.debuglogText = value;
          base.OnPropertyChanged("TheLog");
        }
      }
    }
  }
}

