using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Xml.Serialization;

namespace FileNotify2
{
    public class DirectoryPicture
    {
        public const int MAX_PATH = 260;
        public const int MAX_ALTERNATE = 14;

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        public class FileTime
        {
            [XmlAttribute]
            public uint dwLowDateTime;
            [XmlAttribute]
            public uint dwHighDateTime;

            public FileTime() // For serialization
            {
            }

            public FileTime(FILETIME other)
            {
                dwLowDateTime = other.dwLowDateTime;
                dwHighDateTime = other.dwHighDateTime;
            }

            public bool Equal(FileTime other)
            {
                return (dwLowDateTime == other.dwLowDateTime && dwHighDateTime == other.dwHighDateTime);
            }

            public long GetLong()
            {
                return (((long)dwHighDateTime) << 32) + dwLowDateTime;
            }
        };

        public const int FILE_ATTRIBUTE_READONLY = 0x00000001;
        public const int FILE_ATTRIBUTE_HIDDEN = 0x00000002;
        public const int FILE_ATTRIBUTE_SYSTEM = 0x00000004;
        public const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public const int FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        public const int FILE_ATTRIBUTE_DEVICE = 0x00000040;
        public const int FILE_ATTRIBUTE_NORMAL = 0x00000080;
        public const int FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
        public const int FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200;
        public const int FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
        public const int FILE_ATTRIBUTE_COMPRESSED = 0x00000800;
        public const int FILE_ATTRIBUTE_OFFLINE = 0x00001000;
        public const int FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
        public const int FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WIN32_FIND_DATA
        {
            [XmlAttribute]
            public int dwFileAttributes;
            [XmlAttribute]
            public FILETIME ftCreationTime;
            [XmlAttribute]
            public FILETIME ftLastAccessTime;
            [XmlAttribute]
            public FILETIME ftLastWriteTime;
            [XmlAttribute]
            public int nFileSizeHigh;
            [XmlAttribute]
            public int nFileSizeLow;
            [XmlIgnore]
            public int dwReserved0;
            [XmlIgnore]
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ALTERNATE)]
            public string cAlternate;
        }

        public class Win32FindData
        {
            public int dwFileAttributes;
            public FileTime ftCreationTime;
            public FileTime ftLastAccessTime;
            public FileTime ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public string cFileName;
            public string cAlternate;

            public Win32FindData() // For serialization
            {
            }

            public Win32FindData(WIN32_FIND_DATA other)
            {
                dwFileAttributes = other.dwFileAttributes;
                ftCreationTime = new FileTime(other.ftCreationTime);
                ftLastAccessTime = new FileTime(other.ftLastAccessTime);
                ftLastWriteTime = new FileTime(other.ftLastWriteTime);
                nFileSizeHigh = other.nFileSizeHigh;
                nFileSizeLow = other.nFileSizeLow;
                cFileName = other.cFileName;
                cAlternate = other.cAlternate;
            }

            public bool Equal(Win32FindData other)
            {
                return (cFileName == other.cFileName) && (nFileSizeHigh == other.nFileSizeHigh) &&
                    (nFileSizeLow == other.nFileSizeLow) && (dwFileAttributes == other.dwFileAttributes) &&
                    ftCreationTime.Equal(other.ftCreationTime) && ftLastAccessTime.Equal(other.ftLastAccessTime) &&
                    ftLastWriteTime.Equal(other.ftLastWriteTime);
            }

            public DateTime CreationTime
            {
                get { return DateTime.FromFileTimeUtc(ftCreationTime.GetLong()).ToLocalTime(); }
            }

            public DateTime LastAccessTime
            {
                get { return DateTime.FromFileTimeUtc(ftLastAccessTime.GetLong()).ToLocalTime(); }
            }

            public DateTime LastWriteTime
            {
                get { return DateTime.FromFileTimeUtc(ftLastWriteTime.GetLong()).ToLocalTime(); }
            }
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        static extern bool FindClose(IntPtr hFindFile);

        public Hashtable m_snapshot = new Hashtable();

        public static DirectoryPicture TakeSnapshot(string directory)
        {
            //Console.WriteLine("Snapshot " + directory);
            DirectoryPicture result = new DirectoryPicture();
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            WIN32_FIND_DATA findData;
            IntPtr findHandle = FindFirstFile(directory, out findData);
            if (findHandle != INVALID_HANDLE_VALUE)
            {
                do
                {
                    if ((findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == 0)
                        result.m_snapshot[findData.cFileName] = new Win32FindData(findData);
                }
                while (FindNextFile(findHandle, out findData));
                FindClose(findHandle);
            }
            return result;
        }

        public bool IsIdentical(DirectoryPicture otherSnapshot)
        {
            bool bResult = false;
            if (m_snapshot.Count>0 && m_snapshot.Count == otherSnapshot.m_snapshot.Count)
            {
                int exact = 0;
                foreach (string name in m_snapshot.Keys)
                {
                    Win32FindData local = (Win32FindData)m_snapshot[name];
                    Win32FindData other = (Win32FindData)otherSnapshot.m_snapshot[name];
                    if (other != null)
                    {
                        if (local.Equal(other))
                            exact++;
                    }
                    else
                        break;
                }
                bResult = (exact == m_snapshot.Count);
            }
            return bResult;
        }
    }
}
