using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Foreman
{
    class ProcfileEntry
    {
        private Procfile m_objProcfile = null;
        private int m_intIndex = 0;
        private string m_strName = null;
        private string m_strCommand = null;
        private Process m_objProcess = null;

        public ProcfileEntry(Procfile objProcfile, int intIndex, string strName, string strCommand)
        {
            m_objProcfile = objProcfile;
            m_intIndex = intIndex;
            m_strName = strName;
            m_strCommand = strCommand.Replace("$PORT", Port().ToString());
        }

        public string Header()
        {
            return (String.Format(@"{{\cf{0} {1} {2,-" + m_objProcfile.LongestNameLength() + "} |}} ", m_intIndex, "00:00:00", m_strName));
        }

        public int Port()
        {
            return (5000 + (100 * (m_intIndex - 1)));
        }

        public void Start()
        {
            m_objProcess = new Process();

            m_objProcess.StartInfo.CreateNoWindow = true;
            m_objProcess.StartInfo.UseShellExecute = false;
            m_objProcess.StartInfo.RedirectStandardOutput = true;
            m_objProcess.StartInfo.RedirectStandardError = true;

            m_objProcess.StartInfo.FileName = "cmd.exe";
            m_objProcess.StartInfo.Arguments = "/interactive /c " + m_strCommand;
            m_objProcess.StartInfo.WorkingDirectory = new FileInfo(m_objProcfile.FileName).DirectoryName;

            m_objProcess.EnableRaisingEvents = true;

            m_objProcess.OutputDataReceived += DataReceived;
            m_objProcess.ErrorDataReceived += DataReceived;
            m_objProcess.Exited += ProcessExited;

            TextReceived(this, "starting: " + m_strCommand);

            m_objProcess.Start();

            m_objProcess.BeginOutputReadLine();
            m_objProcess.BeginErrorReadLine();
        }

        public void Stop()
        {
            Dictionary<int, List<int>> arrPidMappings = ProcessUtilities.PidsByParent();

            if (! m_objProcess.HasExited)
            {
                m_objProcfile.Info(this, "stopping process");
                KillPid(arrPidMappings, m_objProcess.Id);
            }
        }

        public event Procfile.TextReceivedHandler TextReceived;

        public string Name
        {
            get
            {
                return (m_strName);
            }
        }

        private void KillPid(Dictionary<int, List<int>> arrPidMappings, int intPid)
        {
            if (arrPidMappings.ContainsKey(intPid))
            {
                foreach (int intChildPid in arrPidMappings[intPid])
                {
                    Debug.WriteLine("killing {0}", intChildPid);
                    KillPid(arrPidMappings, intChildPid);
                }
            }

            try
            {
                Process.GetProcessById(intPid).Kill();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error killing");
                Debug.WriteLine(ex.ToString());
            }
        }

        private void DataReceived(object objSender, DataReceivedEventArgs args)
        {
            TextReceived(this, args.Data);
        }

        private void ProcessExited(object objSender, EventArgs args)
        {
            m_objProcfile.Info(this, "process terminated");
            m_objProcfile.Stop();
        }
    }
}
