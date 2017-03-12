using System;
using System.Xml.Serialization;
using System.IO;

namespace FileNotify2
{
    /// <summary>
    /// Serialize/Deserialize in XML format to file, stream or string
    /// </summary>
    public class XMLSerialize<T> : MarshalByRefObject where T : new()
    {
        // The serializer is static so the CLR build it only once
        static XmlSerializer m_serializer = new XmlSerializer(typeof(T));

        static public T LoadFromFile(string fileName)
        {
            T result;
            try
            {
                using (FileStream input = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    result = (T)m_serializer.Deserialize(input);
                }
            }
            catch
            {
                result = new T();
            }
            return result;
        }

        static public T LoadFromString(string data)
        {
            T result;
            try
            {
                using (StringReader sr = new StringReader(data))
                {
                    result = (T)m_serializer.Deserialize(sr);
                }
            }
            catch
            {
                result = new T();
            }
            return result;
        }

        static public T LoadFromStream(Stream stream)
        {
            T result;
            try
            {
                XmlSerializer read = new XmlSerializer(typeof(T));
                result = (T)m_serializer.Deserialize(stream);
            }
            catch
            {
                result = new T();
            }
            return result;
        }

        public void SaveToStream(Stream stream)
        {
            BeforeSave();
            m_serializer.Serialize(stream, this);
        }

        public string SaveToString()
        {
            BeforeSave();
            StringWriter sw = new StringWriter();
            m_serializer.Serialize(sw, this);
            return sw.ToString();
        }

        public void SaveToFile(string fileName)
        {
            BeforeSave();
            using (StreamWriter sm = new StreamWriter(fileName))
            {
                m_serializer.Serialize(sm, this);
            }
        }

        public virtual void BeforeSave()
        {
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
