using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Xml.Serialization;
using wpfInBefore404.Properties;

namespace wpfInBefore404
{

  public class WatchThread
  {
    [XmlIgnore] public bool checkLocalFiles;
    [XmlIgnore] public DebugLog debugLogText;
    [XmlIgnore] private string decpass;
    [XmlIgnore] private string decuser;
    public string destFile;
    public string destFolder;
    public string destFullPath;
    public string destSubFolder;
    public bool downloadImages;
    [XmlIgnore] private List<DownloadItem> downloadItemList;
    public string downloadLinkTypes;
    [XmlIgnore] private DownloadManager downloadManager;
    [XmlIgnore] private Timer downloadTimer;
    public bool hiddenWatch;
    [XmlIgnore] public bool isRunning;
    public bool overrideDownloadPath;
    public bool overrideSubFolder;
    public bool overrideUpdateInterval;
    public string password;
    [XmlIgnore] public RefreshWatchListDelegate RefreshListViewCallback;
    [XmlIgnore] public int statDownloaded;
    [XmlIgnore] public int statDownloadErrors;
    [XmlIgnore] public int statDownloadTotal;
    [XmlIgnore] public DateTime statLastChange;
    [XmlIgnore] public DateTime statLastUpdate;
    private const int status_404 = 4;
    private const int status_ErrorUpdating = 5;
    private const int status_Running = 0;
    private const int status_Starting = 2;
    private const int status_Stopped = 1;
    private const int status_Updating = 3;
    private const int status_Waiting = 6;
    [XmlIgnore] public bool thread404d;
    public string threadName;
    [XmlIgnore] public Uri threadURI;
    public long updateInterval;
    [XmlIgnore] public bool updatingHtml;
    public string username;
    [XmlIgnore] public WatchThreadManager watchThreadManager;
    [XmlIgnore] public WatchThreadViewItem watchThreadViewItem;

    public WatchThread()
    {
      try {
        this.downloadItemList = new List<DownloadItem>();
        this.threadStatus = 2;
        this.thread404d = false;
        this.updatingHtml = false;
        this.checkLocalFiles = true;
        this.isRunning = false;
        this.statDownloadErrors = 0;
        this.statDownloaded = 0;
        this.statDownloadTotal = 0;
        this.username = "";
        this.password = "";
      } catch (Exception exception) {
        MessageBox.Show("Error in WatchThread constructor, throwing.\r\n" + exception.Message);
        throw exception;
      }
    }

    public WatchThread(DownloadManager argDLManager, WatchThreadManager argWatchThreadManager) : this()
    {
      try {
        this.downloadManager = argDLManager;
        this.watchThreadManager = argWatchThreadManager;
      } catch (Exception exception) {
        MessageBox.Show("Error in WatchThread constructor, throwing.\r\n" + exception.Message);
        throw exception;
      }
    }

    private void AddUrls(IEnumerable<Uri> uris, UrlType type, ref ArrayList arrLinks)
    {
      int pageHighest = 0;
      try {
        IEnumerator<Uri> enumerator = uris.GetEnumerator();
        string source = "";
        while (enumerator.MoveNext()) {
          try {
            source = enumerator.Current.ToString();
            if (!this.hiddenWatch && this.RegexMatch(source, "&s=")) {
              string s = source.Substring(source.ToLower().LastIndexOf("&s=")).Remove(0, 3);
              try {
                int num2 = int.Parse(s);
                if (pageHighest < num2) {
                  pageHighest = num2;
                }
              } catch {
              }
            }
            if (source.ToLower().LastIndexOf("http://") > 0) {
              source = source.Substring(source.LastIndexOf("http://"));
            }
            if (this.RegexMatch(source.Substring(source.ToLower().LastIndexOf(".")), this.downloadLinkTypes.ToLower()) && !arrLinks.Contains(source)) {
              arrLinks.Add(source);
            }
            continue;
          } catch {
            continue;
          }
        }
      } catch (Exception exception) {
        MessageBox.Show("Error in AddUrls.\r\n" + exception.Message);
      }
      if (pageHighest > 0) {
        this.ProcessExtraPages(pageHighest);
      }
    }

    private bool FileCompare(string file1, string file2)
    {
      try {
        int num = 0;
        int num2 = 0;
        FileStream stream = null;
        FileStream stream2 = null;
        if (file1 == file2) {
          return true;
        }
        try {
          stream = new FileStream(file1, FileMode.Open);
          stream2 = new FileStream(file2, FileMode.Open);
        } catch {
          return false;
        }
        if (stream.Length != stream2.Length) {
          stream.Close();
          stream2.Close();
          return false;
        }
        do {
          num = stream.ReadByte();
          num2 = stream2.ReadByte();
        }
        while ((num == num2) & (num != -1));
        stream.Close();
        stream2.Close();
        return ((num - num2) == 0);
      } catch (Exception exception) {
        MessageBox.Show("Error in FileCompare.\r\n" + exception.Message);
        return false;
      }
    }

    public string GetFileContents(string FullPath)
    {
      string str = null;
      StreamReader reader = null;
      try {
        reader = new StreamReader(FullPath);
        str = reader.ReadToEnd();
        reader.Close();
        return str;
      } catch (Exception) {
        return null;
      }
    }

    public void GetLinksFromHTML(string argFullPath, ref ArrayList arrLinks)
    {
      try {
        StreamReader streamReader = new StreamReader(argFullPath);
        string htmlText = streamReader.ReadToEnd();
        streamReader.Close();

        using (HtmlParser parser = new HtmlParser(htmlText)) {
          this.AddUrls(parser.GetHrefs(this.threadURI.ToString()), UrlType.Href, ref arrLinks);
        }
      } catch (Exception exception) {
        MessageBox.Show("Error in GetLinksFromHTML.\r\n" + exception.Message);
      }
      try {
        if (this.downloadImages) {
          StreamReader streamReader2 = new StreamReader(argFullPath);
          string htmlText2 = streamReader2.ReadToEnd();
          using (HtmlParser parser2 = new HtmlParser(htmlText2)) {
            this.AddUrls(parser2.GetImages(this.threadURI.ToString()), UrlType.Img, ref arrLinks);
          }
        }
      } catch (Exception exception2) {
        MessageBox.Show("Error in GetLinksFromHTML.\r\n" + exception2.Message);
      }
    }

    public bool HaveItemsToDownload()
    {
      try {
        bool flag = false;
        foreach (DownloadItem item in this.downloadItemList) {
          if (!item.completed) {
            flag = true;
          }
        }
        return flag;
      } catch {
        return false;
      }
    }

    public void NotifyDownloadCompleted(DownloadItem argItem, DownloadStatus argDownloadStatus)
    {
      try {
        if (argItem.isThread) {
          if (argDownloadStatus == DownloadStatus.CompletedSuccessfully) {
            this.ProcessHtml(argItem.fullPath);
          } else if (argDownloadStatus == DownloadStatus.Failed_404) {
            this.thread404d = true;
            this.threadStatus = 4;
          } else {
            this.updatingHtml = false;
            this.threadStatus = 5;
          }
        } else if (argDownloadStatus == DownloadStatus.CompletedSuccessfully) {
          this.statDownloaded++;
        } else {
          this.statDownloadErrors++;
        }
        if (this.threadStatus != 5) {
          if (this.HaveItemsToDownload()) {
            this.threadStatus = 0;
          } else {
            this.threadStatus = 6;
          }
        }
        this.SetStatus(true);
      } catch (Exception exception) {
        MessageBox.Show("Error in NotifyDownloadComplete.\r\n" + exception.Message);
      }
    }

    public void ParseHTMLIntoDownloads(string argFullPath, string destFullFolder)
    {
      try {
        ArrayList arrLinks = new ArrayList();
        this.GetLinksFromHTML(argFullPath, ref arrLinks);
        arrLinks.Sort();
        try {
          this.downloadItemList.Sort();
        } catch {
        }
        ArrayList list2 = new ArrayList();
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        while (true) {
          if (num >= arrLinks.Count) {
            break;
          }
          if (num2 < this.downloadItemList.Count) {
            num3 = string.CompareOrdinal(this.downloadItemList[num2].itemUri.ToString(), arrLinks[num].ToString());
          } else {
            num3 = 1;
          }
          if (num3 == 0) {
            num2++;
            num++;
          } else if (num3 < 0) {
            num2++;
          } else {
            Uri relativeUri = new Uri(arrLinks[num].ToString());
            if (!relativeUri.IsAbsoluteUri) {
              relativeUri = new Uri(this.threadURI, relativeUri);
            }
            list2.Add(new DownloadItem(relativeUri, Path.Combine(destFullFolder, relativeUri.Segments[relativeUri.Segments.Length - 1]), false, this, this.decuser, this.decpass));
            num++;
          }
        }
        if (list2 != null) {
          foreach (DownloadItem item in list2) {
            this.downloadItemList.Add(item);
            if (this.checkLocalFiles) {
              if (!File.Exists(item.fullPath)) {
                this.downloadManager.AddDownload(item);
              } else {
                FileInfo info = new FileInfo(item.fullPath);
                if (info.Length > 0L) {
                  item.completed = true;
                  this.statDownloaded++;
                } else {
                  this.downloadManager.AddDownload(item);
                }
              }
              continue;
            }
            this.downloadManager.AddDownload(item);
          }
        }
        if (this.checkLocalFiles) {
          foreach (DownloadItem item2 in this.downloadItemList) {
            if (!item2.completed && !item2.isInDownloadQueue) {
              this.downloadManager.AddDownload(item2);
            }
          }
        }
        this.checkLocalFiles = false;
        this.statDownloadTotal = this.downloadItemList.Count;
        this.SetStatus();
      } catch (Exception exception) {
        MessageBox.Show("Error in ParseHTMLIntoDownloads.\r\n" + exception.Message);
      }
    }

    public void PerformUpdate()
    {
      try {
        this.debugLogText.AddLine("Thread Performing Update: " + this.threadURL);
        if (!this.updatingHtml) {
          this.updatingHtml = true;
          this.threadStatus = 3;
          this.statLastUpdate = DateTime.Now;
          this.SetStatus();
          this.TryDeleteTargetHTML();
          DownloadItem argDownloadItem = new DownloadItem(this.threadURI, this.destFullPath, true, this, this.decuser, this.decpass);
          this.downloadManager.AddDownload(argDownloadItem);
        }
      } catch (Exception exception) {
        MessageBox.Show("Error in PerformUpdate.\r\n" + exception.Message);
      }
    }

    public string PrintDebugInfo()
    {
      object obj2 = (((((((((("" + Environment.NewLine) + "~Watch Thread: " + this.threadURL + Environment.NewLine) + " threadStatus: " + this.threadStatus) + " running: " + this.isRunning.ToString()) + " updating: " + this.updatingHtml.ToString()) + " timer: " + this.downloadTimer.Enabled.ToString()) + " interval: " + this.updateInterval) + " errors: " + this.statDownloadErrors) + " 404d: " + this.thread404d.ToString() + Environment.NewLine) + " lastupdate: " + this.statLastUpdate) + " downloaded: " + this.statDownloaded;
      string str = ((((string.Concat(new object[] { obj2, " total: ", this.statDownloadTotal, Environment.NewLine }) + " destFolder: " + this.destFolder) + " destSubFolder: " + this.destSubFolder + Environment.NewLine) + " destFullPath: " + this.destFullPath) + " destFile: " + this.destFile + Environment.NewLine) + " ~~~Download Items~~~" + Environment.NewLine;
      foreach (DownloadItem item in this.downloadItemList) {
        str = str + (item.isThread ? "  thread: " : "  item:   ");
        str = str + item.itemUri.ToString();
        str = str + (item.completed ? " completed" : "");
        str = str + (item.isInDownloadQueue ? " queued" : "");
        str = str + (item.started ? " started" : "");
        object obj3 = str;
        str = string.Concat(new object[] { obj3, " retries: ", item.retries, Environment.NewLine });
      }
      return str;
    }

    public void ProcessExtraPages(int pageHighest)
    {
      int num = 0;
      if (num > pageHighest) {
        num = pageHighest;
      }
      do {
        num += 40;
        Uri argUri = new Uri(this.threadURL + "&s=" + num.ToString());
        string argDestFilename = "s_" + num.ToString() + "_" + this.destFile;
        if (argUri.IsWellFormedOriginalString()) {
          this.watchThreadManager.AddURL(argUri, Path.Combine(this.destFolder, num.ToString()), argDestFilename, this.downloadImages, this.downloadLinkTypes, Convert.ToInt32(this.updateInterval), this.username, this.password, this.threadName, true);
        }
      }
      while (num <= pageHighest);
    }

    public void ProcessHtml(string argFullPath)
    {
      try {
        int num = 0;
        while (File.Exists(Path.Combine(this.destFolder, this.destSubFolder) + "_" + Convert.ToString(num) + ".html")) {
          num++;
        }
        if (num == 0) {
          this.statLastChange = DateTime.Now;
          this.ParseHTMLIntoDownloads(argFullPath, this.destFolder);
          File.Move(this.destFullPath, Path.Combine(this.destFolder, this.destSubFolder) + "_" + Convert.ToString(num) + ".html");
        } else if (this.checkLocalFiles) {
          this.ParseHTMLIntoDownloads(argFullPath, this.destFolder);
          File.Move(this.destFullPath, Path.Combine(this.destFolder, this.destSubFolder) + "_" + Convert.ToString(num) + ".html");
        } else if (!this.FileCompare(this.destFullPath, Path.Combine(this.destFolder, this.destSubFolder) + "_" + Convert.ToString((int)(num - 1)) + ".html")) {
          this.statLastChange = DateTime.Now;
          this.ParseHTMLIntoDownloads(argFullPath, this.destFolder);
          File.Move(this.destFullPath, Path.Combine(this.destFolder, this.destSubFolder) + "_" + Convert.ToString(num) + ".html");
        } else {
          File.Delete(this.destFullPath);
        }
        this.updatingHtml = false;
        this.SetStatus(true);
      } catch (Exception exception) {
        MessageBox.Show("Error ProcessHTML.\r\n" + exception.Message);
      }
    }

    public bool RegexMatch(string source, string regex)
    {
      Regex regex2 = new Regex(regex);
      return regex2.IsMatch(source);
    }

    public void RegisterDebugLog(DebugLog argDebugLog)
    {
      this.debugLogText = argDebugLog;
    }

    public void SetStatus()
    {
      try {
        if (this.thread404d) {
          this.threadStatus = 4;
          this.watchThreadViewItem.Status = "404";
          if (this.downloadTimer != null) {
            this.downloadTimer.Stop();
          }
        }
        switch (this.threadStatus) {
          case 0:
            this.SetStatus("Running");
            break;

          case 1:
            this.SetStatus("Stopped");
            break;

          case 2:
            this.SetStatus("Starting...");
            break;

          case 3:
            this.SetStatus("Updating...");
            break;

          case 4:
            this.SetStatus("404");
            break;

          case 5:
            this.SetStatus("Error Updating...");
            break;

          case 6:
            this.SetStatus("Waiting...");
            break;
        }
        this.UpdateStats();
      } catch {
      }
    }

    public void SetStatus(bool notifyManager)
    {
      this.SetStatus();
      this.RefreshListViewCallback();
    }

    public void SetStatus(string argstr)
    {
      this.watchThreadViewItem.Status = argstr;
      this.UpdateStats();
    }

    public void SetStatus(string argstr, bool notifyManager)
    {
      this.SetStatus(argstr);
      if (notifyManager) {
        this.RefreshListViewCallback();
      }
    }

    public void SetupAfterDeSerialize(DownloadManager argDLManager, WatchThreadManager argWatchThreadManager, RefreshWatchListDelegate argRefreshListViewCallback)
    {
      try {
        this.threadStatus = 2;
        this.thread404d = false;
        this.updatingHtml = false;
        this.checkLocalFiles = true;
        this.isRunning = false;
        this.RefreshListViewCallback = argRefreshListViewCallback;
        this.downloadManager = argDLManager;
        this.watchThreadManager = argWatchThreadManager;
        this.threadURI = new Uri(this.threadURL);
        this.RegisterDebugLog(argWatchThreadManager.debugLogText);
        this.SetupViewItem();
        this.StartRunning();
      } catch (Exception exception) {
        MessageBox.Show("Error in SetupAfterDeSerialize.\r\n" + exception.Message);
      }
    }

    public void SetupViewItem()
    {
      try {
        WatchThreadViewItem item2 = new WatchThreadViewItem
        {
          Downloaded = "0",
          URL = this.threadURL,
          Status = "Running",
          Total = "0"
        };
        WatchThreadViewItem item = item2;
        item.watchThread = this;
        this.watchThreadManager.watchThreadViewCollection.Add(item);
        this.watchThreadViewItem = item;
      } catch (Exception exception) {
        MessageBox.Show("Error in SetupViewItem.\r\n" + exception.Message);
      }
    }

    public void StartRunning()
    {
      try {
        if (this.isRunning || this.updatingHtml) {
          this.StopRunning(false);
          this.updatingHtml = false;
        }
        this.isRunning = true;
        this.thread404d = false;
        this.checkLocalFiles = true;
        this.threadStatus = 2;
        this.decpass = EncDec.Decrypt(this.password, this.threadURL);
        this.decuser = EncDec.Decrypt(this.username, this.threadURL);
        this.TryCreateDestFolder();
        this.StartTimer();
        this.debugLogText.AddLine("Thread Started Running: " + this.threadURL);
        this.PerformUpdate();
      } catch (Exception exception) {
        MessageBox.Show("Error in StartRunning, throwing.\r\n" + exception.Message);
        throw exception;
      }
    }

    public void StartTimer()
    {
      try {
        this.downloadTimer = new Timer();
        this.downloadTimer.Elapsed += new ElapsedEventHandler(this.timerWatchThreadUpdate);
        long num = 0L;
        num += Settings.Default.TimerSeconds * 1000;
        num += (Settings.Default.TimerMinutes * 1000) * 60;
        num += ((Settings.Default.TimerHours * 1000) * 60) * 60;
        this.downloadTimer.Interval = num;
        this.downloadTimer.Start();
      } catch (Exception exception) {
        MessageBox.Show("Error in StartTimer error.\r\n" + exception.Message);
      }
    }

    public void StopRunning()
    {
      try {
        this.threadStatus = 1;
        this.isRunning = false;
        this.updatingHtml = false;
        if (this.downloadTimer != null) {
          this.downloadTimer.Stop();
        }
        this.downloadManager.RemoveMyItems(this);
        this.debugLogText.AddLine("Thread Stopped Running: " + this.threadURL);
        this.SetStatus();
      } catch (Exception exception) {
        MessageBox.Show("Error in StopRunning().\r\n" + exception.Message);
      }
    }

    public void StopRunning(bool argUpdateStatus)
    {
      this.StopRunning();
      if (argUpdateStatus) {
        this.SetStatus();
      }
    }

    public void timerWatchThreadUpdate(object source, ElapsedEventArgs e)
    {
      this.PerformUpdate();
    }

    public void TryCreateDestFolder()
    {
      try {
        Directory.CreateDirectory(this.destFolder);
      } catch {
      }
    }

    public void TryDeleteTargetHTML()
    {
      try {
        File.Delete(this.destFullPath);
      } catch {
      }
    }

    public void UpdateStats()
    {
      try {
        this.statDownloaded = 0;
        foreach (DownloadItem item in this.downloadItemList) {
          if (item.completed) {
            this.statDownloaded++;
          }
        }
        this.watchThreadViewItem.Downloaded = this.statDownloaded.ToString();
        this.watchThreadViewItem.Total = this.statDownloadTotal.ToString();
        this.watchThreadViewItem.LastUpdate = this.statLastUpdate.ToLongTimeString();
        if (this.statLastChange.Year.ToString() == "1") {
          this.watchThreadViewItem.LastChange = "";
        } else {
          this.watchThreadViewItem.LastChange = this.statLastChange.ToLongTimeString();
        }
        this.watchThreadViewItem.Name = this.threadName;
      } catch {
      }
    }

    public int threadStatus { get; set; }

    public string threadURL { get; set; }
  }
}

