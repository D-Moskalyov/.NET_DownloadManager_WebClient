using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DownloadManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<System.Windows.Controls.Button, DownLoadInfo> pauseDict;
        Dictionary<System.Windows.Controls.Button, DownLoadInfo> stopDict;

        string pathToCopyTemp = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            pauseDict = new Dictionary<System.Windows.Controls.Button, DownLoadInfo>();
            stopDict = new Dictionary<System.Windows.Controls.Button, DownLoadInfo>();

            from.Text = "http://releases.ubuntu.com/14.04.1/ubuntu-14.04.1-desktop-amd64.iso";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string fileURI = from.Text;

            WebClient client = new WebClient();

            client.OpenReadCompleted += client_OpenReadCompleted;

            try
            {
                FolderBrowserDialog foldBrowDialog = new FolderBrowserDialog();
                foldBrowDialog.Description = "Выбирите папку для сохранения";
                DialogResult result = foldBrowDialog.ShowDialog();



                if (result == System.Windows.Forms.DialogResult.OK && (from.Text.Length - from.Text.LastIndexOf('/') - 1) != 0)
                {
                    Console.WriteLine(foldBrowDialog.SelectedPath);
                    pathToCopyTemp = foldBrowDialog.SelectedPath;
                    client.OpenReadAsync(new Uri(fileURI));
                    
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                Console.WriteLine(e.ToString());
                int last = from.Text.LastIndexOf('/') + 1;
                char[] name = new char[from.Text.Length - last];
                from.Text.CopyTo(last, name, 0, from.Text.Length - last);
                string pathToCopy = string.Concat(pathToCopyTemp, "\\", new string(name));

                Console.WriteLine(pathToCopy);

                if (!File.Exists(pathToCopy))
                {
                    System.Windows.Controls.Label label = new System.Windows.Controls.Label();
                    label.Content = new string(name);
                    label.Height = 30;
                    label.Width = 90;
                    label.Margin = new Thickness(5);
                    label.ToolTip = pathToCopy;

                    stackPanel.Children.Add(label);

                    System.Windows.Controls.ProgressBar pB = new System.Windows.Controls.ProgressBar();
                    pB.Height = 20;
                    pB.Width = 350;
                    pB.Margin = new Thickness(5);

                    stackPanel.Children.Add(pB);

                    System.Windows.Controls.Button pauseBtn = new System.Windows.Controls.Button();
                    pauseBtn.Content = "||";
                    pauseBtn.Height = pauseBtn.Width = 20;
                    pauseBtn.Margin = new Thickness(5);
                    pauseBtn.Click += pauseBtn_Click;

                    stackPanel.Children.Add(pauseBtn);

                    System.Windows.Controls.Button stopBtn = new System.Windows.Controls.Button();
                    stopBtn.Content = "[]";
                    stopBtn.Height = stopBtn.Width = 20;
                    stopBtn.Margin = new Thickness(5);
                    stopBtn.Click += stopBtn_Click;

                    stackPanel.Children.Add(stopBtn);

                    Thread thread = new Thread(DownloadInThread);
                    thread.IsBackground = true;

                    DownLoadInfo downInfo = new DownLoadInfo(thread, pathToCopy, pB, e.Result, label);

                    pauseDict.Add(pauseBtn, downInfo);
                    stopDict.Add(stopBtn, downInfo);

                    thread.Start(downInfo);
                }
                else
                {
                    System.Windows.MessageBox.Show(pathToCopy + " уже существует");
                }
                
            }
            else
            {
                System.Windows.MessageBox.Show("Путь не существует");
            }
            
        }

        void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pauseDict.ContainsKey((System.Windows.Controls.Button)sender))
            {
                DownLoadInfo dInfo = pauseDict[(System.Windows.Controls.Button)sender];

                if (!dInfo.Complete)
                {
                    Console.WriteLine(dInfo.Thread.ThreadState.ToString());
                    if ((dInfo.Thread.ThreadState & ThreadState.Suspended) != 0)
                    {
                        dInfo.Thread.Resume();
                        dInfo.ProgressBar.Foreground = Brushes.Green;
                    }
                    else
                    {
                        dInfo.Thread.Suspend();
                        dInfo.ProgressBar.Foreground = Brushes.Yellow;
                    }
                }
                else
                {
                    pauseDict.Remove((System.Windows.Controls.Button)sender);
                    stopDict.Remove((System.Windows.Controls.Button)sender);
                }
            }

        }

        void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (stopDict.ContainsKey((System.Windows.Controls.Button)sender))
            {
                DownLoadInfo dInfo = stopDict[(System.Windows.Controls.Button)sender];

                if (!dInfo.Complete)
                {
                    if ((dInfo.Thread.ThreadState & ThreadState.Suspended) != 0)
                        dInfo.Thread.Resume();

                    dInfo.Thread.Abort();
                    dInfo.Complete = true;
                    pauseDict.Remove((System.Windows.Controls.Button)sender);
                    stopDict.Remove((System.Windows.Controls.Button)sender);
                }
                else
                {
                    pauseDict.Remove((System.Windows.Controls.Button)sender);
                    stopDict.Remove((System.Windows.Controls.Button)sender);
                }
            }
        }

        void DownloadInThread(object dInfos)
        {
            
            int kbCount = 0;

            int part = 1024;

            DownLoadInfo dInfo = dInfos as DownLoadInfo;

            Stream stream = dInfo.Stream;

            FileStream fs = new FileStream(dInfo.Path, FileMode.Create);
            //FileStream fs = new FileStream("E:\\ubuntu-14.04.1-desktop-amd64.iso\0\0\0", FileMode.Create);

            byte[] buffer = new byte[part];

            int bytesCount = 0;

            try
            {
                while ((bytesCount = stream.Read(buffer, 0, part)) != 0)
                {
                    kbCount++;

                    dInfo.ProgressBar.Dispatcher.Invoke(new Action(() =>
                    {
                        dInfo.ProgressBar.Value = kbCount / 1000;
                    }));

                    fs.Write(buffer, 0, bytesCount);
                }

                Console.WriteLine(dInfo.Path + " completed");
                dInfo.Complete = true;

                stream.Close();
                fs.Close();
            }
            catch (ThreadAbortException exp)
            {
                Console.WriteLine("Словил " + exp.Message);

                stream.Close();
                fs.Close();

                File.Delete(dInfo.Path);
                dInfo.Complete = true;
                dInfo.ProgressBar.Dispatcher.Invoke(new Action(() =>
                {
                    dInfo.ProgressBar.Foreground = Brushes.Red;
                }));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dictionary<System.Windows.Controls.Button, DownLoadInfo>.ValueCollection vCol = pauseDict.Values;

            foreach (DownLoadInfo dInfo in vCol)
            {
                if ((dInfo.Thread.ThreadState & ThreadState.Suspended) != 0)
                {
                    dInfo.Thread.Resume();
                }
                dInfo.Thread.Abort();
            }
        }
    }
}
