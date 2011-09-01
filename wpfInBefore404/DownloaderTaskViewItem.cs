using System;
using System.Runtime.CompilerServices;

namespace wpfInBefore404
{
  public class DownloaderTaskViewItem
  {
    public DownloaderTask downloaderTask;
    public string BytesDownloaded { get; set; }
    public string BytesTotal { get; set; }
    public string PercentDownloaded { get; set; }
    public string Retries { get; set; }
    public string URL { get; set; }
  }
}

