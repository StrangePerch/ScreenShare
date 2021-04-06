using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
    /// Interaction logic for Screen.xaml
    /// </summary>
    public partial class Screen : Window
    {
        private int TcpPort => int.Parse(ConfigurationManager.AppSettings["TCP-PORT"]);
        private int UdpPort => int.Parse(ConfigurationManager.AppSettings["UDP-PORT"]);

        public Screen()
        {
            InitializeComponent();

            ConnectionManager.ScreenWindow = this;
            ConnectionManager.RequestScreen = true;
        }
        private void FpsBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (ConnectionManager.fps <= 0)
                {
                    ConnectionManager.fps = 1;
                    FpsBox.Text = ConnectionManager.fps.ToString();
                }
                else
                {
                    ConnectionManager.fps = int.Parse(FpsBox.Text);
                }
            }
            catch (Exception exception)
            {
                FpsBox.Text = ConnectionManager.fps.ToString();
            }
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionManager.RequestScreen = true;
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionManager.RequestScreen = false;
        }

        private void Screen_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
