using System;
using System.Collections.Generic;
using System.Text;
using FileNotify2;

namespace fnConsole
{
    public class MoveFTP : IScript
    {
        public override void IdenticalFile(DirectoryPicture.Win32FindData file)
        {
            try
            {
                System.IO.File.Move(m_directory + "\\" + file.cFileName, @"c:\temp");
            }
            catch { }
        }
    }
}
