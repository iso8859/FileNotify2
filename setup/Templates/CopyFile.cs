using System;
using System.IO;
using FileNotify2;

namespace fnConsole
{
    public class CopyFile : IScript
    {
        public override void NewFile(DirectoryPicture.Win32FindData file)
        {
            Console.WriteLine("File {0}, Creation {1}", file.cFileName, file.CreationTime);
        }
    }
}
