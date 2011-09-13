using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Foreman
{
    class Procfile
    {
        public delegate void TextReceivedHandler(ProcfileEntry objEntry, string strText);
        public delegate void StatusRecievedHandler(string strText);

        private string m_strFilename = null;
        private List<ProcfileEntry> m_arrProcfileEntries = null;
        private bool m_blnStarted = false;

        public Procfile(string strFilename)
        {
            m_strFilename = strFilename;
            m_arrProcfileEntries = new List<ProcfileEntry>();

            string strContents = System.IO.File.ReadAllText(strFilename);
            int intCurrent = 1;

            foreach (string strLine in strContents.Split('\n'))
            {
                string[] arrLine = strLine.Split(':');

                if (arrLine.Length == 2)
                {
                    ProcfileEntry objProcfileEntry = new ProcfileEntry(this, intCurrent, arrLine[0].Trim(), arrLine[1].Trim());
                    objProcfileEntry.TextReceived += delegate(ProcfileEntry objEntry, string strData)
                    {
                        TextReceived(objEntry, strData);
                    };
                    m_arrProcfileEntries.Add(objProcfileEntry);
                    intCurrent += 1;
                }
            }
        }

        public string Header()
        {
            return (String.Format(@"{{{0} {1,-" + LongestNameLength() + "} |}} ", "00:00:00", "system"));
        }

        public void Start()
        {
            if (m_blnStarted)
                return;

            m_blnStarted = true;

            foreach (ProcfileEntry objProcfileEntry in m_arrProcfileEntries)
            {
                Thread objThread = new Thread(new ThreadStart(objProcfileEntry.Start));
                objThread.Start();
            }
        }

        public void Stop()
        {
            if (!m_blnStarted)
                return;

            StatusReceived("stopping all processes");

            m_blnStarted = false;

            foreach (ProcfileEntry objProcfileEntry in m_arrProcfileEntries)
            {
                objProcfileEntry.Stop();
            }
        }

        public void Info(ProcfileEntry objEntry, string strText)
        {
            TextReceived(objEntry, strText);
        }

        public event TextReceivedHandler TextReceived;
        public event StatusRecievedHandler StatusReceived;

        public int LongestNameLength()
        {
            int intLongestName = m_arrProcfileEntries.Select( objEntry => objEntry.Name.Length ).Max();
            return ((intLongestName > 6) ? intLongestName : 6);
        }

        public string FileName
        {
            get
            {
                return (m_strFilename);
            }
        }
    }
}
