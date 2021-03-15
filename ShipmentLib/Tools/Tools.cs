using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;

namespace ShipmentLib
{
    public static class ShipmentTools
    {
        private static string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        private static string[] astrEUCodes = new string[]
        {
            "BE", "BG", "GR", "CZ", "DK", "DE", "EE", "IE", "EL", "ES", "FR", "HR", "IT", "CY", "LV", "LT", "LU", "HU", "MT", "NL", "AT", "PL", "PT", "RO", "SI", "SK", "FI", "SE", "UK", "GB"
        };

        private static string[] astrEUCandidatesCodes = new string[]
        {
            "IS", "LI", "NO", "CH"
        };

        private static string[] astrEUPotentialCandidatesCodes = new string[]
        {
            "AL", "MK", "TR", "ME", "RS"
        };


        public static DateTime ParseDate(string strDate, bool bEndOfDay)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            DateTime objDateTime = DateTime.Today;
            string strEndOfDay = bEndOfDay ? "23:59:59" : "00:00:00";

            if (strDate.Length > 0)
            {
                try
                {
                    objDateTime = DateTime.ParseExact(strDate + " " + strEndOfDay, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                }
                catch (Exception objException)
                {
                    new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Date has an invalid format!");
                }
            }

            return objDateTime;
        }

        public static int SafeParseInt(string strValue)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            int retValue = 0;

            if (!Int32.TryParse(strValue, out retValue))
            {
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": String to Integer not parseable!");
            }
            return retValue;
        }

        public static string ChangeUmlaute(string line)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (string.IsNullOrEmpty(line))
                return string.Empty;

            try
            {
                if (line.Contains("Ö"))
                    line = line.Replace("Ö", "OE");

                if (line.Contains("ö"))
                    line = line.Replace("ö", "oe");

                if (line.Contains("Ü"))
                    line = line.Replace("Ü", "UE");

                if (line.Contains("ü"))
                    line = line.Replace("ü", "ue");

                if (line.Contains("Ä"))
                    line = line.Replace("Ä", "AE");

                if (line.Contains("ä"))
                    line = line.Replace("ä", "ae");

                if (line.Contains("ß"))
                    line = line.Replace("ß", "ss");

                return line;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Not able to change Umlaute in line: " + line);
                return line;
            }
        }

        public static string MaxLength(this string strValue, int iMxLength)
        {
            return strValue?.Substring(0, System.Math.Min(strValue.Length, iMxLength));
        }

        public static bool IsEU(string strCode)
        {
            return Array.Exists(astrEUCodes, a => a == strCode);
        }

        public static bool IsEUCandidate(string strCode)
        {
            return Array.Exists(astrEUCandidatesCodes, a => a == strCode);
        }

        public static bool IsEUPotentialCandidate(string strCode)
        {
            return Array.Exists(astrEUPotentialCandidatesCodes, a => a == strCode);
        }

        public static PackageTypePointings GetShippingTypes(string strOffset, string strConfigFile)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            ExeConfigurationFileMap objMap = new ExeConfigurationFileMap();
            objMap.ExeConfigFilename = strConfigFile;
            Configuration objConfig = ConfigurationManager.OpenMappedExeConfiguration(objMap, ConfigurationUserLevel.None);
            AppSettingsSection objSection = (objConfig.GetSection("appSettings") as AppSettingsSection);

            try
            {
                PackageTypePointings objPtP = new PackageTypePointings();

                int iIndex = 1;
                string sValue = "";

                while (sValue != "EXIT")
                {
                    sValue = objSection.Settings[strOffset + iIndex.ToString()].Value;
                    string[] s = sValue.Split(new char[] { ';' });
                    if (s.Length == 3)
                    {
                        string[] foreignIds = s[2].Split(new char[] { ',' });

                        for (int j = 0; j < foreignIds.Length; j++)
                        {
                            PackageTypePointings p = new PackageTypePointings();
                            p.InternalId = s[0];
                            p.Naming = s[1];
                            p.ForeignId = SafeParseInt(foreignIds[j]);
                            objPtP.Add(p);
                        }
                    }

                    iIndex++;
                }

                return objPtP;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Init ShipmentTypePointing failed!s");
                return null;
            }

        }

        public static string IsValidEmail(string strEmail)
        {
            try
            {
                string strFirstMail = string.Empty;

                string[] astrMails = strEmail.Split(new char[] { ';' });


                if (astrMails.Length > 0)
                    strFirstMail = astrMails[0];

                var objMail = new System.Net.Mail.MailAddress(strFirstMail);

                if (objMail.Address == strFirstMail)
                    return strFirstMail;

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void Download(string strURLFileandPath, string strFileSaveFileandPath)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(strURLFileandPath);
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();

            using (Stream objStream = objResponse.GetResponseStream())
            {
                byte[] abyteBuffer = new byte[100000];
                int iBytesToRead = (int)abyteBuffer.Length;
                int iBytesRead = 0;
                while (iBytesToRead > 0)
                {
                    int iRead = objStream.Read(abyteBuffer, iBytesRead, iBytesToRead);
                    if (iRead == 0)
                        break;
                    iBytesRead += iRead;
                    iBytesToRead -= iRead;
                }

                using (FileStream objFileStream = new FileStream(strFileSaveFileandPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    objFileStream.Write(abyteBuffer, 0, iBytesRead);
                }

                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": File downloaded and saved as " + strFileSaveFileandPath);
            }
        }

        public static void DeleteFiles(string strPath, string strExtension)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                foreach (string strFile in Directory.GetFiles(strPath, strExtension))
                {
                    if(File.Exists(strFile))
                        File.Delete(strFile);
                }
            }
            catch (Exception objException)
            {
                Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Deleting files where not successfull!");
                Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": " + objException.Message);
                Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": " + objException.InnerException.ToString());
            }
        }
    }
}
