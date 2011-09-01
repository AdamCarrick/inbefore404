using ICSharpCode.SharpZipLib.GZip;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Timers;
using System.Windows;
using wpfInBefore404.Properties;

namespace wpfInBefore404
{

  public class DownloaderTask
  {
    public long bytesDownloadedLast;
    public long bytesDownloadedTotal;
    private const int DETAIL_BYTES = 1;
    private const int DETAIL_CANCEL = 2;
    private const int DETAIL_URI = 0;
    public DownloaderTaskViewItem downloaderTaskViewItem;
    public System.Timers.Timer downloaderTicker;
    public DownloadItem downloadItem;
    public DownloadManager downloadManager;
    public bool errTimedOut;
    private string fileName;
    private long fileSize;
    public DateTime lastUpdateTime;
    private static Mutex mutexNotify;
    public RefreshDownloadListDelegate RefreshDownloadListViewCallback;
    public int timeoutNoResponse;
    private Uri uriData;
    private WebClient webClient;

    public DownloaderTask(DownloadItem argDownloadItem, DownloadManager argDownloadManager, Mutex argMutex)
    {
      mutexNotify = argMutex;
      this.downloadItem = argDownloadItem;
      this.downloadManager = argDownloadManager;
    }

    public void CancelDownload()
    {
      try {
        this.webClient.CancelAsync();
      } catch {
      }
    }

    public void downloaderTickerUpdate(object source, ElapsedEventArgs e)
    {
      try {
        TimeSpan span = (TimeSpan)(DateTime.Now - this.lastUpdateTime);
        if (span.Seconds > this.timeoutNoResponse) {
          this.errTimedOut = true;
          this.CancelDownload();
        }
      } catch {
      }
    }

    private string GetHumanReadableFileSize(long fileSize)
    {
      if ((fileSize / 0x40000000L) > 0L) {
        return string.Format("{0} Gb", Math.Round((double)(fileSize / 0x40000000L), 2));
      }
      if ((fileSize / 0x100000L) > 0L) {
        return string.Format("{0} Mb", Math.Round((double)(fileSize / 0x100000L), 2));
      }
      if ((fileSize / 0x400L) > 0L) {
        return string.Format("{0} Kb", Math.Round((double)(fileSize / 0x400L), 2));
      }
      return string.Format("{0} b", fileSize);
    }

    public void LetsDoThisThing()
    {
      this.webClient.DownloadFileAsync(this.uriData, this.fileName, this.downloadItem);
    }

    public void StartDownload()
    {
      try {
        this.uriData = this.downloadItem.itemUri;
        this.fileName = this.downloadItem.fullPath;
        this.downloadItem.downloaderTask = this;
        this.webClient = new WebClient();
        this.lastUpdateTime = DateTime.Now;
        this.errTimedOut = false;
        this.webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.webClient_DownloadProgressChanged);
        this.webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.webClient_DownloadFileCompleted);
        this.webClient.Headers.Add("user-agent", "Opera/9.99 (Windows NT 5.1; U; pl) Presto/9.9.9");
        this.webClient.Credentials = new NetworkCredential(this.downloadItem.username, this.downloadItem.password);
        this.bytesDownloadedTotal = 0L;
        this.bytesDownloadedLast = 0L;
        this.lastUpdateTime = DateTime.Now;
        this.timeoutNoResponse = Settings.Default.TimeoutNoResponse;
        this.downloaderTicker = new System.Timers.Timer();
        this.downloaderTicker.Elapsed += new ElapsedEventHandler(this.downloaderTickerUpdate);
        this.downloaderTicker.Interval = 10000.0;
        this.downloaderTicker.Start();
        DownloaderTaskViewItem item2 = new DownloaderTaskViewItem
        {
          URL = this.uriData.ToString(),
          PercentDownloaded = "0",
          BytesDownloaded = "0",
          BytesTotal = "",
          Retries = this.downloadItem.retries.ToString()
        };
        DownloaderTaskViewItem item = item2;
        item.downloaderTask = this;
        this.downloadManager.dltaskViewCollection.Add(item);
        this.downloaderTaskViewItem = item;
        this.LetsDoThisThing();
      } catch (Exception exception) {
        throw exception;
      }
    }

    private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      try {
        string str = null;
        try {
          str = this.webClient.ResponseHeaders["Content-Encoding"];
        } catch {
        }
        if (!string.IsNullOrEmpty(str) && str.ToLower().Contains("gzip")) {
          try {
            System.IO.File.Delete(this.fileName + ".gzip");
          } catch {
          }
          try {
            System.IO.File.Move(this.fileName, this.fileName + ".gzip");
          } catch {
          }
          Stream stream = new GZipInputStream(System.IO.File.OpenRead(Path.GetFullPath(this.fileName + ".gzip")));
          FileStream stream2 = System.IO.File.Create(Path.GetFullPath(this.fileName));
          int count = 0x800;
          byte[] buffer = new byte[0x800];
          while (true) {
            count = stream.Read(buffer, 0, count);
            if (count <= 0) {
              break;
            }
            stream2.Write(buffer, 0, count);
          }
          stream2.Close();
          stream.Close();
          try {
            System.IO.File.Delete(this.fileName + ".gzip");
          } catch {
          }
        }
        mutexNotify.WaitOne();
        this.downloadManager.dltaskViewCollection.Remove(this.downloaderTaskViewItem);
        try {
          this.downloaderTicker.Stop();
        } catch {
        }
        try {
          if (this.errTimedOut) {
            this.downloadManager.NotifyDownloadCompleted(this.downloadItem, DownloadStatus.Failed);
          } else if (e.Error == null) {
            this.downloadManager.NotifyDownloadCompleted(this.downloadItem, DownloadStatus.CompletedSuccessfully);
          } else if (e.Error.Message.ToString().Contains("404")) {
            this.downloadManager.NotifyDownloadCompleted(this.downloadItem, DownloadStatus.Failed_404);
          } else if (!e.Cancelled && (e.Error != null)) {
            this.downloadManager.NotifyDownloadCompleted(this.downloadItem, DownloadStatus.Failed);
          } else if (e.Cancelled) {
            this.downloadManager.NotifyDownloadCompleted(this.downloadItem, DownloadStatus.Cancelled);
          } else {
            this.downloadManager.NotifyDownloadCompleted(this.downloadItem, DownloadStatus.Failed);
          }
        } catch (Exception exception) {
          MessageBox.Show("Error in webClient_DownloadFileCompleted.\r\n" + exception.Message);
        }
        mutexNotify.ReleaseMutex();
      } catch {
      }
    }

    private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      try {
        this.bytesDownloadedTotal = e.BytesReceived;
        if (this.bytesDownloadedLast != this.bytesDownloadedTotal) {
          this.lastUpdateTime = DateTime.Now;
          this.bytesDownloadedLast = this.bytesDownloadedTotal;
        }
        this.downloaderTaskViewItem.PercentDownloaded = e.ProgressPercentage.ToString();
        this.downloaderTaskViewItem.Retries = this.downloadItem.retries.ToString();
        this.downloaderTaskViewItem.BytesDownloaded = this.GetHumanReadableFileSize(e.BytesReceived);
        this.downloaderTaskViewItem.BytesTotal = this.GetHumanReadableFileSize(e.TotalBytesToReceive);
        this.RefreshDownloadListViewCallback();
      } catch {
        this.bytesDownloadedTotal = 0L;
      }
    }
  }
}

