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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Markup;
using wpfInBefore404.Properties;

using System.Windows.Forms;

namespace wpfInBefore404
{
  /// <summary>
  /// Interaction logic for windowPreferences.xaml
  /// </summary>
  public partial class windowPreferences : Window
  {
    protected Hashtable observerContainer = new Hashtable();

    public windowPreferences()
    {
      InitializeComponent();
    }

    private void btnBrowse_Click(object sender, RoutedEventArgs e)
    {
      try {
        FolderBrowserDialog dialog = new FolderBrowserDialog();
        dialog.ShowDialog();
        this.txtDownloadFolder.Text = dialog.SelectedPath;
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error btnBrowse_Click.\r\n" + exception.Message);
      }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      base.Close();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      base.Close();
    }

    private void btnOK_Click(object sender, RoutedEventArgs e)
    {
      try {
        Settings.Default.DownloadFolder = this.txtDownloadFolder.Text;
        Settings.Default.maxRetries = Convert.ToInt32(this.txtMaxRetries.Text);
        Settings.Default.TimeoutNoResponse = Convert.ToInt32(this.txtTimeout.Text);
        Settings.Default.AutoSaveOnAdd = this.chkAutoSaveOnAdd.IsChecked.Value;
        Settings.Default.AutoLoadWatchList = this.chkAutoLoadWatchList.IsChecked.Value;
        Settings.Default.deleteFailedDownloads = this.chkDeleteFailedDownloads.IsChecked.Value;
        Settings.Default.MinimizeToTray = this.chkMinimizeToTray.IsChecked.Value;
        if (Convert.ToInt32(this.txtMaxConcurrentTransfers.Text) > 10) {
          Settings.Default.MaxConcurrentDownloads = 10;
        } else {
          Settings.Default.MaxConcurrentDownloads = Convert.ToInt32(this.txtMaxConcurrentTransfers.Text);
        }
        if (((Convert.ToInt32(this.timePicker1.SelectedHour) == 0) && (Convert.ToInt32(this.timePicker1.SelectedMinute) == 0)) && (Convert.ToInt32(this.timePicker1.SelectedSecond) < 30)) {
          Settings.Default.TimerHours = Convert.ToInt32(this.timePicker1.SelectedHour);
          Settings.Default.TimerMinutes = Convert.ToInt32(this.timePicker1.SelectedMinute);
          Settings.Default.TimerSeconds = 30;
        } else {
          Settings.Default.TimerHours = Convert.ToInt32(this.timePicker1.SelectedHour);
          Settings.Default.TimerMinutes = Convert.ToInt32(this.timePicker1.SelectedMinute);
          Settings.Default.TimerSeconds = Convert.ToInt32(this.timePicker1.SelectedSecond);
        }
        Settings.Default.Save();
      } catch (Exception exception) {
        System.Windows.MessageBox.Show(" Error Saving Preferences.\r\n" + exception.Message);
      }
      try {
        this.NotifyObservers();
      } catch {
      }
      base.Close();
    }

    public void NotifyObservers()
    {
      try {
        foreach (IObserver observer in this.observerContainer.Keys) {
          observer.Notify(true);
        }
      } catch {
      }
    }

    public void Register(IObserver anObserver)
    {
      try {
        this.observerContainer.Add(anObserver, anObserver);
      } catch {
      }
    }

    public void UnRegister(IObserver anObserver)
    {
      try {
        this.observerContainer.Remove(anObserver);
      } catch {
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      try {
        this.txtDownloadFolder.Text = Settings.Default.DownloadFolder;
        this.timePicker1.SelectedHour = Settings.Default.TimerHours;
        this.timePicker1.SelectedMinute = Settings.Default.TimerMinutes;
        this.timePicker1.SelectedSecond = Settings.Default.TimerSeconds;
        this.txtMaxRetries.Text = Settings.Default.maxRetries.ToString();
        this.txtTimeout.Text = Settings.Default.TimeoutNoResponse.ToString();
        this.chkAutoSaveOnAdd.IsChecked = new bool?(Settings.Default.AutoSaveOnAdd);
        this.chkAutoLoadWatchList.IsChecked = new bool?(Settings.Default.AutoLoadWatchList);
        this.chkDeleteFailedDownloads.IsChecked = new bool?(Settings.Default.deleteFailedDownloads);
        this.txtMaxConcurrentTransfers.Text = Settings.Default.MaxConcurrentDownloads.ToString();
        this.chkMinimizeToTray.IsChecked = new bool?(Settings.Default.MinimizeToTray);
      } catch (Exception exception) {
        System.Windows.MessageBox.Show("Error windowPreferences_OnLoad.\r\n" + exception.Message);
      }

    }


  }
}
