using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;

namespace FileNotify2
{
    public class fnlib
    {
        System.Threading.ManualResetEvent m_exit = new System.Threading.ManualResetEvent(false);

        public void Start()
        {
            bool exit = false;
            Settings settings = Settings.LoadFromFile(AppDomain.CurrentDomain.BaseDirectory + "\\fileNotify2.xml");
            // Try to load script assembly
            foreach (Setting setting in settings.m_settings)
            {
                setting.m_lastExecute = DateTime.Now;
                IScript script = LoadScript(setting.m_guid.ToString()+".dll");
                if (script != null)
                    Factory.GetInstance().AddInstance(setting.m_guid, script);
            }
            Snapshoots snapshoots = Snapshoots.LoadFromFile(AppDomain.CurrentDomain.BaseDirectory + "\\snapshoots.xml");
            foreach (Setting setting in settings.m_settings)
                setting.m_lastPicture = snapshoots.ToDirectoryPicture(setting.m_guid);
            do
            {
                // Find next directory snapshot
                DateTime now = DateTime.Now;
                TimeSpan nextts = TimeSpan.MaxValue;
                Setting nexts = null;
                foreach (Setting setting in settings.m_settings)
                {
                    if (setting.m_active)
                    {
                        TimeSpan next = setting.m_cron.GetNextTime(now)-now;
                        if (next < nextts)
                        {
                            nextts = next;
                            nexts = setting;
                        }
                    }
                }
                if (nexts != null)
                {
                    if (nexts.m_lastExecute.AddSeconds(5) > now)
                        nextts = nexts.m_cron.GetNextTime(now.AddSeconds(5)) - now;
                    // Wait before doing the snapshot
                    Console.WriteLine("Wait {0} before executin {1}", nextts, nexts.m_name);
                    if (m_exit.WaitOne(nextts, false))
                        exit = true;
                    else
                    {
                        Console.WriteLine("Execute " + nexts.m_name);
                        nexts.m_lastExecute = DateTime.Now;
                        // Do the snapshoot
                        if (string.IsNullOrEmpty(nexts.m_filter))
                            nexts.m_filter="*";
                        DirectoryPicture picture = DirectoryPicture.TakeSnapshot(nexts.m_directory + "\\" + nexts.m_filter);

                        if (nexts.m_lastPicture != null)
                        {
                            if (!nexts.m_lastPicture.IsIdentical(picture))
                            {
                                IScript script = Factory.GetInstance().FindInstance(nexts.m_guid);
                                if (script == null && !string.IsNullOrEmpty(nexts.m_script))
                                {
                                    script = CompileScript(nexts);
                                    if (script != null)
                                        Factory.GetInstance().AddInstance(nexts.m_guid, script);
                                    else
                                        Console.WriteLine("Error compiling {0}", nexts.m_name);
                                }
                                if (script != null)
                                {
                                    script.m_directory = nexts.m_directory;
                                    // Missing files
                                    foreach (FileNotify2.DirectoryPicture.Win32FindData data in nexts.m_lastPicture.m_snapshot.Values)
                                    {
                                        if (!picture.m_snapshot.ContainsKey(data.cFileName))
                                            script.DeletedFile(data);
                                    }
                                    // New files
                                    foreach (FileNotify2.DirectoryPicture.Win32FindData data in picture.m_snapshot.Values)
                                    {
                                        if (!nexts.m_lastPicture.m_snapshot.ContainsKey(data.cFileName))
                                            script.NewFile(data);
                                    }
                                    // Changed files and identical one
                                    foreach (FileNotify2.DirectoryPicture.Win32FindData data in nexts.m_lastPicture.m_snapshot.Values)
                                    {
                                        FileNotify2.DirectoryPicture.Win32FindData data2 = picture.m_snapshot[data.cFileName] as FileNotify2.DirectoryPicture.Win32FindData;
                                        if (data2 != null)
                                        {
                                            if (data2.Equal(data))
                                                script.IdenticalFile(data);
                                            else
                                                script.ChangedFile(data, data2);
                                        }
                                    }
                                    // Identical files
                                }
                            }
                        }
                        nexts.m_lastPicture = picture;
                    }
                }
                else
                    exit = true; // All rule are desactivated
            }
            while (!exit);
            // Persist snapshoots
            snapshoots = new Snapshoots();
            foreach (Setting s in settings.m_settings)
            {
                if (s.m_persistant)
                    snapshoots.AddSnapshoot(s.m_guid, s.m_lastPicture);
            }
            snapshoots.SaveToFile(AppDomain.CurrentDomain.BaseDirectory + "\\snapshoots.xml");
        }

        public void Stop()
        {
            m_exit.Set();
        }

        public IScript LoadScript(string fileName)
        {
            IScript result = null;
            try
            {
                Assembly baseAssembly = Assembly.LoadFile(fileName, AppDomain.CurrentDomain.Evidence);
                if (baseAssembly != null)
                {
                    // Make an instance of the first class that implements interface FileNotify2.IScript
                    Type[] types = baseAssembly.GetTypes();
                    foreach (Type t in types)
                    {
                        if (t.BaseType.FullName == typeof(FileNotify2.IScript).FullName)
                        {
                            object tmp = baseAssembly.CreateInstance(t.ToString());
                            result = tmp as FileNotify2.IScript;
                            break;
                        }
                    }
                }
            }
            catch { }
            return result;
        }

        public IScript CompileScript(Setting setting)
        {
            IScript result = null;
            if (!string.IsNullOrEmpty(setting.m_script))
            {
                string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\" + setting.m_guid + ".dll";
                try
                {
                    System.IO.File.Delete(fileName);
                }
                catch { }
                CompileParam cp = new CompileParam(setting.m_script, fileName, false, new string[] { "System.dll", "FileNotify2.dll" });
                CompilerResults cr = ExecuteCompileScript.Compile(cp);
                if (cr.Errors.Count == 0)
                    result = LoadScript(fileName);
                else
                {
                    Console.WriteLine("Error compiling {0}", setting.m_name);
                    foreach (CompilerError error in cr.Errors)
                        Console.WriteLine(error);
                }
            }
            return result;
        }
    }
}
