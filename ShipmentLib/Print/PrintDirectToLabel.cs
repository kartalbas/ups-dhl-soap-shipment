using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ShipmentLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DOCINFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDocName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pOutputFile;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDataType;
    }
    
    public class PrintDirectToLabel
    {
        private static string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        public static void PrintRaw(string targetprinter, string directtext, string docName, Logger logger)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            System.IntPtr lhPrinter = new System.IntPtr();

            DOCINFO di = new DOCINFO();
            int pcWritten = 0;

            di.pDocName = docName;
            di.pDataType = "RAW";

            PrintDirectToLabel.OpenPrinter(targetprinter, ref lhPrinter, 0);
            PrintDirectToLabel.StartDocPrinter(lhPrinter, 1, ref di);

            PrintDirectToLabel.StartPagePrinter(lhPrinter);
            try
            {
                PrintDirectToLabel.WritePrinter(lhPrinter, directtext, directtext.Length, ref pcWritten);
            }

            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": PrintRaw " + targetprinter + " " + directtext);
            }

            PrintDirectToLabel.EndPagePrinter(lhPrinter);
            PrintDirectToLabel.EndDocPrinter(lhPrinter);
            PrintDirectToLabel.ClosePrinter(lhPrinter);
        }

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern long OpenPrinter(string pPrinterName, ref IntPtr phPrinter, int pDefault);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern long StartDocPrinter(IntPtr hPrinter, int Level, ref DOCINFO pDocInfo);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern long StartPagePrinter(IntPtr hPrinter);
        [DllImport("winspool.drv", CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern long WritePrinter(IntPtr hPrinter, string data, int buf, ref int pcWritten);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern long EndPagePrinter(IntPtr hPrinter);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern long EndDocPrinter(IntPtr hPrinter);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern long ClosePrinter(IntPtr hPrinter);
    }
}
