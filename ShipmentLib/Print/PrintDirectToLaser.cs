using System;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.Diagnostics;
using System.Reflection;

namespace ShipmentLib
{
    public class PrintDirectToLaser
    {
        private string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        // String data to be printed.
        static string _strPrintData = string.Empty;

        //Add the printer connection for specified pName.
        [DllImport("winspool.drv")]
        public static extern bool AddPrinterConnection(string pName);

        //Set the added printer as default printer.
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string Name);

        /// <summary>
        /// Sends the file to the printer choosed.
        /// </summary>
        /// <param name="strFileName">Name & path of the file to be printed.</param>
        /// <param name="strPrinterPath">The path of printer.</param>
        /// <param name="iNumCopies">The number of copies send to printer.</param>
        public bool PrintFile(string strFileName, string strPrinterPath, string strWorkingDir, int iNumCopies, Logger objLogger)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            // Check if the incomming strings are null or empty.
            if (string.IsNullOrEmpty(strFileName) || string.IsNullOrEmpty(strPrinterPath))
            {
                return false;
            }

            //Instantiate the object of ProcessStartInfo.
            try
            {
                Process objProcess = new Process();
                ProcessStartInfo objPsInfo = new ProcessStartInfo();

                // Set the printer.
                AddPrinterConnection(strPrinterPath);
                SetDefaultPrinter(strPrinterPath);

                objPsInfo.FileName = strFileName;
                objPsInfo.Verb = "print";
                objPsInfo.WindowStyle = ProcessWindowStyle.Hidden;
                objPsInfo.UseShellExecute = true;
                objPsInfo.CreateNoWindow = true;
                objPsInfo.WorkingDirectory = strWorkingDir;

                objProcess.StartInfo = objPsInfo;

                //Print the file with number of copies sent.
                for (int intCount = 0; intCount < iNumCopies; intCount++)
                {
                    objProcess.Start();
                    //objProcess.WaitForExit();
                    objProcess.WaitForInputIdle();
                }

                objProcess.Close();
                objProcess.Dispose();

                // Return true for success.
                return true;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": PrintFile " + strFileName);
                return false;
            }
        }

        /// <summary>
        /// Sends a string to the printer.
        /// </summary>
        /// <param name="strSendString">String to be printed.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public bool SendStringToPrinter(string strSendString)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                PrintDocument objDocument;
                string strPrintername;
                //Get the default printer name.
                objDocument = new PrintDocument();
                strPrintername = Convert.ToString(objDocument.PrinterSettings.PrinterName);

                if (string.IsNullOrEmpty(strPrintername))
                    throw new Exception("No default printer is set.Printing failed!");

                _strPrintData = strSendString;
                objDocument.PrintPage += new PrintPageEventHandler(PrintDoc_PrintPage);
                objDocument.Print();
                return true;
            }
            catch (COMException objComException)
            {
                new ShipmentException(objComException, _strAssembly + ":" + strMethod + ": SendStringToPrinter " + strSendString);
                return false;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": SendStringToPrinter " + strSendString);
                return false;
            }
        }

        /// <summary>
        /// Printes the page.
        /// </summary>
        /// <param name="sender">object : Sender event</param>
        /// <param name="e">PrintPageEventArgs</param>
        static void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            System.Drawing.Font objFont = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, 10);
            e.Graphics.DrawString(_strPrintData, objFont, System.Drawing.Brushes.Black, 0, 0);
        }
    }
}
