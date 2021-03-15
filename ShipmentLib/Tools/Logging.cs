using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace ShipmentLib
{
    public class Logger
    {
        private static Logger _objInstance;

        private static TraceSource _objTracesource;
        private string _strLogFile;
        private string _strLogDir;
        private string _strFileName;
        private string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        public Logger()
        {
        }

        private void Init()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            //YYYY-MM-DD_HH-MM_UPSShipping.txt
            string strAssemblyName = "ShippingModule";
            string strVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            string strLogFile = DateTime.Now.ToString(CultureInfo.GetCultureInfo("de-DE")).Replace(".", "-").Replace(" ", "_").Replace(":", "-") + "_" + strAssemblyName + ".txt";
            Logger.Instance.Init(strLogFile);
            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Wegatrade Shipping Module");
            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Application Version " + strVersion);
        }


        public static Logger Instance
        {
            get
            {
                if (_objInstance == null)
                {
                    _objInstance = new Logger();
                    _objInstance.Init();
                }
                return _objInstance;
            }
        }

        public TraceSource Init(string strFilename)
        {
            _strFileName = strFilename;
            _objTracesource = new TraceSource(strFilename);
            _objTracesource.Switch = new SourceSwitch("sourceSwitch", "Verbose");
            _objTracesource.Listeners.Remove("Default");

            _strLogDir = SettingController.LogFolder;
            _strLogDir += @"\";

            if (!Directory.Exists(_strLogDir))
            {
                System.IO.Directory.CreateDirectory(_strLogDir);
            }

            TextWriterTraceListener textListener = new TextWriterTraceListener(_strLogDir + strFilename);
            _strLogFile = _strLogDir + strFilename;

            _objTracesource.Listeners.Add(textListener);
            return _objTracesource;
        }

        public void DisposeLogger()
        {
            _objTracesource.Flush();
            _objTracesource.Close();
            _objTracesource = null;
        }

        public TraceSource GetTracesource()
        {
            if (_objTracesource == null)
                Init(_strFileName);

            return _objTracesource;
        }

        public string GetLogfile()
        {
            return _strLogFile;
        }

        public string GetLogDir()
        {
            return _strLogDir;
        }

        public void LogEx(TraceEventType type, int eventID, Exception ex, string sMessage)
        {
            if (_objTracesource == null)
                Init(_strFileName);

            string _header = DateTime.Now.ToString() + ":";

            _objTracesource.TraceEvent(type, eventID, _header + sMessage);
            Console.WriteLine(_header + sMessage);

            if (ex.Message != null)
            {
                _objTracesource.TraceEvent(type, eventID, _header + ex.Message);
                Console.WriteLine(_header + ex.Message);
            }

            if (ex.InnerException != null)
            {
                _objTracesource.TraceEvent(type, eventID, _header + ex.InnerException);
                Console.WriteLine(_header + ex.InnerException);
            }

            if (ex.StackTrace != null)
            {
                _objTracesource.TraceEvent(type, eventID, _header + ex.StackTrace);
                Console.WriteLine(_header + ex.StackTrace);
            }

            if (ex.Source != null)
            {
                _objTracesource.TraceEvent(type, eventID, _header + ex.Source);
                Console.WriteLine(_header + ex.Source);
            }

            if (ex.TargetSite != null)
            {
                _objTracesource.TraceEvent(type, eventID, _header + ex.TargetSite.ToString());
                Console.WriteLine(_header + ex.TargetSite.ToString());
            }

            _objTracesource.Flush();
        }

        public void RenameLogfile(string strNewfilename)
        {
            string strOldlogfile = Logger.Instance.GetLogfile();
            Logger.Instance.DisposeLogger();
            if (File.Exists(strNewfilename))
                File.Delete(strNewfilename);
            File.Move(strOldlogfile, strNewfilename);
        }

        public void Log(TraceEventType objType, int iEventId, string strMessage)
        {
            if (_objTracesource == null)
                Init(_strFileName);

            string strHeader = DateTime.Now.ToString() + ":";
            _objTracesource.TraceEvent(objType, iEventId, strHeader + strMessage);
            Console.WriteLine(strHeader + strMessage);
            _objTracesource.Flush();
        }
    }
}
