using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace wpfInBefore404
{

  public class WatchThreadSerializer
  {
    public DebugLog debugLogText;
    private DownloadManager downloadManager;
    public RefreshWatchListDelegate RefreshListViewCallback;
    private string serializeFilePath;
    private WatchThreadManager watchThreadManager;
    private DispatchedObservableCollection<WatchThreadViewItem> watchThreadViewCollection;

    public WatchThreadSerializer()
    {
      this.debugLogText = new DebugLog();
    }

    public WatchThreadSerializer(WatchThreadManager argWatchThreadManager, DownloadManager argDownloadManager, DispatchedObservableCollection<WatchThreadViewItem> argwatchThreadViewCollection)
    {
      this.debugLogText = new DebugLog();
      this.watchThreadManager = argWatchThreadManager;
      this.downloadManager = argDownloadManager;
      this.watchThreadViewCollection = argwatchThreadViewCollection;
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString(), "InBefore404");
      try {
        Directory.CreateDirectory(path);
      } catch {
      }
      this.serializeFilePath = Path.Combine(path, "WatchThreadList.xml");
    }

    public WatchThreadManager Deserialize()
    {
      if (!File.Exists(this.serializeFilePath)) {
        throw new Exception("File Not Found on deserialize");
      }
      this.watchThreadManager = new WatchThreadManager();
      try {
        XmlSerializer serializer = new XmlSerializer(typeof(WatchThreadManager));
        TextReader textReader = new StreamReader(this.serializeFilePath);
        this.watchThreadManager = (WatchThreadManager)serializer.Deserialize(textReader);
        textReader.Close();
        this.watchThreadManager.DeleteAllHiddenThreads();
        this.watchThreadManager.RefreshListViewCallback = this.RefreshListViewCallback;
        this.watchThreadManager.RegisterWatchThreadViewCollection(this.watchThreadViewCollection);
        this.watchThreadManager.RegisterDownloadManager(this.downloadManager);
        this.watchThreadManager.RegisterDebugLog(this.debugLogText);
        this.watchThreadManager.SetupAfterDeSerialize();
      } catch (Exception exception) {
        throw new Exception("Error in WatchThreadSerializer Deserialize.\r\n" + exception.Message);
      }
      return this.watchThreadManager;
    }

    public void Serialize()
    {
      this.Serialize(this.watchThreadManager, this.downloadManager);
    }

    public void Serialize(WatchThreadManager argWatchThreadManager, DownloadManager argDownloadManager)
    {
      try {
        XmlSerializer serializer = new XmlSerializer(typeof(WatchThreadManager));
        TextWriter textWriter = new StreamWriter(this.serializeFilePath);
        serializer.Serialize(textWriter, argWatchThreadManager);
        textWriter.Close();
      } catch (Exception exception) {
        MessageBox.Show("Error in WatchThreadSerializer Serialize.\r\n" + exception.Message);
      }
    }
  }
}

