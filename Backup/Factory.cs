using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace FileNotify2
{
    public class Factory
    {
        Hashtable m_registeredInstances = new Hashtable();
        static Factory m_singleton = null;

        static public Factory GetInstance()
        {
            if (m_singleton == null)
                m_singleton = new Factory();
            return m_singleton;
        }

        public IScript FindInstance(Guid guid)
        {
            IScript result = null;
            if (m_registeredInstances.ContainsKey(guid))
                result = (IScript)m_registeredInstances[guid];
            return result;
        }

        public void AddInstance(Guid guid, IScript instance)
        {
            m_registeredInstances[guid] = instance;
        }
    }
}
