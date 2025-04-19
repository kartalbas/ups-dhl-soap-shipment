using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ShipmentLib
{
    public class ShipmentException : Exception
    {
        public ShipmentException()
        {
        }

        public ShipmentException(Exception objException, string strMessage) : base(strMessage)
        {
            if (objException == null)
            {
                Logger.Instance.Log(TraceEventType.Critical, 9999, strMessage);
            }
            else
            {
                Logger.Instance.LogEx(TraceEventType.Critical, 9999, objException, strMessage);
            }


            Logger.Instance.Log(TraceEventType.Critical, 9999, "Appliation now exits with code -1!");
            strMessage = strMessage + "\n\n" + "Logfile: " + Logger.Instance.GetLogfile();
            MessageBox.Show(strMessage , "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (Properties.Settings.Default.OpenNotepadOnException.Equals("1"))
            {
                Process notePad = new Process();
                notePad.StartInfo.FileName = "notepad.exe";
                notePad.StartInfo.Arguments = Logger.Instance.GetLogfile();
                notePad.Start();
            }

            Environment.Exit(-1);
        }    
    }
}
