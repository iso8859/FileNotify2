using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;

namespace FileNotify2
{
    [Serializable]
    public class CompileParam : MarshalByRefObject
    {
        public string m_inSource, m_outName;
        public string[] m_dependencies;
        public bool m_bDebug;

        public CompileParam()
        {
        }

        public CompileParam(string inSource, string outName, bool bDebug, string[] dependencies)
        {
            m_inSource = inSource;
            m_outName = outName;
            m_bDebug = bDebug;
            m_dependencies = dependencies;
        }
    }

    public class ExecuteCompileScript
    {
        // This version load the compiled assembly in the current domain => you can't unload it
        static public CompilerResults Compile(CompileParam cparam)
        {
            string[] dependencies = new string[] { "System.dll" };
            if (cparam.m_dependencies!=null)
                dependencies = cparam.m_dependencies;

            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();

            cp.GenerateExecutable = false;
            cp.OutputAssembly = cparam.m_outName;
            cp.IncludeDebugInformation = cparam.m_bDebug;
            foreach (string s in dependencies)
                cp.ReferencedAssemblies.Add(s);
            cp.GenerateInMemory = false;
            cp.WarningLevel = 3;
            cp.TreatWarningsAsErrors = false;
            cp.CompilerOptions = "/optimize";

            // Invoke compilation.
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, cparam.m_inSource);
            return cr;
        }

        /*
        static public CompilerResults CompileInDomain(CompileParam cparam, int timeout)
        {
            CompilerResults result = null;
            ExecuteEngine ee = new ExecuteEngine();
            ee.AddScript(new ScriptInstance("Compile", typeof(Compile), cparam, null, ExecutionMode.Domain, System.Threading.ThreadPriority.Normal, null, timeout, true));
            int instance = ee.StartNow("Compile");
            result = ee.GetResult(instance, timeout) as CompilerResults;
            return result;
        }
        */
    }
}
