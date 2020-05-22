using HttpServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Automation;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        public const int WM_CLOSE = 0x10;
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        public delegate bool CallBack(int hwnd, int lParam);
        [DllImport("user32")] public static extern int EnumWindows(CallBack x, int y);
        [DllImport("user32.dll")] private static extern int GetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")] private static extern int GetClassNameW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);

        private static System.Timers.Timer aTimer;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //KillProcess("chrome");
            CloseAllChromeBrowsers();

            IntPtr hwnd_win;
            hwnd_win = FindWindow("Chrome_WidgetWin_0", null );
            SendMessage(hwnd_win, WM_CLOSE, 0, 0);


            CallBack myCallBack = new CallBack(Recall);
            EnumWindows(myCallBack, 0);
        }

        public void CloseAllChromeBrowsers()
        {
            foreach (Process process in Process.GetProcessesByName("chrome"))
            {
                if (process.MainWindowHandle == IntPtr.Zero) // some have no UI
                    continue;

                AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
                if (element != null)
                {
                    ((WindowPattern)element.GetCurrentPattern(WindowPattern.Pattern)).Close();
                }
            }
        }

        public bool Recall(int hwnd, int lParam)
        {
            StringBuilder sb = new StringBuilder(256);
            IntPtr PW = new IntPtr(hwnd);

            GetWindowTextW(PW, sb, sb.Capacity); //得到窗口名并保存在strName中
            string strName = sb.ToString();

            GetClassNameW(PW, sb, sb.Capacity); //得到窗口类名并保存在strClass中
            string strClass = sb.ToString();

            if (strClass.ToLower().Contains("chrome"))
            {

            }

            if (strName.ToLower().Contains("chrome"))
            {

            }

            if (strName.IndexOf("") >= 0 && strClass.IndexOf("Chrome") >= 0)
            {
                return false; //返回false中止EnumWindows遍历
            }
            else
            {
                return true; //返回true继续EnumWindows遍历
            }
        }


        public  void KillProcess(string strProcessesByName)//关闭线程
        {
            var pre = Process.GetProcessesByName(strProcessesByName);
            foreach (Process p in Process.GetProcesses())//GetProcessesByName(strProcessesByName))
            {
                if (p.ProcessName.ToUpper().Contains(strProcessesByName.ToUpper()))
                {
                    try
                    {
                        p.Kill();
                        p.Exited += (sender, e) =>
                        {
                            MessageBox.Show("结束成功");   // process was terminating or can't be terminated - deal with it
                        }; // possibly with a timeout
                    }
                    catch (Win32Exception e)
                    {
                        MessageBox.Show(e.Message.ToString());   // process was terminating or can't be terminated - deal with it
                    }
                    catch (InvalidOperationException e)
                    {
                        MessageBox.Show(e.Message.ToString()); // process has already exited - might be able to let this one go
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {


            ReloadChrome();
            //ExecuteCommandAsync(shell);

        }

        /// <summary>
        /// 重启浏览器
        /// </summary>
        public void ReloadChrome()
        {
            Process proc = null;
            try
            {
                proc = new Process();
                proc.StartInfo.FileName = @"runChrom.bat";
                proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.CreateNoWindow = false;

                if (checkBox1.Checked)
                {
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }

                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Executes a shell command synchronously.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="command">string command</param></span>
        /// <span class="code-SummaryComment"><returns>string, as output of the command.</returns></span>
        public void ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Execute the command Asynchronously.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="command">string command.</param></span>
        public void ExecuteCommandAsync(string command)
        {
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(command);
            }
            catch (ThreadStartException objException)
            {
                // Log the exception
            }
            catch (ThreadAbortException objException)
            {
                // Log the exception
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }


        ExampleServer.Callback caCllback;


        private void button3_Click(object sender, EventArgs e)
        {
            var port = Convert.ToInt32(this.txtPort.Text);
            Task.Factory.StartNew(() =>
            {
                ExampleServer server = new ExampleServer("0.0.0.0", port);
                server.callback = new ExampleServer.Callback(() => {
                    aTimer.Stop();
                    aTimer.Start();

                });
                server.Start();

               

            });


            // 10秒检查一次
            aTimer = new System.Timers.Timer(10000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += (Object source, ElapsedEventArgs e1) => {
                //如果停止连接
                if (!ExampleServer.ISCONNECT)
                {
                    label3.Invoke((Action)delegate { 
                        label3.Text = "将要重启浏览器... ";
                    });
                    aTimer.Stop();
                    Task.Factory.StartNew(ReloadChrome);
                    Thread.Sleep(10000);
                    aTimer.Start();
                }
                else
                {
                    label3.Invoke((Action)delegate {
                        label3.Text = "";
                    });
                }
            };
            aTimer.AutoReset = true;
            aTimer.Enabled = true;


            button3.Enabled = false;
            //WindowState = FormWindowState.Minimized;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ExampleServer.ISCONNECT)
            {
                lblStatus.Text = "已连接";
                lblStatus.BackColor = Color.ForestGreen;
            }
            else
            {
                lblStatus.Text = "未连接";
                lblStatus.BackColor = Color.Red;
            }
        }
    }
}
