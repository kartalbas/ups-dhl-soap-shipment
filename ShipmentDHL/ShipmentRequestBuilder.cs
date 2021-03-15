using ShipmentLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.IO;
using ShipmentDHL.ShipWebReference;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ShipmentLib.SoapDumper;

namespace ShipmentDHL
{
    public class ShipmentRequestBuilder : ITraceable
    {
        private Shipment _objShipment;
        private ShipWebReference.SWSServicePortTypeClient _objWebService;
        private ShipWebReference.AuthentificationType _objAuthentication;
        private DatabaseController _objDbController;
        private string _strCmd;
        private string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;
        private PrintListLabel _objLL = new PrintListLabel();

        private string componentName;
        private bool isTraceRequestEnabled;
        private bool isTraceResponseEnabled;

        public bool IsTraceRequestEnabled
        {
            get { return isTraceRequestEnabled; }
            set { isTraceRequestEnabled = value; }
        }

        public bool IsTraceResponseEnabled
        {
            get { return isTraceResponseEnabled; }
            set { isTraceResponseEnabled = value; }
        }

        public string ComponentName
        {
            get { return componentName; }
            set { componentName = value; }
        }

        public ShipmentRequestBuilder(string strNumber, string strCmd)
        {
            string strMethod = "ShipmentRequestBuilder";

            if (string.IsNullOrEmpty(strNumber))
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": strNumber is NULL!");

            if (string.IsNullOrEmpty(strCmd))
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": strCmd is NULL!");

            _strCmd = strCmd;
            _objShipment = new Shipment();

            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Command = " + strCmd);
            Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Executing Request in Test Mode? : " + SettingController.DHL_TestMode.ToString());

            InitRequest();
            _objDbController = new DatabaseController();

            if (strCmd.Equals("DELETE"))
            {
                _objShipment.Trackingnumber = strNumber;
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Trackingnumber " + strNumber);
            }
            else if (strCmd.Equals("SHIP"))
            {
                _objShipment.InvoiceNumber = strNumber;
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Belegnummer " + strNumber);

                if (!_objShipment.ExecutedByShipmentTests)
                    InitShipment();
            }
        }

        public ShipmentRequestBuilder(Shipment objShipment)
        {
            string strMethod = "ShipmentRequestBuilder";

            if (objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": objShipment is NULL!");

            _objShipment = objShipment;

            if (objShipment.InvoiceNumber.Length < 6 && !objShipment.InvoiceNumber.Equals("0"))
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": InvoiceNumber is lower than 5 characters!");

            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Belegnummer = " + objShipment.InvoiceNumber);

            _objDbController = new DatabaseController();

            InitRequest();

            if(!objShipment.ExecutedByShipmentTests)
                InitShipment();

        }

        #region Public methods

        public void DeleteOldShipmentDD()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            if (_objShipment.Trackingnumber.Length < 1)
                return;

            try
            {
                int iNumRetriesAllowed = ShipmentTools.SafeParseInt(SettingController.Retries);
                if (iNumRetriesAllowed < 2)
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Retry count is lower than 2!");
                    return;
                }

                for (int i = 0; i <= iNumRetriesAllowed; i++)
                {
                    if(_strCmd.Equals("SHIP"))
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Canceling Trackingnumber " + _objShipment.Trackingnumber);

                    if (DeleteShipmentDDRequest())
                        break;
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during executing CREATEDD request to DHL service!");
            }
        }

        public ShipWebReference.CreateShipmentResponse CreateNewShipmentDDRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                int iNumRetriesAllowed = ShipmentTools.SafeParseInt(SettingController.Retries);
                if (iNumRetriesAllowed < 2)
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Retry count is lower than 2!");
                    return null;
                }

                ShipWebReference.CreateShipmentDDRequest objRequest = CreateShipmentDDRequest();
                ShipWebReference.CreateShipmentResponse objResponse = null;

                for (int i = 0; i <= iNumRetriesAllowed; i++)
                {
                    objResponse = _objWebService.createShipmentDD(_objAuthentication, objRequest);

                    if (objResponse != null && objResponse.status.StatusCode.Equals("0"))
                    {
                        _objShipment.Trackingnumber = objResponse.CreationState[0].ShipmentNumber.Item;

                        for(int iUrl=0; iUrl < objResponse.CreationState.Length; iUrl++)
                        {
                            string strLabelUrl = objResponse.CreationState[iUrl].Labelurl;
                            string strFile = Path.Combine(SettingController.DownloadFolder, iUrl.ToString() + "_" + _objShipment.Trackingnumber + "_" + SettingController.DHL_PDF_Filename);
                            ShipmentTools.Download(strLabelUrl, strFile);
                            _objShipment.DownloadedFiles.Add(strFile);
                        }

                        GetExportDocumentDD();

                        if (!_objShipment.ExecutedByShipmentTests)
                        {
                            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Printing labels ...");
                            PrintLabels();
                            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Saving Trackingnumber ...");
                            SaveTrackingnumbers();
                        }
                        return objResponse;
                    }
                    else
                    {

                        Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);

                        if (_strCmd.Equals("SHIP")) DeleteOldShipmentDD();
                        new ShipmentException(null, ReadableException(objResponse, _strAssembly));
                    }
                }

                if(objResponse != null)
                    Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);
                else
                    Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : objResponse = null" );

                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Create Shipping with Retry FAILED!!");
                return null;
            }
            catch (Exception objException)
            {
                if (_strCmd.Equals("SHIP")) DeleteOldShipmentDD();
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during executing CREATEDD request to DHL service!");
                return null;
            }
        }

        public void DoManifestTodayDD()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                int iNumRetriesAllowed = ShipmentTools.SafeParseInt(SettingController.Retries);
                if (iNumRetriesAllowed < 2)
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Retry count is lower than 2!");
                    return;
                }

                for (int i = 0; i <= iNumRetriesAllowed; i++)
                {
                    if (DoAndPrintManifestDD(0))
                    {
                        string strNewFilename = Logger.Instance.GetLogDir();
                        strNewFilename += "DOMANIFEST_" + DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".txt";
                        Logger.Instance.RenameLogfile(strNewFilename);
                        break;
                    }
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during manifesting for today ");
            }

        }

        public void GetManifestDD(string strDateBegin, string strDateEnd)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (strDateBegin.Length < 8)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Invalid date format for date " + strDateBegin);

            if (strDateEnd.Length < 8)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Invalid date format for date " + strDateEnd);

            DateTime objDateBegin = default(DateTime);
            DateTime objDateEnd = default(DateTime);

            if (strDateBegin.Equals(strDateEnd))
            {
                objDateBegin = ShipmentTools.ParseDate(strDateBegin, false);
            }
            else
            {
                objDateBegin = ShipmentTools.ParseDate(strDateBegin, false);
                objDateEnd = ShipmentTools.ParseDate(strDateEnd, true);
            }

            try
            {
                int iNumRetriesAllowed = ShipmentTools.SafeParseInt(SettingController.Retries);
                if (iNumRetriesAllowed < 2)
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Retry count is lower than 2!");
                    return;
                }

                for (int i = 0; i <= iNumRetriesAllowed; i++)
                {
                    if (GetAndPrintManifestDD(objDateBegin, objDateEnd))
                    {
                        string strNewFilename = Logger.Instance.GetLogDir();
                        strNewFilename += "GETMANIFEST_" + strDateBegin.Replace(".","-") + "_" + strDateEnd.Replace(".","-") + ".txt";
                        Logger.Instance.RenameLogfile(strNewFilename);
                        break;
                    }
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data");
            }
        }

        public void PrintSummary(string strDateBegin, string strDateEnd)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                DateTime objDateBegin = ShipmentTools.ParseDate(strDateBegin, false);
                DateTime objDateEnd = ShipmentTools.ParseDate(strDateEnd, true);

                var cobjOrders = _objDbController.GetShippedOrders(objDateBegin, objDateEnd, "DHL");
                if (cobjOrders == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Error during printing summary! No orders found which are shipped with UPS!");

                ShipmentSummaryDetail objShipmentSummaryDetail = new ShipmentSummaryDetail();
                for (int iIndexOrder = 0; iIndexOrder < cobjOrders.Count; iIndexOrder++)
                {
                    var objOrder = cobjOrders[iIndexOrder];
                    string[] astrTrackingNumbers = objOrder.Trackingnumbers.Split(new char[] { ',' });

                    var objLieferadress = _objDbController.GetLieferadressen(objOrder.LieferadressenID);
                    if (objLieferadress == null)
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Error during printing summary! Lieferadress could not be found!");

                    for (int iIndexTrackingnumber = 0; iIndexTrackingnumber < astrTrackingNumbers.Length; iIndexTrackingnumber++)
                    {
                        objShipmentSummaryDetail.Add(new ShipmentSummaryDetail()
                        {
                            AddressNumber = objOrder.Kundennummer,
                            Name = objLieferadress.Name1,
                            Street = objLieferadress.Strasse + " " + objLieferadress.HausNr,
                            Zip = objLieferadress.Plz,
                            City = objLieferadress.Ort,
                            Country = objLieferadress.Land,
                            Versandart = objOrder.Versandart,
                            Weigth = (double)objOrder.Gesamtgewicht,
                            Trackingnumber = astrTrackingNumbers[iIndexTrackingnumber]
                        });
                    }
                }

                //Create the shipper informations
                ShipmentSummary objShipmentSummary = ShipmentSummary.CreateDefault();
                objShipmentSummary.ShipmentSummaryDetails = objShipmentSummaryDetail.GetItems();
                objShipmentSummary.DateBegin = objDateBegin;
                objShipmentSummary.DateEnd = objDateEnd;

                //print
                bool bDesign = SettingController.LLDesign.Equals("1") ? true : false;
                bool bPreview = SettingController.LLPreview.Equals("1") ? true : false;
                _objLL.DesignLLSummary(SettingController.FileLLSummaryDHL, bDesign, bPreview, objShipmentSummary);
                _objLL.DesignLLSummary(SettingController.FileLLListDHL, bDesign, bPreview, objShipmentSummary);

            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during printing summary!");
            }
        }

        #endregion

        #region Private methods

        private bool DoAndPrintManifestDD(int iDay)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var cobjBelege = _objDbController.GetShippedOrders(DateTime.Today.AddDays(iDay), DateTime.Today.AddDays(iDay), "DHL");
            if (cobjBelege != null && cobjBelege.Count <= 0)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": No Orders found to manifest for today!");

            List<string> cstrTrackingnumbers = cobjBelege.Select(a => a.Trackingnumbers).ToList<string>();

            if (cstrTrackingnumbers == null || cstrTrackingnumbers.Count == 0)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": No Trackingnumbers are given!");

            ShipWebReference.DoManifestDDRequest objRequest = DoManifestDDRequest(cstrTrackingnumbers.ToArray());

            try
            {
                ShipWebReference.DoManifestResponse objResponse = _objWebService.doManifestDD(_objAuthentication, objRequest);

                if (objResponse != null && objResponse.Status.StatusCode.Equals("0"))
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Manifested " + objResponse.ManifestState.Length + " of " + cstrTrackingnumbers.Count);

                    List<ManifestedDetail> cobjManifests = new List<ManifestedDetail>();

                    foreach (var objState in objResponse.ManifestState)
                    {
                        if(objState.Status.Equals("0"))
                            Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": OK    Trackingnumber " + objState.ShipmentNumber.Item + " manifested!");
                        else
                            Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": ERROR Trackingnumber " + objState.ShipmentNumber.Item + " > " + objState.Status.StatusCode + "/" + objState.Status.StatusMessage);

                        cobjManifests.Add(new ManifestedDetail(objState.ShipmentNumber.Item, objState.Status.StatusCode, objState.Status.StatusMessage));
                    }

                    //print

                    //Create the shipper informations
                    ShipmentSummary objShipmentSummary = ShipmentSummary.CreateDefault();
                    objShipmentSummary.ManifestedDetails = cobjManifests;
                    objShipmentSummary.DateBegin = DateTime.Today;
                    objShipmentSummary.DateEnd = DateTime.Today;

                    bool bDesign = SettingController.LLDesign.Equals("1") ? true : false;
                    bool bPreview = SettingController.LLPreview.Equals("1") ? true : false;
                    _objLL.DesignLLSummary(SettingController.FileLLManifestedDHL, bDesign, bPreview, objShipmentSummary);
                }
                else
                {
                    if (objResponse == null)
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during Manifest Data with objResponse = null!");
                    else
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during Manifest Data with Code/Error " + objResponse.Status.StatusCode + "/" + objResponse.Status.StatusMessage);
                }

                return true;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during manifesting!");
                return false;
            }
        }

        private bool GetAndPrintManifestDD(DateTime objDateBegin, DateTime objDateEnd)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            ShipWebReference.GetManifestDDRequest objRequest;

            if (objDateEnd == default(DateTime))
                objRequest = GetManifestDDRequest(objDateBegin);
            else
                objRequest = GetManifestDDRequest(objDateBegin, objDateEnd);

            try
            {
                ShipWebReference.GetManifestDDResponse objResponse = _objWebService.getManifestDD(_objAuthentication, objRequest);

                if (objResponse != null && objResponse.status.StatusCode.Equals("0"))
                {
                    if (objResponse.ManifestPDFData.Length > 0)
                    {
                        string strExportDocPDFData = objResponse.ManifestPDFData;
                        var abyteExportDocPDFData = Convert.FromBase64String(strExportDocPDFData);
                        string strFile = Path.Combine(SettingController.DownloadFolder, SettingController.DHL_Manifest_Filename);
                        File.WriteAllBytes(strFile, abyteExportDocPDFData);
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Manifest Data downloaded and saved as " + strFile);

                        //print
                        bool bDesign = SettingController.LLDesign.Equals("1") ? true : false;
                        bool bPreview = SettingController.LLPreview.Equals("1") ? true : false;
                        _objLL.DesignLLHtml(SettingController.FileLLExportDocumentPdf, strFile, bDesign, bPreview);
                        return true;
                    }
                    else
                    {
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data with Code/Error " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);
                    }
                }
                else
                {
                    if(objResponse == null)
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data with objResponse = null!");
                    else
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data with Code/Error " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);
                }

                return false;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during downloading Export Document!");
                return false;
            }
        }

        private void GetExportDocumentDD()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (ShipmentTools.IsEU(_objShipment.ReceiverCountryCode))
                return;

            ShipWebReference.GetExportDocDDRequest objRequest = GetExportDocDDRequest();

            try
            {
                ShipWebReference.GetExportDocResponse objResponse = _objWebService.getExportDocDD(_objAuthentication, objRequest);

                if (objResponse != null && objResponse.status.StatusCode.Equals("0"))
                {
                    if (objResponse.ExportDocData.Length > 0)
                    {
                        string strExportDocPDFData = objResponse.ExportDocData[0].Item;  //ExportDocPDFData
                        var abyteExportDocPDFData = Convert.FromBase64String(strExportDocPDFData);
                        _objShipment.ExportDocumentDownloadedFile = Path.Combine(SettingController.DownloadFolder, _objShipment.Trackingnumber + "_" + SettingController.DHL_ExportDocument_Filename);
                        File.WriteAllBytes(_objShipment.ExportDocumentDownloadedFile, abyteExportDocPDFData);
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Export Documents downloaded and saved as " + _objShipment.ExportDocumentDownloadedFile);
                    }
                    else
                    {
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Export Document with Code/Error " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);
                    }
                }
                else
                {
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Download of Export Document Fail with Error: " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);

                    if (objResponse.ExportDocData.Length > 0)
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Download of Export Document Fail with inner Error " + objResponse.ExportDocData[0].Status.StatusCode + "/" + objResponse.ExportDocData[0].Status.StatusMessage);

                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Export Document with Code/Error " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during downloading Export Document!");
            }
        }

        private bool DeleteShipmentDDRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            ShipWebReference.DeleteShipmentDDRequest objRequest = GetDeleteShipmentDDRequest(_objShipment.Trackingnumber);

            try
            {
                ShipWebReference.DeleteShipmentResponse objResponse = _objWebService.deleteShipmentDD(_objAuthentication, objRequest);

                if(objResponse == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Delete request failed for trackingnumber " + _objShipment.Trackingnumber + " because objResponse = null!");

                ShipWebReference.Statusinformation status = objResponse.Status;
                string statusMessage = status.StatusMessage;
                ShipWebReference.DeletionState[] aobjStates = objResponse.DeletionState;

                foreach (ShipWebReference.DeletionState objState in aobjStates)
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Status of Trackingnumber was " + objState.Status.StatusCode + "/" + objState.Status.StatusMessage);

                    if(objState.Status.StatusCode.Equals("0"))
                        RemoveTrackingnumber();
                }

                return true;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during executing DELETEDD request to DHL service!");
                return false;
            }
        }

        private void RemoveTrackingnumber()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                var objOrder = _objDbController.RemoveTrackingnumber(_objShipment.Trackingnumber);

                if (objOrder == null)
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Trackingnumber " + _objShipment.Trackingnumber + " not found in database!");

                if(_strCmd.Equals("DELETE"))
                {
                    string strNewFilename = Logger.Instance.GetLogDir();
                    strNewFilename += "DELETE_" + _objShipment.Trackingnumber + ".txt";
                    Logger.Instance.RenameLogfile(strNewFilename);
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during removing trackingnumbers!");
            }
        }

        private void SaveTrackingnumbers()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                var objOrder = _objDbController.SaveTrackingnumbers(_objShipment.Trackingnumber, _objShipment.InvoiceNumber);

                if (objOrder == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Saving trackingnumber " + _objShipment.Trackingnumber + " Failed!");

                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": " + objOrder.trackingnumbers.Replace(", ", "-") + " has been saved to database");

                string[] astrTNR = _objShipment.Trackingnumber.Split(new char[] { ',' });

                if (astrTNR.Length > 0)
                {
                    string strNewFilename = Logger.Instance.GetLogDir();
                    strNewFilename += "SHIP_" + objOrder.trackingnumbers.Replace(",", "-") + "-" + astrTNR.Length + "_" + objOrder.Belegnummer + "_" + objOrder.Kundennummer + ".txt";
                    Logger.Instance.RenameLogfile(strNewFilename);
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during saving trackingnumbers!");
            }
        }

        public static string ReadableException(ShipWebReference.CreateShipmentResponse objResult, string strMethod)
        {
            string strDhlException = "ERROR: " + objResult.status.StatusCode + " \n";

            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}: {1}\n", strMethod, strLine);
                }
            }

            return strDhlException;
        }

        public Shipment InitShipment()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            try
            {
                CreateShipment();
                CreateReceiver();
                CreateExportDocuments();

                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Ship request initialized successfull");

                return _objShipment;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during ship request initialisation!");
                return null;
            }
        }

        private void CreateShipment()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                //Get Order Data
                if (_objShipment == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

                int iOrderNr = ShipmentTools.SafeParseInt(_objShipment.InvoiceNumber);
                if (iOrderNr == 0)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Ordernumber must be greater than 0");

                _objShipment.Order = _objDbController.GetOrder(iOrderNr);
                if (_objShipment.Order == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": No order found for Ordernumber=" + iOrderNr.ToString());

                _objShipment.Lieferaddress = _objDbController.GetLieferadressen(_objShipment.Order.LieferadressenID);

                _objShipment.Address = _objDbController.GetAdress(_objShipment.Order.AdressenID);
                if (_objShipment.Address == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": No Address found for addressenId=" + _objShipment.Order.AdressenID.ToString());

                _objShipment.OrderCountryCode = _objDbController.GetCountryCode(_objShipment.Order.LieferLand).MaxLength(2);
                if (_objShipment.OrderCountryCode.Equals("DE"))
                    _objShipment.DDProdCode = _objShipment.DDProdCode_EPN_EUROPACK_NATIONAL;
                else
                    _objShipment.DDProdCode = _objShipment.DDProdCode_EPI_EUROPAK_INTERNATIONAL;

                if (_objShipment.Order.PaketAnzahl > 0)
                    _objShipment.PackageCount = (int)_objShipment.Order.PaketAnzahl;

                if (_objShipment.Order.ZahlungsartID == (int)Paymenttypes.Bar)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": This shipment must be payed in Cash/Bar!");

                for (int i = 0; i < _objShipment.PackageCount; i++)
                {
                    var objPackage = new Shipment.Package(_objShipment.Order.Gesamtgewicht, SettingController.Paket_Length, SettingController.Paket_Width, SettingController.Paket_Height, "PK");
                    _objShipment.Packages.Add(objPackage);
                }

                //string strConfigFile = _strAssembly + ".dll.config";
                //var objPtP = ShipmentTools.GetShippingTypes("DHL", strConfigFile);
                //var objPackageType = objPtP.Find((int)_objShipment.Order.VersandartenID);

                ////Assign DHL ProductCode
                //if (objPackageType == null)
                //{
                //    new ShipmentException(null, _strAssembly + ":" + strMethod + ": DHL ID not found in config file " + strConfigFile + "!");
                //}
                //else if (objPackageType.InternalId.Length == 3)
                //{
                //    _objShipment.DDProdCode = objPackageType.InternalId;
                //    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": ProductCode is " + _objShipment.DDProdCode);
                //}
                //else
                //{
                //    new ShipmentException(null, _strAssembly + ":" + strMethod + ": DHL ProductCode not found in config file " + strConfigFile + "!");
                //}

                if (_objShipment.Order.ZahlungsartID == (int)Paymenttypes.Nachnahme)
                {
                    if(_objShipment.DDProdCode == _objShipment.DDProdCode_EPI_EUROPAK_INTERNATIONAL)
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": COD with DHL ProductCode = EPI not possible!");

                    _objShipment.Order.Buchungsbetrag = _objShipment.Order.Buchungsbetrag + SettingController.PauschalBetrag;
                    _objShipment.CODCurrency = "EUR";
                    _objShipment.CODAmount = (decimal)_objShipment.Order.Buchungsbetrag;
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": COD Amount is " + _objShipment.CODAmount.ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Shipment could not be created!");
            }

        }

        private void CreateReceiver()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            _objShipment.ReceiverCompanyName = ShipmentTools.ChangeUmlaute(_objShipment.Lieferaddress.Name1).MaxLength(30);
            _objShipment.ReceiverFirstName = ShipmentTools.ChangeUmlaute(_objShipment.Lieferaddress.Name2).MaxLength(30);
            _objShipment.ReceiverLastName = string.Empty;

            _objShipment.ReceiverContactName = ShipmentTools.ChangeUmlaute(_objShipment.Lieferaddress.Name2).MaxLength(30);

            if (_objShipment.ReceiverContactName.Length == 0)
                _objShipment.ReceiverContactName = _objShipment.ReceiverCompanyName;

            _objShipment.ReceiverStreet = ShipmentTools.ChangeUmlaute(_objShipment.Lieferaddress.Strasse).MaxLength(30);
            _objShipment.ReceiverStreentNr = _objShipment.Lieferaddress.HausNr;
            _objShipment.ReceiverZip = ShipmentTools.ChangeUmlaute(_objShipment.Lieferaddress.Plz).MaxLength(10);
            _objShipment.ReceiverCity = ShipmentTools.ChangeUmlaute(_objShipment.Lieferaddress.Ort).MaxLength(20);
            _objShipment.ReceiverCountryCode = _objShipment.OrderCountryCode;
            _objShipment.ReceiverCountry = string.Empty;

            string strMail = ShipmentTools.IsValidEmail(_objShipment.Address.eMail);

            if (!strMail.Equals(string.Empty))
                _objShipment.ReceiverContactEMail = strMail.MaxLength(50);
            else
                _objShipment.ReceiverContactEMail = SettingController.ShipperEmail;

            if (_objShipment.Address.Telefon1.Length > 3)
                _objShipment.ReceiverContactPhone = _objShipment.Address.Telefon1.MaxLength(20);
            else if (_objShipment.Address.Mobil.Length > 3)
                _objShipment.ReceiverContactPhone = _objShipment.Address.Mobil.MaxLength(20);
            else
                _objShipment.ReceiverContactPhone = SettingController.ShipperPhoneNumber.MaxLength(20);
        }

        private void CreateExportDocuments()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            if (ShipmentTools.IsEU(_objShipment.ReceiverCountryCode))
            {
                _objShipment.IncoTerm = _objShipment.IncoTerm_CIP_Frachtfrei_Versichert;
                return;
            }
            else
            {
                _objShipment.IncoTerm = _objShipment.IncoTerm_DAP_Geliefert_Benannter_Ort;
            }

            _objShipment.Orderdetails = _objDbController.GetOrderdetails((int)_objShipment.Order.Belegnummer);

            _objShipment.ExportDocumentInvoiceNumber = _objShipment.InvoiceNumber.MaxLength(30);
            _objShipment.ExportDocumentInvoiceDate = DateTime.Today.ToString(_objShipment.SDF).MaxLength(10);
            _objShipment.ExportDocumentCommodityCode = "".MaxLength(30);
            _objShipment.ExportDocumentTermsOfTrade = _objShipment.IncoTerm.MaxLength(3);

            if(_objShipment.DDProdCode.Equals(_objShipment.DDProdCode_BPI_BUSINESS_PAKET_INTERNATIONAL))
                _objShipment.ExportDocumentAmount = _objShipment.Orderdetails.Count.ToString().MaxLength(22);
            else
                _objShipment.ExportDocumentAmount = "1".ToString().MaxLength(22);

            _objShipment.ExportDocumentDescription = "Commodity";
            _objShipment.ExportDocumentCountryCodeOrigin = SettingController.ShipperCountryCode.MaxLength(2);
            _objShipment.ExportDocumentAdditionalFee = 0.0M;
            _objShipment.ExportDocumentCustomsValue = (decimal)_objShipment.Order.Buchungsbetrag == 0 ? (decimal)_objShipment.Order.Gesamtbetrag : (decimal)_objShipment.Order.Buchungsbetrag;
            _objShipment.ExportDocumentCustomsCurrency = _objShipment.Order.Waehrung;
            _objShipment.ExportDocumentPermitNumber = "".MaxLength(30);
            _objShipment.ExportDocumentAttestationNumber = "".MaxLength(30);
            _objShipment.ExportDocumentExportTypeDescription = "".MaxLength(30);
            _objShipment.ExportDocumentMRNNumber = "".MaxLength(9);

            foreach (var objPos in _objShipment.Orderdetails)
            {
                _objShipment.ItemPositions.Add(new Shipment.ItemPosition()
                {
                    Amount = objPos.Anzahl.ToString().MaxLength(22),
                    CommodityCode = objPos.Warennummer == null ? string.Empty : objPos.Warennummer.MaxLength(30),
                    CountryCodeOrigin = SettingController.ShipperCountryCode.MaxLength(3),
                    CustomsCurrency = _objShipment.ExportDocumentCustomsCurrency.MaxLength(3),
                    CustomsValue = (decimal)objPos.EndpreisNetto,
                    Description = objPos.Artikeltext1.MaxLength(40),
                    GrossWeightInKG = 1.0M,
                    NetWeightInKG = objPos.Gewicht == null ? 0.0M : (decimal)objPos.Gewicht
                });
            }
        }

        private void PrintLabels()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            try
            {
                //print the first page to laser printer
                bool bDesign = SettingController.LLDesign.Equals("1") ? true : false;
                bool bPreview = SettingController.LLPreview.Equals("1") ? true : false;

                if (File.Exists(SettingController.FileLLPdf))
                {
                    foreach(string strFile in _objShipment.DownloadedFiles)
                        _objLL.DesignLLHtml(SettingController.FileLLPdf, strFile, bDesign, bPreview);
                }
                else
                {
                    if (_strCmd.Equals("SHIP")) DeleteOldShipmentDD();
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": List and Label file " + SettingController.FileLLPdf + " not found!");
                }

                if (File.Exists(_objShipment.ExportDocumentDownloadedFile))
                {
                    if (File.Exists(SettingController.FileLLExportDocumentPdf))
                    {
                        _objLL.DesignLLHtml(SettingController.FileLLExportDocumentPdf, _objShipment.ExportDocumentDownloadedFile, bDesign, bPreview);
                    }
                    else
                    {
                        if (_strCmd.Equals("SHIP")) DeleteOldShipmentDD();
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": List and Label file " + SettingController.FileLLExportDocumentPdf + " not found!");
                    }
                }

            }
            catch (Exception objException)
            {
                if (_strCmd.Equals("SHIP")) DeleteOldShipmentDD();
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Printing Error!");
            }
        }

        private void InitRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            _objWebService = new ShipWebReference.SWSServicePortTypeClient("ShipmentServiceSOAP11port0");
            _objWebService.Endpoint.Address = new System.ServiceModel.EndpointAddress(_objShipment.Endpoint);
            System.ServiceModel.BasicHttpBinding objBinding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport);
            objBinding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic;
            objBinding.MaxReceivedMessageSize = 10*1024*1024;
            _objWebService.Endpoint.Binding = objBinding;

            _objAuthentication = new ShipWebReference.AuthentificationType();
            _objAuthentication.type = "0";


            if (SettingController.DHL_TestMode)
            {
                _objWebService.ClientCredentials.UserName.UserName = SettingController.DHL_CigUser;
                _objWebService.ClientCredentials.UserName.Password = SettingController.DHL_CigPassword;
                _objAuthentication.user = SettingController.DHL_IntraShipUser;
                _objAuthentication.signature = SettingController.DHL_IntraShipPassword;
            }
            else
            {
                _objWebService.ClientCredentials.UserName.UserName = SettingController.DHL_ApplicationsID;
                _objWebService.ClientCredentials.UserName.Password = SettingController.DHL_Applicationstoken;
                _objAuthentication.user = SettingController.DHL_Username;
                _objAuthentication.signature = SettingController.DHL_CigPassword;
            }

        }

        private ShipWebReference.CreateShipmentDDRequest CreateShipmentDDRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.ShipmentOrderDDTypeShipment objShipment = new ShipWebReference.ShipmentOrderDDTypeShipment();
            objShipment.ShipmentDetails = CreateShipmentDetailsDDType(_objShipment.PackageCount);

            ShipWebReference.ShipperDDType objShipper = new ShipWebReference.ShipperDDType();
            objShipper.Company = CreateShipperCompany();
            objShipper.Address = CreateShipperNativeAddressType();
            objShipper.Communication = CreateShipperCommunicationType();
            objShipment.Shipper = objShipper;

            ShipWebReference.ReceiverDDType objReceiver = new ShipWebReference.ReceiverDDType();
            objReceiver.Company = CreateReceiverCompany();
            objReceiver.Item = CreateReceiverNativeAddressType();
            objReceiver.Communication = CreateReceiverCommunicationType();
            objShipment.Receiver = objReceiver;

            if(((ShipWebReference.NativeAddressType)objReceiver.Item).Zip.ItemElementName == ShipWebReference.ItemChoiceType6.other)
            {
                var objResult = CreateExportDocDDType();
                if(objResult != null)
                    objShipment.ExportDocument = objResult;
            }

            ShipWebReference.ShipmentOrderDDType objOrder = new ShipWebReference.ShipmentOrderDDType();
            objOrder.SequenceNumber = "1";
            objOrder.LabelResponseType = ShipWebReference.ShipmentOrderDDTypeLabelResponseType.URL;
            objOrder.Shipment = objShipment;

            ShipWebReference.ShipmentOrderDDType[] aOrder = new ShipWebReference.ShipmentOrderDDType[] { objOrder };

            ShipWebReference.CreateShipmentDDRequest objRequest = new ShipWebReference.CreateShipmentDDRequest();
            objRequest.ShipmentOrder = aOrder;
            objRequest.Version = CreateVersion();
            return objRequest;
        }

        private ShipWebReference.NameType CreateShipperCompany()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.NameTypeCompany company = new ShipWebReference.NameTypeCompany();
            company.name1 = _objShipment.ShipperCompanyName;

            ShipWebReference.NameType name = new ShipWebReference.NameType();
            name.Item = company;
            return name;
        }

        private ShipWebReference.NativeAddressType CreateShipperNativeAddressType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.NativeAddressType objAddress = new ShipWebReference.NativeAddressType();
            objAddress.streetName = _objShipment.ShipperStreet;
            objAddress.streetNumber = _objShipment.ShipperStreetNr;
            objAddress.city = _objShipment.ShipperCity;

            ShipWebReference.ZipType objZip = new ShipWebReference.ZipType();
            objZip.ItemElementName = ShipWebReference.ItemChoiceType6.germany;
            objZip.Item = _objShipment.ShipperZip;
            objAddress.Zip = objZip;

            ShipWebReference.CountryType objOrigin = new ShipWebReference.CountryType();
            objOrigin.countryISOCode = _objShipment.ShipperCountryCode;
            objAddress.Origin = objOrigin;

            return objAddress;
        }

        private ShipWebReference.NativeAddressType CreateReceiverNativeAddressType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.NativeAddressType address = new ShipWebReference.NativeAddressType();
            ShipWebReference.ZipType zip = new ShipWebReference.ZipType();
            ShipWebReference.CountryType origin = new ShipWebReference.CountryType();

            address.streetName = _objShipment.ReceiverStreet;
            address.streetNumber = _objShipment.ReceiverStreentNr;
            address.city = _objShipment.ReceiverCity;

            if (_objShipment.ReceiverCountryCode.Equals("DE"))
                zip.ItemElementName = ShipWebReference.ItemChoiceType6.germany;
            else if (_objShipment.ReceiverCountryCode.Equals("GB"))
                zip.ItemElementName = ShipWebReference.ItemChoiceType6.england;

            else
                zip.ItemElementName = ShipWebReference.ItemChoiceType6.other;

            zip.Item = _objShipment.ReceiverZip;
            //origin.country = _objShipment.ReceiverCountry;
            origin.countryISOCode = _objShipment.ReceiverCountryCode;
            address.Zip = zip;
            address.Origin = origin;

            return address;
        }

        private ShipWebReference.CommunicationType CreateShipperCommunicationType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.CommunicationType objCommunication = new ShipWebReference.CommunicationType();
            objCommunication.email = _objShipment.ShipperEMail;
            objCommunication.contactPerson = _objShipment.ShipperName;
            objCommunication.phone = _objShipment.ShipperPhone;
            return objCommunication;
        }
        
        private ShipWebReference.CommunicationType CreateReceiverCommunicationType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.CommunicationType objCommunication = new ShipWebReference.CommunicationType();
            objCommunication.email = _objShipment.ReceiverContactEMail;
            objCommunication.contactPerson = _objShipment.ReceiverContactName;
            objCommunication.phone = _objShipment.ReceiverContactPhone;
            return objCommunication;
        }
        
        private ShipWebReference.NameType CreateReceiverCompany()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.NameType objName = new ShipWebReference.NameType();
            ShipWebReference.NameTypeCompany objCompany = new ShipWebReference.NameTypeCompany();
            objCompany.name1 = _objShipment.ReceiverCompanyName;
            objCompany.name2 = string.Format("{0} {1}", _objShipment.ReceiverFirstName, _objShipment.ReceiverLastName);
            objName.Item = objCompany;
            return objName;
        }
        
        private ShipWebReference.ShipmentDetailsDDType CreateShipmentDetailsDDType(int iCountOfPackages)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.ShipmentDetailsDDType objShipmentDetails = new ShipWebReference.ShipmentDetailsDDType();
            objShipmentDetails.ProductCode = _objShipment.DDProdCode;
            objShipmentDetails.EKP = _objShipment.EKP;
            objShipmentDetails.ShipmentDate = DateTime.Today.ToString(_objShipment.SDF).MaxLength(10);

            if (_objShipment.CODAmount > 0)
            {
                objShipmentDetails.BankData = CreateShipperBankData();
                objShipmentDetails.DeclaredValueOfGoods = (float)_objShipment.CODAmount;
                objShipmentDetails.DeclaredValueOfGoodsCurrency = _objShipment.CODCurrency;
            }

            ShipWebReference.ShipmentDetailsDDTypeAttendance objAttendance = new ShipWebReference.ShipmentDetailsDDTypeAttendance();
            objAttendance.partnerID = _objShipment.GetPartnerCode();

            if (objAttendance.partnerID.Equals(string.Empty))
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Shipment could not be created! PartnerID is empty!");

            objShipmentDetails.Attendance = objAttendance;

            List<ShipWebReference.ShipmentServiceDD> cobjShipmentServices = new List<ShipWebReference.ShipmentServiceDD>();

            //COD Service
            if (_objShipment.CODAmount > 0)
            {
                ShipWebReference.DDServiceGroupOtherTypeCOD objCOD = new ShipWebReference.DDServiceGroupOtherTypeCOD();
                objCOD.CODAmount = _objShipment.CODAmount;
                objCOD.CODCurrency = _objShipment.CODCurrency;

                ShipWebReference.DDServiceGroupOtherType objServiceOther = new ShipWebReference.DDServiceGroupOtherType();
                objServiceOther.ItemElementName = ShipWebReference.ItemChoiceType5.COD;
                objServiceOther.Item = objCOD;

                ShipWebReference.ShipmentServiceDD objService = new ShipWebReference.ShipmentServiceDD();
                objService.Item = objServiceOther;

                cobjShipmentServices.Add(objService);
            }

            //Multipack Service
            if(iCountOfPackages > 1)
            {
                ShipWebReference.DDServiceGroupDHLPaketType objServiceDHL = new ShipWebReference.DDServiceGroupDHLPaketType();
                objServiceDHL.ItemElementName = ShipWebReference.ItemChoiceType4.Multipack;
                objServiceDHL.Item = true;

                ShipWebReference.ShipmentServiceDD objService = new ShipWebReference.ShipmentServiceDD();
                objService.Item = objServiceDHL;

                cobjShipmentServices.Add(objService);
            }

            if (cobjShipmentServices.Count > 0)
                objShipmentDetails.Service = cobjShipmentServices.ToArray();

            objShipmentDetails.ShipmentItem = new ShipWebReference.ShipmentItemDDType[iCountOfPackages];

            for (int i = 0; i < iCountOfPackages; i++)
            {
                objShipmentDetails.ShipmentItem[i] = CreateShipmentItemDDType(i);
            }
            objShipmentDetails.Description = _objShipment.ShipmentDesc;

            return objShipmentDetails;
        }

        private ShipWebReference.BankType CreateShipperBankData()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.BankType objBankData = new ShipWebReference.BankType()
            {
                accountOwner = SettingController.ShipperAccountOwner,
                accountNumber = SettingController.ShipperAccountNumber,
                bankCode = SettingController.ShipperBankCode,
                bankName = SettingController.ShipperBankName,
                iban = SettingController.ShipperIban,
                bic = SettingController.ShipperBic
            };

            return objBankData;
        }

        private ShipWebReference.ShipmentItemDDType CreateShipmentItemDDType(int iIndex)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.ShipmentItemDDType objShipmentItem = new ShipWebReference.ShipmentItemDDType();
            Shipment.Package objItemInfo = _objShipment.Packages[iIndex];
            objShipmentItem.WeightInKG = decimal.Parse(objItemInfo.WeightInKG);
            objShipmentItem.LengthInCM = objItemInfo.LengthInCM;
            objShipmentItem.WidthInCM = objItemInfo.WidthInCM;
            objShipmentItem.HeightInCM = objItemInfo.HeightInCM;
            objShipmentItem.PackageType = objItemInfo.PackageType;
            return objShipmentItem;
        }

        private ShipWebReference.ExportDocumentDDType CreateExportDocDDType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            if (_objShipment.ItemPositions.Count <= 0)
                return null;

            List<ShipWebReference.ExportDocumentDDTypeExportDocPosition> aobjExportDocumentPosition = new List<ShipWebReference.ExportDocumentDDTypeExportDocPosition>();

            foreach (var objPos in _objShipment.ItemPositions)
            {
                ShipWebReference.ExportDocumentDDTypeExportDocPosition objExportDocumentPosition = new ShipWebReference.ExportDocumentDDTypeExportDocPosition();
                objExportDocumentPosition.Amount = objPos.Amount;
                objExportDocumentPosition.Description = objPos.Description;
                objExportDocumentPosition.CommodityCode = objPos.CommodityCode;
                objExportDocumentPosition.CountryCodeOrigin = objPos.CountryCodeOrigin;
                objExportDocumentPosition.CustomsCurrency = objPos.CustomsCurrency;
                objExportDocumentPosition.CustomsValue = objPos.CustomsValue;
                objExportDocumentPosition.GrossWeightInKG = objPos.GrossWeightInKG;
                objExportDocumentPosition.NetWeightInKG = objPos.NetWeightInKG;
                aobjExportDocumentPosition.Add(objExportDocumentPosition);
            }

            ShipWebReference.ExportDocumentDDType objExportDocument = new ShipWebReference.ExportDocumentDDType();
            objExportDocument.ExportDocPosition = aobjExportDocumentPosition.ToArray();
            objExportDocument.AttestationNumber = _objShipment.ExportDocumentAttestationNumber;
            objExportDocument.CommodityCode = _objShipment.ExportDocumentCommodityCode;
            objExportDocument.ExportTypeDescription = _objShipment.ExportDocumentExportTypeDescription;
            objExportDocument.MRNNumber = _objShipment.ExportDocumentMRNNumber;
            objExportDocument.PermitNumber = _objShipment.ExportDocumentPermitNumber;
            objExportDocument.CountryCodeOrigin = _objShipment.ExportDocumentCountryCodeOrigin;
            objExportDocument.Amount = _objShipment.ExportDocumentAmount;
            objExportDocument.CustomsValue = _objShipment.ExportDocumentCustomsValue;
            objExportDocument.AdditionalFee = _objShipment.ExportDocumentAdditionalFee;
            objExportDocument.CustomsCurrency = _objShipment.ExportDocumentCustomsCurrency;
            objExportDocument.Description = _objShipment.ExportDocumentDescription;
            objExportDocument.TermsOfTrade = _objShipment.ExportDocumentTermsOfTrade;
            objExportDocument.InvoiceType = ShipWebReference.ExportDocumentDDTypeInvoiceType.commercial;
            objExportDocument.InvoiceDate = _objShipment.ExportDocumentInvoiceDate;
            objExportDocument.InvoiceNumber = _objShipment.ExportDocumentInvoiceNumber;
            objExportDocument.ExportType = ShipWebReference.ExportDocumentDDTypeExportType.Item0;
            objExportDocument.ExportTypeSpecified = true;
            objExportDocument.InvoiceTypeSpecified = true;

            return objExportDocument;
        }
               
        private ShipWebReference.GetLabelDDRequest GetLabelDDRequest(string strShipmentId)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.GetLabelDDRequest ddRequest = new ShipWebReference.GetLabelDDRequest();
            ddRequest.Version = CreateVersion();
            ShipWebReference.ShipmentNumberType shNumber = new ShipWebReference.ShipmentNumberType();
            shNumber.ItemElementName = ShipWebReference.ItemChoiceType7.shipmentNumber;
            shNumber.Item = strShipmentId;

            ShipWebReference.ShipmentNumberType[] shNumbers = new ShipWebReference.ShipmentNumberType[1];
            shNumbers[0] = shNumber;
            ddRequest.ShipmentNumber = shNumbers;
            return ddRequest;
        }

        private ShipWebReference.DeleteShipmentDDRequest GetDeleteShipmentDDRequest(string strShipmentId)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.DeleteShipmentDDRequest ddRequest = new ShipWebReference.DeleteShipmentDDRequest();
            ddRequest.Version = CreateVersion();
            ShipWebReference.ShipmentNumberType shNumber = new ShipWebReference.ShipmentNumberType();
            shNumber.ItemElementName = ShipWebReference.ItemChoiceType7.shipmentNumber;
            shNumber.Item = strShipmentId;

            ShipWebReference.ShipmentNumberType[] shNumbers = new ShipWebReference.ShipmentNumberType[1];
            shNumbers[0] = shNumber;
            ddRequest.ShipmentNumber = shNumbers;
            return ddRequest;
        }

        private DoManifestDDRequest DoManifestDDRequest(string[] astrTrackingnumbers)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            ShipWebReference.DoManifestDDRequest objRequest = new ShipWebReference.DoManifestDDRequest();
            objRequest.Version = CreateVersion();

            List<ShipWebReference.ShipmentNumberType> cobjShipmentNumberTypes = new List<ShipmentNumberType>();

            foreach(string strItem in astrTrackingnumbers)
            {
                cobjShipmentNumberTypes.Add(new ShipWebReference.ShipmentNumberType()
                {
                    Item = strItem,
                    ItemElementName = ShipWebReference.ItemChoiceType7.shipmentNumber
                });
            }

            objRequest.ShipmentNumber = cobjShipmentNumberTypes.ToArray();

            return objRequest;
        }

        private ShipWebReference.GetManifestDDRequest GetManifestDDRequest(DateTime objDate)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            ShipWebReference.GetManifestDDRequest objRequest = new ShipWebReference.GetManifestDDRequest();
            objRequest.Version = CreateVersion();

            objRequest.Item = objDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            return objRequest;
        }

        private ShipWebReference.GetManifestDDRequest GetManifestDDRequest(DateTime objFromDate, DateTime objToDate)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            ShipWebReference.GetManifestDDRequest objRequest = new ShipWebReference.GetManifestDDRequest();
            objRequest.Version = CreateVersion();

            objRequest.Item = new ShipWebReference.GetManifestDDRequestManifestDateRange()
            {
                fromDate = objFromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                toDate = objToDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };

            return objRequest;
        }

        private ShipWebReference.GetExportDocDDRequest GetExportDocDDRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.GetExportDocDDRequest objRequest = new ShipWebReference.GetExportDocDDRequest();
            objRequest.Version = CreateVersion();

            ShipWebReference.ShipmentNumberType objShipmentNumber = new ShipWebReference.ShipmentNumberType()
            {
                Item = _objShipment.Trackingnumber,
                ItemElementName = ShipWebReference.ItemChoiceType7.shipmentNumber
            };

            objRequest.ShipmentNumber = new ShipWebReference.ShipmentNumberType[] { objShipmentNumber };

            return objRequest;
        }

        private ShipWebReference.Version CreateVersion()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipWebReference.Version objVersion = new ShipWebReference.Version();
            objVersion.majorRelease = _objShipment.MajorRelease;
            objVersion.minorRelease = _objShipment.MinorRelease;
            return objVersion;
        }

        #endregion
    }
}
