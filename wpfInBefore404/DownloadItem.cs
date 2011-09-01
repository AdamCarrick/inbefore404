using System;
using System.Windows;

namespace wpfInBefore404
{
  public class DownloadItem
  {
    public bool completed;
    public DownloaderTaskViewItem dltViewItem;
    public DownloaderTask downloaderTask;
    public string downloadPath;
    public string fullPath;
    public bool isInDownloadQueue;
    public bool isThread;
    public Uri itemUri;
    public string password;
    public int retries;
    public bool started;
    public string username;
    public WatchThread watchThread;

    public DownloadItem(Uri argUri, string argFullPath, bool argIsThread, WatchThread argWatcher, string argUserName, string argPassword)
    {
      try {
        this.itemUri = argUri;
        this.fullPath = argFullPath;
        this.retries = 0;
        this.isThread = argIsThread;
        this.watchThread = argWatcher;
        this.completed = false;
        this.started = false;
        this.isInDownloadQueue = false;
        this.username = argUserName;
        this.password = argPassword;
        this.dltViewItem = new DownloaderTaskViewItem();
      } catch (Exception exception) {
        MessageBox.Show("Erorr in DownloadItem constructor, throwing.\r\n" + exception.Message);
      }
    }
  }
}

