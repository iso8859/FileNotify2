using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FileNotify2
{

    public class Setting
    {
        public Guid     m_guid;
        public string   m_name;
        public string   m_directory;  // The directory to look at
        public string   m_filter;     // File filter (*.txt, *, ...)
        public string   m_script;     // The script to execute
        public Trigger  m_cron = new Trigger();
        public bool m_persistant, m_active;

        [XmlIgnore]
        public DirectoryPicture m_lastPicture;
        [XmlIgnore]
        public DateTime m_lastExecute;

        public override string ToString()
        {
            return m_name;
        }
    }

    public class Settings : XMLSerialize<Settings>
    {
        public List<Setting> m_settings = new List<Setting>();
    }

    public class Snapshoot
    {
        public Guid m_guid;
        public List<FileNotify2.DirectoryPicture.Win32FindData> m_files = new List<FileNotify2.DirectoryPicture.Win32FindData>();
    }

    public class Snapshoots : XMLSerialize<Snapshoots>
    {
        public List<Snapshoot> m_snapshoots = new List<Snapshoot>();

        public DirectoryPicture ToDirectoryPicture(Guid guid)
        {
            DirectoryPicture result = new DirectoryPicture();
            foreach (Snapshoot ss in m_snapshoots)
            {
                if (ss.m_guid == guid)
                {
                    foreach (FileNotify2.DirectoryPicture.Win32FindData file in ss.m_files)
                        result.m_snapshot[file.cFileName] = file;
                }
            }
            return result;
        }

        public void AddSnapshoot(Guid guid, DirectoryPicture picture)
        {
            if (picture != null)
            {
                Snapshoot s = new Snapshoot();
                s.m_guid = guid;
                foreach (FileNotify2.DirectoryPicture.Win32FindData data in picture.m_snapshot.Values)
                    s.m_files.Add(data);
                m_snapshoots.Add(s);
            }
        }
    }
}
