using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using wpfInBefore404.Properties;

namespace wpfInBefore404
{

  public class WatchThreadManager : IObserver
  {
    [XmlIgnore]public DebugLog debugLogText;
    [XmlIgnore] public RefreshWatchListDelegate RefreshListViewCallback;
    [XmlIgnore] public int status;
    public const int status_Running = 1;
    public const int status_Stopped = 0;
    public const int status_Updating = 2;
    [XmlIgnore] public DownloadManager theDownloadManager;
    [XmlIgnore] public WatchManagerObserver theWatcher;
    public List<WatchThread> watchThreadList;
    [XmlIgnore] public DispatchedObservableCollection<WatchThreadViewItem> watchThreadViewCollection;

    public WatchThreadManager()
    {
      try {
        this.watchThreadList = new List<WatchThread>();
        this.Notify(true);
        this.StartRunning();
      } catch (Exception exception) {
        MessageBox.Show("Error in WatchThreadManager Constructor.\r\n" + exception.Message);
      }
    }

    public bool AddURL(Uri argUri)
    {
      try {
        string query = argUri.Segments[argUri.Segments.Length - 1];
        if (query == "board.php") {
          query = argUri.Query;
          query = query.Substring(query.LastIndexOf("&tid=")).Remove(0, 5);
        }
        string str2 = query;
        if (str2.EndsWith(".html")) {
          str2 = str2.Substring(0, str2.Length - 5);
        }
        if (str2.EndsWith(".htm")) {
          str2 = str2.Substring(0, str2.Length - 4);
        }
        if (str2.EndsWith(".php")) {
          str2 = str2.Substring(0, str2.Length - 4);
        }
        if ((str2 == @"\") || (str2 == "")) {
          return false;
        }
        if (((query.Contains(@"\") || query.Contains("/")) || (query.Contains(":") || query.Contains("*"))) || ((query.Contains("?") || query.Contains("\"")) || ((query.Contains("<") || query.Contains(">")) || query.Contains("|")))) {
          return false;
        }
        string argDestFolder = Path.Combine(Settings.Default.DownloadFolder, str2);
        string argDestFilename = query;
        string argLinkTypes = "jpg|jpeg|gif|png";
        int argTimerSeconds = Settings.Default.TimerSeconds * 0x3e8;
        argTimerSeconds += (Settings.Default.TimerMinutes * 0x3e8) * 60;
        argTimerSeconds += ((Settings.Default.TimerHours * 0x3e8) * 60) * 60;
        return this.AddURL(argUri, argDestFolder, argDestFilename, false, argLinkTypes, argTimerSeconds, "", "", "", false);
      } catch (Exception exception) {
        MessageBox.Show("Error in AddURL.\r\n" + exception.Message);
        return false;
      }
    }

    public bool AddURL(Uri argUri, string argDestFolder, string argDestFilename, bool argDownloadImages, string argLinkTypes, int argTimerSeconds, string argUserName, string argPassword, string argName, bool hiddenWatch)
    {
      try {
        foreach (WatchThread thread in this.watchThreadList) {
          if (thread.threadURI == argUri) {
            return false;
          }
        }
        if (((argDestFilename.Contains(@"\") || argDestFilename.Contains("/")) || (argDestFilename.Contains(":") || argDestFilename.Contains("*"))) || ((argDestFilename.Contains("?") || argDestFilename.Contains("\"")) || ((argDestFilename.Contains("<") || argDestFilename.Contains(">")) || argDestFilename.Contains("|")))) {
          return false;
        }
        WatchThread item = new WatchThread(this.theDownloadManager, this)
        {
          threadURI = argUri,
          threadURL = argUri.ToString(),
          destFile = argDestFilename,
          destFolder = argDestFolder
        };
        int startIndex = argDestFolder.LastIndexOf(@"\") + 1;
        if (startIndex < 0) {
          startIndex = 0;
        }
        item.destSubFolder = argDestFolder.Substring(startIndex);
        item.destFullPath = Path.Combine(item.destFolder, argDestFilename);
        item.updateInterval = argTimerSeconds;
        item.overrideUpdateInterval = true;
        item.overrideDownloadPath = true;
        item.overrideSubFolder = true;
        item.downloadLinkTypes = argLinkTypes;
        item.downloadImages = argDownloadImages;
        item.hiddenWatch = hiddenWatch;
        item.username = EncDec.Encrypt(argUserName, item.threadURL);
        item.password = EncDec.Encrypt(argPassword, item.threadURL);
        item.threadName = argName;
        item.RegisterDebugLog(this.debugLogText);
        item.watchThreadManager = this;
        item.RefreshListViewCallback = this.RefreshListViewCallback;
        item.SetupViewItem();
        this.watchThreadList.Add(item);
        this.debugLogText.AddLine("URL Added: " + item.threadURL);
        item.StartRunning();
      } catch (Exception exception) {
        this.debugLogText.AddLine("Error in AddURL.\r\n" + exception.Message);
        return false;
      }
      return true;
    }

    public void DeleteAllHiddenThreads()
    {
      try {
        List<WatchThread> list = new List<WatchThread>();
        foreach (WatchThread thread in this.watchThreadList) {
          if (thread.hiddenWatch) {
            list.Add(thread);
          }
        }
        foreach (WatchThread thread2 in list) {
          this.watchThreadList.Remove(thread2);
        }
      } catch (Exception exception) {
        MessageBox.Show("Error in WTManager DeleteAllHiddenThreads.\r\n" + exception.Message);
      }
    }

    public void ForceStartAll()
    {
      foreach (WatchThread thread in this.watchThreadList) {
        thread.StartRunning();
      }
    }

    public void Notify(object anObject)
    {
    }

    public void NotifyTheWatchManagerObserver()
    {
      this.theWatcher.Notify(true);
    }

    public string PrintDebugInfo()
    {
      string str = "";
      str = (str + Environment.NewLine + Environment.NewLine) + "--- Watch Thread Manager ---" + Environment.NewLine;
      foreach (WatchThread thread in this.watchThreadList) {
        str = str + thread.PrintDebugInfo();
      }
      return str;
    }

    public void RegisterDebugLog(DebugLog argDebugLog)
    {
      this.debugLogText = argDebugLog;
    }

    public void RegisterDownloadManager(DownloadManager argDownloadManager)
    {
      this.theDownloadManager = argDownloadManager;
    }

    public void RegisterWatchManagerObserver(WatchManagerObserver argWatcher)
    {
      this.theWatcher = argWatcher;
    }

    public void RegisterWatchThreadViewCollection(DispatchedObservableCollection<WatchThreadViewItem> argwatchThreadViewCollection)
    {
      this.watchThreadViewCollection = argwatchThreadViewCollection;
    }

    public void RemoveAll404d()
    {
      new List<WatchThread>();
      List<WatchThreadViewItem> list = new List<WatchThreadViewItem>();
      foreach (WatchThreadViewItem item in this.watchThreadViewCollection) {
        if (item.watchThread.thread404d) {
          list.Add(item);
        }
      }
      foreach (WatchThreadViewItem item2 in list) {
        this.watchThreadViewCollection.Remove(item2);
        this.watchThreadList.Remove(item2.watchThread);
      }
    }

    public void SetupAfterDeSerialize()
    {
      try {
        foreach (WatchThread thread in this.watchThreadList) {
          thread.SetupAfterDeSerialize(this.theDownloadManager, this, this.RefreshListViewCallback);
        }
      } catch (Exception exception) {
        MessageBox.Show("Error in WTManager SetupAfterDeSerialize.\r\n" + exception.Message);
      }
    }

    public void StartAll()
    {
      foreach (WatchThread thread in this.watchThreadList) {
        if (!thread.isRunning) {
          thread.StartRunning();
        }
      }
    }

    public void StartRunning()
    {
      this.status = 1;
    }

    public void StopAll()
    {
      foreach (WatchThread thread in this.watchThreadList) {
        thread.StopRunning();
      }
    }

    public void StopRunning()
    {
      this.status = 0;
    }

    public void UnRegisterWatchManagerObserver(WatchManagerObserver argWatcher)
    {
      this.theWatcher = null;
    }
  }
}

