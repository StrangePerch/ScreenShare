using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Protocol;

namespace Client
{
    public static class ScreenSend
    {
        private static int fps = 30;
        private static int UdpPort => int.Parse(ConfigurationManager.AppSettings["UDP-PORT"]);
        private static IPAddress Ip => IPAddress.Parse(ConfigurationManager.AppSettings["IP"]);

        private static readonly UdpClient _server = new UdpClient(Ip.ToString(), UdpPort);
        //private readonly UdpClient _server = new UdpClient("127.0.0.1", 57650);

        private static IPEndPoint endPoint = null;
        
        public static void SendScreen()
        {
            Transfer.SendUpdBig(_server, ScreenShot());
        }

        public static byte[] ScreenShot()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            using (Bitmap bmp = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppRgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }
                return BitmapToByteArray(bmp);
            }

        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                return memory.ToArray();
            }
        }

    }
}