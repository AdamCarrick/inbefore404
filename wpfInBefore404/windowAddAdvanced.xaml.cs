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

using AC.AvalonControlsLibrary.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Markup;
using wpfInBefore404.Properties;
using System.Windows.Forms;

namespace wpfInBefore404
{
  /// <summary>
  /// Interaction logic for windowAddAdvanced.xaml
  /// </summary>
  public partial class windowAddAdvanced : Window
  {

    public WatchThread watchThread;
    public WatchThreadManager watchThreadManager;
    public WatchThreadSerializer watchThreadSerializer;


    public windowAddAdvanced()
    {
      InitializeComponent();
      this.timePicker1.SelectedHour = Settings.Default.TimerHours;
      this.timePicker1.SelectedMinute = Settings.Default.TimerMinutes;
      this.timePicker1.SelectedSecond = Settings.Default.TimerSeconds;
    }

    public windowAddAdvanced(Uri argURI)
    {
      this.InitializeComponent();
      this.txtURL.Text = argURI.ToString();
      this.txtDestFolder.Text = Settings.Default.DownloadFolder;
      this.txtDestFilename.Text = "index.html";
      this.timePicker1.SelectedHour = Settings.Default.TimerHours;
      this.timePicker1.SelectedMinute = Settings.Default.TimerMinutes;
      this.timePicker1.SelectedSecond = Settings.Default.TimerSeconds;
    }

    public windowAddAdvanced(WatchThread argWatchThread)
    {
      this.InitializeComponent();
      base.Title = "Properties";
      this.watchThread = argWatchThread;
      this.txtURL.Text = this.watchThread.threadURL;
      this.txtDestFolder.Text = this.watchThread.destFolder;
      this.txtDestFilename.Text = this.watchThread.destFile;
      TimeSpan span = TimeSpan.FromMilliseconds((double)this.watchThread.updateInterval);
      this.timePicker1.SelectedHour = span.Hours;
      this.timePicker1.SelectedMinute = span.Minutes;
      this.timePicker1.SelectedSecond = span.Seconds;
      this.chkDownloadImages.IsChecked = new bool?(this.watchThread.downloadImages);
      this.txtLinkTypes.Text = this.watchThread.downloadLinkTypes;
      this.txtUserName.Text = EncDec.Decrypt(this.watchThread.username, this.watchThread.threadURL);
      this.passPassword.Password = EncDec.Decrypt(this.watchThread.password, this.watchThread.threadURL);
      this.txtName.Text = this.watchThread.threadName;
    }

    private void AddThisWatchThread()
    {
      try {
        bool flag;
        if (this.chkDownloadImages.IsChecked == true) {
          flag = true;
        } else {
          flag = false;
        }
        Uri argUri = new Uri(this.txtURL.Text);
        if (argUri.IsWellFormedOriginalString()) {
          this.watchThreadManager.AddURL(argUri, this.txtDestFolder.Text.ToString(), this.txtDestFilename.Text.ToString(), flag, this.txtLinkTypes.Text.ToString(), this.GetTimePickerMilliSeconds(), this.txtUserName.Text.ToString(), this.passPassword.Password.ToString(), this.txtName.Text.ToString(), false);
          if (Settings.Default.AutoSaveOnAdd) {
            this.watchThreadSerializer.Serialize();
          }
          base.Close();
        } else {
          BrushConverter converter = new BrushConverter();
          this.txtURL.Background = converter.ConvertFromString("RED") as SolidColorBrush;
        }
      } catch (Exception exception) {
        throw exception;
      }
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
      try {
        if (this.watchThread == null) {
          this.AddThisWatchThread();
        } else {
          this.UpdateThisWatchThread();
        }
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error in AddAdvanced Add.\r\n" + exception.Message);
      }
    }

    private void btnBrowse_Click(object sender, RoutedEventArgs e)
    {
      try {
        FolderBrowserDialog dialog = new FolderBrowserDialog
        {
          ShowNewFolderButton = true,
          SelectedPath = this.txtDestFolder.Text
        };
        dialog.ShowDialog();
        this.txtDestFolder.Text = dialog.SelectedPath;
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error in AddAdvanced btnBrowse.\r\n" + exception.Message);
      }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      base.Close();
    }

    private void UpdateThisWatchThread()
    {
      try {
        this.watchThread.threadURL = this.txtURL.Text;
        this.watchThread.destFolder = this.txtDestFolder.Text;
        this.watchThread.destFile = this.txtDestFilename.Text;
        this.watchThread.updateInterval = this.GetTimePickerMilliSeconds();
        this.watchThread.downloadImages = this.chkDownloadImages.IsChecked.Value;
        this.watchThread.downloadLinkTypes = this.txtLinkTypes.Text;
        this.watchThread.username = EncDec.Encrypt(this.txtUserName.Text.ToString(), this.txtURL.Text);
        this.watchThread.password = EncDec.Encrypt(this.passPassword.Password.ToString(), this.txtURL.Text);
        this.watchThread.threadName = this.txtName.Text;
        Uri uri = new Uri(this.txtURL.Text);
        if (uri.IsWellFormedOriginalString()) {
          this.watchThread.threadURI = uri;
          this.watchThread.UpdateStats();
          this.watchThread.StartRunning();
          if (Settings.Default.AutoSaveOnAdd) {
            this.watchThreadSerializer.Serialize();
          }
          base.Close();
        } else {
          BrushConverter converter = new BrushConverter();
          this.txtURL.Background = converter.ConvertFromString("RED") as SolidColorBrush;
        }
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error updating Watch Thread. Resulting watch item may be broken." + exception.Message);
      }
    }

    private int GetTimePickerMilliSeconds()
    {
      int num = 0;
      if (((Convert.ToInt32(this.timePicker1.SelectedHour) == 0) && (Convert.ToInt32(this.timePicker1.SelectedMinute) == 0)) && (Convert.ToInt32(this.timePicker1.SelectedSecond) < 30)) {
        num = 30;
      } else {
        num += (Convert.ToInt32(this.timePicker1.SelectedHour) * 60) * 60;
        num += Convert.ToInt32(this.timePicker1.SelectedMinute) * 60;
        num += Convert.ToInt32(this.timePicker1.SelectedSecond);
      }
      return (num * 0x3e8);
    }
    public void RegisterWatchThreadManager(ref WatchThreadManager argWatchThreadManager)
    {
      this.watchThreadManager = argWatchThreadManager;
    }

    public void RegisterWatchThreadSerializer(ref WatchThreadSerializer argwts)
    {
      this.watchThreadSerializer = argwts;
    }

    public void FillTextBoxes()
    {
      try {
        Uri uri = new Uri(this.txtURL.Text);
        if (uri.IsWellFormedOriginalString()) {
          string query = uri.Segments[uri.Segments.Length - 1];
          if (query == "board.php") {
            query = uri.Query;
            query = query.Substring(query.LastIndexOf("&tid=")).Remove(0, 5);
          }
          if (this.txtDestFilename.Text.ToString() == "") {
            this.txtDestFilename.Text = query;
          }
          if (this.txtDestFolder.Text.ToString() == "") {
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
            this.txtDestFolder.Text = System.IO.Path.Combine(Settings.Default.DownloadFolder, str2);
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


    private void txtURL_TextChanged(object sender, TextChangedEventArgs e)
    {
      BrushConverter converter = new BrushConverter();
      this.txtURL.Background = converter.ConvertFromString("White") as SolidColorBrush;
    }

    public void txtURLLostFocus(object sender, RoutedEventArgs e)
    {
      this.FillTextBoxes();
    }




  }
}

