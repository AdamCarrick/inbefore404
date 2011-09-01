using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
using wpfInBefore404.Properties;

namespace wpfInBefore404
{

  public class DownloadManager
  {
    private int currentDownloads;
    public DebugLog debugLogText;
    public DispatchedObservableCollection<DownloaderTaskViewItem> dltaskViewCollection;
    private List<DownloaderTask> downloadTasks = new List<DownloaderTask>();
    private List<DownloadItem> lstDownloadItemList;
    public static Mutex mutNotify = new Mutex();
    public RefreshDownloadListDelegate RefreshDownloadListViewCallback;

    public DownloadManager()
    {
      try {
        this.lstDownloadItemList = new List<DownloadItem>();
        this.currentDownloads = 0;
        System.Timers.Timer timer = new System.Timers.Timer();
        timer.Elapsed += new ElapsedEventHandler(this.timerProcessDownloadList);
        timer.Interval = 1000.0;
        timer.Start();
      } catch (Exception exception) {
        MessageBox.Show("Error in DownloadManager constructor, throwing.\r\n" + exception.Message);
        throw exception;
      }
    }

    public void AddDownload(DownloadItem argDownloadItem)
    {
      try {
        this.debugLogText.AddLine("Download Added: " + argDownloadItem.itemUri.ToString());
        this.AddItemToDownloadList(ref argDownloadItem);
      } catch (Exception exception) {
        MessageBox.Show("Error AddDownload.\r\n" + exception.Message);
      }
      this.ProcessDownloadList();
    }

    public void AddItemToDownloadList(ref DownloadItem argDownloadItem)
    {
      if (!argDownloadItem.isThread) {
        argDownloadItem.isInDownloadQueue = true;
        this.lstDownloadItemList.Add(argDownloadItem);
      } else {
        int index = 0;
        foreach (DownloadItem item in this.lstDownloadItemList) {
          if (!item.isThread) {
            break;
          }
          index++;
        }
        argDownloadItem.isInDownloadQueue = true;
        this.lstDownloadItemList.Insert(index, argDownloadItem);
      }
    }

    public void CancelAllDownloads()
    {
      try {
        foreach (DownloaderTask task in this.downloadTasks) {
          task.CancelDownload();
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

    public void NotifyDownloadCompleted(DownloadItem argDownloadItem, DownloadStatus dlStatus)
    {
      try {
        this.currentDownloads--;
        foreach (DownloaderTask task in this.downloadTasks) {
          if (task.downloadItem == argDownloadItem) {
            this.downloadTasks.Remove(task);
            break;
          }
        }
        bool flag = true;
        if (dlStatus == DownloadStatus.CompletedSuccessfully) {
          this.debugLogText.AddLine("Download Finish: " + argDownloadItem.itemUri.ToString());
          argDownloadItem.completed = true;
          this.lstDownloadItemList.Remove(argDownloadItem);
        } else {
          if (dlStatus == DownloadStatus.Failed_404) {
            this.debugLogText.AddLine("Download 404'd:  " + argDownloadItem.itemUri.ToString());
            argDownloadItem.completed = true;
            this.lstDownloadItemList.Remove(argDownloadItem);
            if (!Settings.Default.deleteFailedDownloads) {
              goto Label_01C3;
            }
            try {
              File.Delete(argDownloadItem.fullPath);
              goto Label_01C3;
            } catch {
              goto Label_01C3;
            }
          }
          if (dlStatus == DownloadStatus.Cancelled) {
            this.debugLogText.AddLine("Download Cancel: " + argDownloadItem.itemUri.ToString());
            argDownloadItem.completed = false;
            this.lstDownloadItemList.Remove(argDownloadItem);
            if (!Settings.Default.deleteFailedDownloads) {
              goto Label_01C3;
            }
            try {
              File.Delete(argDownloadItem.fullPath);
              goto Label_01C3;
            } catch {
              goto Label_01C3;
            }
          }
          this.debugLogText.AddLine("Download FAILED: " + argDownloadItem.itemUri.ToString());
          if (argDownloadItem.retries < (Settings.Default.maxRetries - 1)) {
            argDownloadItem.started = false;
            argDownloadItem.retries++;
            this.lstDownloadItemList.Remove(argDownloadItem);
            this.AddItemToDownloadList(ref argDownloadItem);
            flag = false;
          }
          if (Settings.Default.deleteFailedDownloads) {
            try {
              File.Delete(argDownloadItem.fullPath);
            } catch {
            }
          }
        }
        Label_01C3:
        if (flag) {
          argDownloadItem.watchThread.NotifyDownloadCompleted(argDownloadItem, dlStatus);
        }
      } catch (Exception exception) {
        MessageBox.Show("Error NotifyDownloadComplete.\r\n" + exception.Message);
      }
    }

    public void NotifyDownloadSatus(DownloadItem argDownloadItem, long fileRemaining)
    {
    }

    public void ProcessDownloadList()
    {
      try {
        bool flag = true;
        Label_00A8:
        while ((this.currentDownloads < Settings.Default.MaxConcurrentDownloads) && flag) {
          flag = false;
          foreach (DownloadItem item in this.lstDownloadItemList) {
            if (!item.started) {
              item.started = true;
              DownloaderTask task = new DownloaderTask(item, this, mutNotify)
              {
                RefreshDownloadListViewCallback = this.RefreshDownloadListViewCallback
              };
              this.downloadTasks.Add(task);
              new Thread(new ThreadStart(task.StartDownload)) { Name = item.itemUri.ToString() }.Start();
              this.currentDownloads++;
              flag = true;
              goto Label_00A8;
            }
          }
        }
      } catch (Exception exception) {
        this.debugLogText.AddLine("Error ProcessDownloadList.\r\n" + exception.Message);
      }
    }

    public void RegisterDebugLog(ref DebugLog argDebugLog)
    {
      this.debugLogText = argDebugLog;
    }

    public void RegisterDownloadTaskViewCollection(DispatchedObservableCollection<DownloaderTaskViewItem> argDltaskViewItemList)
    {
      this.dltaskViewCollection = argDltaskViewItemList;
    }

    public void RemoveMyItems(WatchThread argWT)
    {
      try {
        List<DownloadItem> list = new List<DownloadItem>();
        foreach (DownloadItem item in this.lstDownloadItemList) {
          if (item.watchThread == argWT) {
            list.Add(item);
          }
        }
        foreach (DownloadItem item2 in list) {
          if (item2.started) {
            item2.downloaderTask.CancelDownload();
          }
          item2.isInDownloadQueue = false;
          this.lstDownloadItemList.Remove(item2);
        }
      } catch (Exception exception) {
        MessageBox.Show("*******************Error RemoveMyItems.\r\n" + exception.Message);
      }
    }

    public void timerProcessDownloadList(object source, ElapsedEventArgs e)
    {
      this.ProcessDownloadList();
    }
  }
}

