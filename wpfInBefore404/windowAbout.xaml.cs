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

namespace wpfInBefore404
{
  /// <summary>
  /// Interaction logic for windowAbout.xaml
  /// </summary>
  public partial class windowAbout : Window
  {
    public windowAbout()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      this.lblVersion.Content = "Version: " + System.Windows.Forms.Application.ProductVersion.ToString();

    }
  }
}

