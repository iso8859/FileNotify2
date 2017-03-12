using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace fnService
{
    public partial class Service1 : ServiceBase
    {
        FileNotify2.fnlib lib = new FileNotify2.fnlib();

        public Service1()
        {
            InitializeComponent();
        }

        public void Thread()
        {
            lib.Start();
        }

        public void Start()
        {
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(Thread));
            thread.Start();
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        protected override void OnStop()
        {
            Stop();
        }
    }
}
