using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Server
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
        }

        public void SetOnline(TextBlock block)
        {
            this.Dispatcher.Invoke(() =>
            {
                block.Text = "Online";
                block.Foreground = Brushes.Green;
            });
        }

        public void SetOffline(TextBlock block)
        {
            this.Dispatcher.Invoke(() =>
            {
                block.Text = "Offline";
                block.Foreground = Brushes.Red;
            });
        }

        private void InfoWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
