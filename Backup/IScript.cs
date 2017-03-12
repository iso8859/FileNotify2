using System;
using System.Collections.Generic;
using System.Text;

namespace FileNotify2
{
    public abstract class IScript
    {
        public string m_directory = null;
        public object m_userData = null;

        public virtual void NewFile(FileNotify2.DirectoryPicture.Win32FindData file)
        {
        }

        public virtual void DeletedFile(FileNotify2.DirectoryPicture.Win32FindData file)
        {
        }

        public virtual void ChangedFile(FileNotify2.DirectoryPicture.Win32FindData before, FileNotify2.DirectoryPicture.Win32FindData now)
        {
        }

        public virtual void IdenticalFile(FileNotify2.DirectoryPicture.Win32FindData file)
        {
        }
    }
}
