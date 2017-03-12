using System;
using System.Collections.Generic;
using System.Text;

namespace fnConsole
{
    class Program
    {
        FileNotify2.fnlib lib = new FileNotify2.fnlib();

        public void Thread()
        {
            lib.Start();
        }

        public void Start()
        {
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(Thread));
            thread.Start();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            lib.Stop();
        }

        static void Main(string[] args)
        {
            Program instance = new Program();
            instance.Start();

            //FileNotify2.Factory.GetInstance().AddInstance(new Guid("5f55c733-fc4b-45ad-bfe2-79f94fd8cea3"), new fnConsole.SendMail());
            //FileNotify2.fnlib lib = new FileNotify2.fnlib();
            //lib.Start();
        }
    }
}
