using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DownloadManager
{
    public class DownLoadInfo
    {
        bool complete;

        public bool Complete
        {
            get { return complete; }
            set { complete = value; }
        }

        System.Windows.Controls.Label label;

        public System.Windows.Controls.Label Label
        {
            get { return label; }
            set { label = value; }
        }
        Thread thread;

        public Thread Thread
        {
            get { return thread; }
            set { thread = value; }
        }
        string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        System.Windows.Controls.ProgressBar progressBar;

        public System.Windows.Controls.ProgressBar ProgressBar
        {
            get { return progressBar; }
            set { progressBar = value; }
        }
        System.IO.Stream stream;

        public System.IO.Stream Stream
        {
            get { return stream; }
            set { stream = value; }
        }

        public DownLoadInfo(Thread thr, string p, System.Windows.Controls.ProgressBar prBar,System.IO.Stream strm,  System.Windows.Controls.Label lb)
        {
            complete = false;
            thread = thr;
            path = p;
            progressBar = prBar;
            stream = strm;
            label = lb;
        }
    }
}
