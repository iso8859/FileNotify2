using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace fnService2
{
    class Program
    {
        public class RunTask
        {
            FileNotify2.fnlib lib = new FileNotify2.fnlib();
            public void Start()
            {
                new Thread(() => lib.Start()).Start();
            }
            public void Stop()
            {
                lib.Stop();
            }
        }
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<RunTask>(s =>
                {
                    s.ConstructUsing(name => new RunTask());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("FileNotify2");
                x.SetDisplayName("FileNotify2");
                x.SetServiceName("FileNotify2");
            });
        }
    }
}
