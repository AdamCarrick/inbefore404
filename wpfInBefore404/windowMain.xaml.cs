using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace wpfInBefore404
{

  public partial class windowMain : Window
  {
    public delegate void UpdateDownloadListDelegate();
    public delegate void UpdateWatchListDelegate();

    private DispatchedObservableCollection<DownloaderTaskViewItem> _DownloadTaskViewCollection = new DispatchedObservableCollection<DownloaderTaskViewItem>();
    private DispatchedObservableCollection<WatchThreadViewItem> _WatchThreadCollection = new DispatchedObservableCollection<WatchThreadViewItem>();
    public DebugLog debugLogText = new DebugLog();
    private NotifyIcon notifyIcon;
    public DownloadManager theDownloadManager;
    public WatchThreadManager theWatchManager;
    private windowAddAdvanced winAddAdvanced;
    public WatchThreadSerializer wts;

    public windowMain()
    {
      InitializeComponent();
    }

    private void AddAdvanced_Click(object sender, RoutedEventArgs e)
    {
      try {
        this.winAddAdvanced = new windowAddAdvanced();
        this.winAddAdvanced.RegisterWatchThreadManager(ref this.theWatchManager);
        this.winAddAdvanced.watchThreadSerializer = this.wts;
        this.winAddAdvanced.Show();
      } catch {
        System.Windows.MessageBox.Show("Error Loading Advanced Watch Window.");
      }
    }

    private void btnAddClicked(object sender, RoutedEventArgs e)
    {
      try {
        Uri argUri = new Uri(this.txtURL.Text);
        if (argUri.IsWellFormedOriginalString()) {
          if (!this.theWatchManager.AddURL(argUri)) {
            this.winAddAdvanced = new windowAddAdvanced(argUri);
            this.winAddAdvanced.RegisterWatchThreadManager(ref this.theWatchManager);
            this.winAddAdvanced.Show();
          }
          if (Properties.Settings.Default.AutoSaveOnAdd) {
            this.wts.Serialize(this.theWatchManager, this.theDownloadManager);
          }
        } else {
          BrushConverter converter = new BrushConverter();
          this.txtURL.Background = converter.ConvertFromString("RED") as SolidColorBrush;
        }
      } catch (Exception) {
        BrushConverter converter2 = new BrushConverter();
        this.txtURL.Background = converter2.ConvertFromString("RED") as SolidColorBrush;
      }
    }

    private void btnClearLog_Click(object sender, RoutedEventArgs e)
    {
      this.debugLogText.Clear();
    }

    private void contextBrowse(object sender, RoutedEventArgs e)
    {
      try {
        if (this.lvWatchList.SelectedItems.Count != 0) {
          WatchThreadViewItem selectedItem = (WatchThreadViewItem)this.lvWatchList.SelectedItem;
          string destFolder = selectedItem.watchThread.destFolder;
          string environmentVariable = Environment.GetEnvironmentVariable("WINDIR");
          new Process { StartInfo = { FileName = environmentVariable + @"\explorer.exe", Arguments = destFolder } }.Start();
        }
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Browsing Item Folder.\r\n" + exception.Message);
      }
    }

    private void contextDelete(object sender, RoutedEventArgs e)
    {
      try {
        if (this.lvWatchList.SelectedItems.Count != 0) {
          foreach (WatchThreadViewItem item in this.lvWatchList.SelectedItems) {
            item.watchThread.StopRunning();
            this.theWatchManager.watchThreadList.Remove(item.watchThread);
            this._WatchThreadCollection.Remove((WatchThreadViewItem)this.lvWatchList.SelectedItem);
            this.RefreshWatchThreadsListView();
            if (Properties.Settings.Default.AutoSaveOnAdd) {
              this.wts.Serialize(this.theWatchManager, this.theDownloadManager);
            }
          }
        }
      } catch {
      }
    }

    private void contextProperties(object sender, RoutedEventArgs e)
    {
      try {
        if (this.lvWatchList.SelectedItems.Count != 0) {
          WatchThreadViewItem selectedItem = (WatchThreadViewItem)this.lvWatchList.SelectedItem;
          this.winAddAdvanced = new windowAddAdvanced(selectedItem.watchThread);
          this.winAddAdvanced.RegisterWatchThreadManager(ref this.theWatchManager);
          this.winAddAdvanced.watchThreadSerializer = this.wts;
          this.winAddAdvanced.Show();
        }
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Opening Properties Window.\r\n" + exception.Message);
      }
    }

    private void contextStart(object sender, RoutedEventArgs e)
    {
      try {
        if (this.lvWatchList.SelectedItems.Count != 0) {
          foreach (WatchThreadViewItem item in this.lvWatchList.SelectedItems) {
            item.watchThread.StartRunning();
          }
          this.RefreshWatchThreadsListView();
        }
      } catch {
      }
    }

    private void contextStop(object sender, RoutedEventArgs e)
    {
      try {
        if (this.lvWatchList.SelectedItems.Count != 0) {
          foreach (WatchThreadViewItem item in this.lvWatchList.SelectedItems) {
            item.watchThread.StopRunning();
          }
          this.RefreshWatchThreadsListView();
        }
      } catch {
      }
    }
    public void CreateNotifyIcon()/////
    {
      this.notifyIcon = new NotifyIcon();
      this.notifyIcon.Icon = new System.Drawing.Icon(@"content\Bunny3.ico");
      this.notifyIcon.DoubleClick += delegate(object sender, EventArgs args)
      {
        base.Show();
        base.WindowState = WindowState.Normal;
        this.notifyIcon.Visible = false;
      };
    }

    private void ExploreItem()/////
    {
      try {
        if (this.lvWatchList.SelectedItem != null) {
          WatchThreadViewItem selectedItem = (WatchThreadViewItem)this.lvWatchList.SelectedItem;
          string destFolder = selectedItem.watchThread.destFolder;
          string environmentVariable = Environment.GetEnvironmentVariable("WINDIR");
          new Process { StartInfo = { FileName = environmentVariable + @"\explorer.exe", Arguments = destFolder } }.Start();
        }
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Browsing Item Folder.\r\n" + exception.Message);
      }
    }

    private void HelpAbout_Click(object sender, RoutedEventArgs e)
    {
      try {
        new windowAbout().Show();
      } catch {
        System.Windows.MessageBox.Show("Error Loading About Dialog");
      }
    }


    private void lvWatchList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      this.ExploreItem();
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e)
    {
      System.Windows.Application.Current.Shutdown();
    }

    private void menuForceStartAll_Click(object sender, RoutedEventArgs e)
    {
      try {
        this.theWatchManager.ForceStartAll();
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error menuForceStartAll_Click \r\n" + exception.Message);
      }
    }

    private void menuLoadWatchList_Click(object sender, RoutedEventArgs e)
    {
      try {
        this.theWatchManager = this.wts.Deserialize();
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Loading Watch List.\r\n" + exception.Message);
      }
    }

    private void MenuPreferences_Click(object sender, RoutedEventArgs e)
    {
      try {
        windowPreferences preferences = new windowPreferences();
        preferences.Register(this.theWatchManager);
        preferences.Show();
      } catch {
        System.Windows.MessageBox.Show("Error Loading Preferences Window.");
      }
    }

    private void menuRemoveAll404_Click(object sender, RoutedEventArgs e)
    {
      try {
        this.theWatchManager.RemoveAll404d();
        this.RefreshWatchThreadsListView();
        if (Properties.Settings.Default.AutoSaveOnAdd) {
          this.wts.Serialize(this.theWatchManager, this.theDownloadManager);
        }
      } catch {
      }
    }

    private void menuSaveWatchList_Click(object sender, RoutedEventArgs e)
    {
      try {
        this.wts.Serialize(this.theWatchManager, this.theDownloadManager);
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Saving Watch List.\r\n" + exception.Message);
      }
    }

    private void menuStartAll_Click(object sender, RoutedEventArgs e)
    {
      try {
        this.theWatchManager.StartAll();
        this.RefreshWatchThreadsListView();
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error menuStartAll_Click \r\n" + exception.Message);
      }
    }

    private void menuStopAll_Click(object sender, RoutedEventArgs e)
    {
      try {
        this.theWatchManager.StopAll();
        this.RefreshWatchThreadsListView();
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error menuStopAll_Click \r\n" + exception.Message);
      }
    }

    public void Notify(object anObject)
    {
    }

    protected override void OnStateChanged(EventArgs e)/////
    {
      if ((this.notifyIcon != null) && (base.WindowState == WindowState.Minimized)) {
        base.Hide();
        this.notifyIcon.Visible = true;
      }
      base.OnStateChanged(e);
    }

    private void PrintDebugInfo_Click(object sender, RoutedEventArgs e)
    {
      try {
        this.debugLogText.AddLine(this.theWatchManager.PrintDebugInfo());
      } catch {
        System.Windows.MessageBox.Show("Error PrintDebugInfo_Click");
      }
    }

    public void RefreshDownloadsListView()
    {
      try {
        this.lvDownloadManager.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new UpdateDownloadListDelegate(this.UpdateDownloadsListView));
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error RefreshDownloadListView.\r\n" + exception.Message);
      }
    }

    public void RefreshWatchThreadsListView()
    {
      try {
        this.lvWatchList.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new UpdateWatchListDelegate(this.UpdateWatchThreadsListView));
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error RefreshListView.\r\n" + exception.Message);
      }
    }

    private void StartItem()/////
    {
      try {
        if (this.lvWatchList.SelectedItem != null) {
          WatchThreadViewItem selectedItem = (WatchThreadViewItem)this.lvWatchList.SelectedItem;
          selectedItem.watchThread.StartRunning();
          this.RefreshWatchThreadsListView();
        }
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Starting Item.\r\n" + exception.Message);
      }
    }

    private void StartRunning_Click(object sender, RoutedEventArgs e)/////
    {
      try {
        this.StartItem();
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Starting Item.\r\n" + exception.Message);
      }
    }

    private void StopItem()
    {
      try {
        if (this.lvWatchList.SelectedItem != null) {
          WatchThreadViewItem selectedItem = (WatchThreadViewItem)this.lvWatchList.SelectedItem;
          selectedItem.watchThread.StopRunning(true);
          this.RefreshWatchThreadsListView();
        }
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Stopping Item.\r\n" + exception.Message);
      }
    }

    private void StopRunning_Click(object sender, RoutedEventArgs e)/////
    {
      try {
        this.StopItem();
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Stopping Item.\r\n" + exception.Message);
      }
    }


    private void txtURL_GotFocus(object sender, RoutedEventArgs e)
    {
      this.txtURL.SelectAll();
    }

    private void txtURL_TextChanged(object sender, TextChangedEventArgs e)
    {
      try {
        BrushConverter converter = new BrushConverter();
        this.txtURL.Background = converter.ConvertFromString("White") as SolidColorBrush;
      } catch {
      }
    }

    private void UpdateDownloadsListView()
    {
      try {
        this.lvDownloadManager.Items.Refresh();
      } catch (Exception) {
      }
    }

    private void UpdateWatchThreadsListView()
    {
      try {
        this.lvWatchList.Items.Refresh();
      } catch (Exception exception) {
        throw exception;
      }
    }

    public void windowMainOnClose(object sender, EventArgs e)
    {
      this.theDownloadManager.CancelAllDownloads();
      if (this.notifyIcon != null) {
        this.notifyIcon.Visible = false;
      }
    }
    public DispatchedObservableCollection<DownloaderTaskViewItem> DownloadTaskViewCollection
    {
      get
      {
        return this._DownloadTaskViewCollection;
      }
    }

    public DispatchedObservableCollection<WatchThreadViewItem> WatchThreadViewCollection
    {
      get
      {
        return this._WatchThreadCollection;
      }
    }

    public ObservableCollection<WatchThreadViewItem> WatchThreadViewCollection2
    {
      get
      {
        return this._WatchThreadCollection;
      }
    }

    
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      try {
        System.Windows.Data.Binding binding = new System.Windows.Data.Binding("TheLog")
        {
          Source = this.debugLogText
        };
        this.txtDebug.SetBinding(System.Windows.Controls.TextBox.TextProperty, binding);
        this.theWatchManager = new WatchThreadManager();
        this.theDownloadManager = new DownloadManager();
        this.theDownloadManager.RegisterDebugLog(ref this.debugLogText);
        this.theDownloadManager.RegisterDownloadTaskViewCollection(this.DownloadTaskViewCollection);
        this.theDownloadManager.RefreshDownloadListViewCallback = new RefreshDownloadListDelegate(this.RefreshDownloadsListView);
        this.theWatchManager.RegisterDebugLog(this.debugLogText);
        this.theWatchManager.RegisterDownloadManager(this.theDownloadManager);
        this.theWatchManager.RegisterWatchThreadViewCollection(this.WatchThreadViewCollection);
        this.theWatchManager.RefreshListViewCallback = new RefreshWatchListDelegate(this.RefreshWatchThreadsListView);
        this.wts = new WatchThreadSerializer(this.theWatchManager, this.theDownloadManager, this.WatchThreadViewCollection);
        this.wts.RefreshListViewCallback = this.theWatchManager.RefreshListViewCallback;
        this.wts.debugLogText = this.debugLogText;

        
        lvWatchList.ItemsSource = WatchThreadViewCollection;
        lvDownloadManager.ItemsSource = DownloadTaskViewCollection;


        if (Properties.Settings.Default.AutoLoadWatchList) {
          try {
            this.theWatchManager = this.wts.Deserialize();
          } catch {
          }
        }
        if (Properties.Settings.Default.MinimizeToTray) {
          this.CreateNotifyIcon();
        }
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error Creating Watch Manager\r\n" + exception.Message);
        System.Windows.Application.Current.Shutdown();
      }
      try {
        this.txtURL.Focus();
      } catch {
      }

    }

    private void txtDebug_SourceUpdated(object sender, DataTransferEventArgs e)
    {
      (sender as System.Windows.Controls.TextBox).ScrollToEnd();
    }




  }
}
