using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FileNotify2
{
    public class Trigger : XMLSerialize<Trigger>
    {
        public enum Mode { Never, Always, Schedule, AlwaysMono }
        // Always = dès qu'un groupe de fichier est dans état stable
        // AlwaysMono = dés qu'un fichier est dans un état stable
        // Schedule = basé uniquement sur l'heure
        [XmlAttribute]
        public Mode m_mode { get; set; }
        [XmlAttribute]
        public string m_year { get; set; } // 2006 or *
        [XmlAttribute]
        public string m_month { get; set; } // 2,4,6 or 5-8, or *
        [XmlAttribute]
        public string m_day { get; set; } // 1,7,14,last or *
        [XmlAttribute]
        public string m_dow { get; set; } // 0=sunday, 6=saturday
        [XmlAttribute]
        public string m_hour { get; set; } // 4,5, 7-9, *
        [XmlAttribute]
        public string m_min { get; set; }
        [XmlAttribute]
        public string m_sec { get; set; }
        [XmlAttribute]
        public int m_secAlways { get; set; }
        [XmlAttribute]
        public string m_cronId { get; set; }
        [XmlIgnore]
        public DateTime m_nextTime { get; set; }

        public Trigger()
        {
            m_mode = Mode.Never;
            m_year = "*";
            m_month = "*";
            m_day = "*";
            m_dow = "*";
            m_hour = "*";
            m_min = "*";
            m_sec = "0";
        }

        /*
        public Trigger(string year, string month, string day, string hour, string min, string sec, string cronId)
        {
            m_year = year;
            m_month = month;
            m_day = day;
            m_hour = hour;
            m_min = min;
            m_sec = sec;
            m_cronId = cronId;
        }
        */

        private int GetNext(string values, int actual, int max, ref int ret)
        {
            int distance = -1;
            int result = actual;
            string[] items = values.Split(',');
            foreach (string item in items)
            {
                string tmp = item;
                if (item == "*")
                {
                    distance = -1;
                    ret = 0;
                    break;
                }
                if (item == "last")
                    tmp = max.ToString();
                try
                {
                    int val = Convert.ToInt32(tmp);
                    int d = val - actual;
                    int ret2 = 0;
                    if (d < 0)
                    {
                        d += max;
                        ret2 = 1;
                    }
                    if (distance == -1 || d < distance)
                    {
                        distance = d;
                        ret = ret2;
                        result = val;
                    }
                }
                catch { }
            }
            return result;
        }

        public DateTime GetNextTime(DateTime now)
        {
            DateTime result = DateTime.MaxValue;
            switch (m_mode)
            {
                case Mode.Schedule:
                    int ret = 0;
                    int sec = GetNext(m_sec, now.Second, 60, ref ret);
                    int min = GetNext(m_min, now.Minute + ret, 60, ref ret);
                    int hour = GetNext(m_hour, now.Hour + ret, 24, ref ret);
                    int dim = DateTime.DaysInMonth(now.Year, now.Month);
                    int d = now.Day;
                    if (m_day == "*") // Prioritaire, on peut avoir les deux
                        d = GetNext(m_day, d + ret, dim, ref ret);
                    else
                    {
                        int thisDay = (int)now.DayOfWeek;
                        int nextDay = GetNext(m_dow, thisDay + ret, 7, ref ret);
                        if (nextDay < thisDay || ret == 1)
                            nextDay += 7;
                        d += nextDay - thisDay;
                        ret = 0;
                    }
                    int m = GetNext(m_month, now.Month + ret, 12, ref ret);
                    int y = GetNext(m_year, now.Year + ret, now.Year, ref ret);

                    result = now;
                    result = result.AddSeconds(sec - now.Second);
                    result = result.AddMinutes(min - now.Minute);
                    result = result.AddHours(hour - now.Hour);
                    result = result.AddDays(d - now.Day);
                    result = result.AddMonths(m - now.Month);
                    result = result.AddYears(y - now.Year);
                    break;
                case Mode.Always:
                case Mode.AlwaysMono:
                    result = now;
                    break;
            }
            return result;
        }

        public override string ToString()
        {
            string result = " - ";
            if (m_mode == Mode.Always || m_mode == Mode.AlwaysMono)
                result = " * ";
            else if (m_mode == Mode.Schedule)
            {
                DateTime date = GetNextTime(DateTime.Now);
                result = date.ToShortDateString() + " - " + date.ToShortTimeString();
            }
            return result;
        }
    }

    public class TriggerEngine : IDisposable
    {
        public List<Trigger> m_triggers = new List<Trigger>();
        public delegate void StartHandler(string id);
        public event StartHandler StartEvent;
        public delegate void StartingHandler(string id, TimeSpan delta);

        private System.Threading.AutoResetEvent m_timer;
        private System.Threading.Thread m_thread;

        public void Clear()
        {
            lock (m_triggers)
            {
                m_triggers.Clear();
            }
        }

        public void Add(Trigger trigger)
        {
            lock (m_triggers)
            {
                m_triggers.Add(trigger);
            }
        }

        public void Stop()
        {
            if (m_timer != null && m_thread != null)
            {
                m_timer.Set();
                if (m_thread.ManagedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
                    m_thread.Join();
            }
        }

        public void Start()
        {
            Stop();
            m_timer = new System.Threading.AutoResetEvent(false);
            m_thread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadProc));
            m_thread.Start();
        }

        void ThreadProc()
        {
            // Console.WriteLine("ThreadProc, StartEvent=" + (StartEvent != null));
            bool bExit = false;
            DateTime lastnow = DateTime.Now;
            while (!bExit && m_triggers.Count > 0 && StartEvent != null)
            {
                TimeSpan min = TimeSpan.MaxValue;
                DateTime now = DateTime.Now;
                TimeSpan diff = now - lastnow;
                if (diff.Seconds < 1)
                    System.Threading.Thread.Sleep(1000);
                else
                {
                    DateTime nextTime = DateTime.MaxValue;
                    // Recherche la prochaine heure de démarrage (la plus proche)
                    lock (m_triggers)
                    {
                        foreach (Trigger t in m_triggers)
                        {
                            t.m_nextTime = t.GetNextTime(now);
                            if (t.m_nextTime != DateTime.MaxValue)
                            {
                                TimeSpan tmp = t.m_nextTime - now;
                                if (tmp < min)
                                {
                                    min = tmp;
                                    nextTime = t.m_nextTime;
                                }
                            }
                        }
                    }
                    if (min == TimeSpan.MaxValue)
                        bExit = m_timer.WaitOne();
                    else
                    {
                        // On tourne toute les minutes en cas de changement d'heure de la machine
                        if (min.Minutes > 1)
                        {
                            min = new TimeSpan(0, 1, 0);
                            nextTime = now.AddMinutes(1);
                        }
                        // Console.WriteLine("WaitOne ({0})", min);
                        bExit = m_timer.WaitOne(min, false); // Attendre le temps qu'il faut
                        // Test pour voir si nous sommes au moins à nextTime
                        now = DateTime.Now;
                        while (!bExit && now < nextTime)
                        {
                            System.Threading.Thread.Sleep(0);
                            now = DateTime.Now;
                        }
                        // Console.WriteLine("{0} - {1}", now.ToString("o"), DateTime.Now.ToString("o"));
                    }
                    if (!bExit)
                    {
                        now = DateTime.Now;
                        foreach (Trigger t in m_triggers)
                        {
                            // Console.WriteLine("{0} {1} {2}", t.m_nextTime.ToString("o"), now.ToString("o"), (t.m_nextTime <= now));
                            if (t.m_nextTime <= now) // l'heure du PC n'est pas forcement précise et retarde ou avance
                            {
                                // Console.WriteLine("StartEvent({0})", t.m_cronId);
                                StartEvent(t.m_cronId);
                            }
                        }
                        lastnow = now;
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}

