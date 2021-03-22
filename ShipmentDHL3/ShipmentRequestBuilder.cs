using ShipmentLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.IO;
using ShipmentDHL.ShipWebReference;
using ShipmentLib.SoapDumper;

namespace ShipmentDHL
{
    public class ShipmentRequestBuilder : ITraceable
    {
        private Shipment _objShipment;
        private GKV3XAPIServicePortTypeClient _objWebService;
        private AuthentificationType _objAuthentication;
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

        public void DeleteOldShipment()
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

                    if (DeleteShipmentRequest())
                        break;
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during executing CREATEDD request to DHL service!");
            }
        }

        public ShipWebReference.CreateShipmentOrderResponse CreateNewShipmentRequest()
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

                ShipWebReference.CreateShipmentOrderRequest objRequest = CreateShipmentOrderRequest(_objShipment.PackageCount);
                ShipWebReference.CreateShipmentOrderResponse objResponse = null;

                for (int i = 0; i <= iNumRetriesAllowed; i++)
                {
                    objResponse = _objWebService.createShipmentOrder(_objAuthentication, objRequest);

                    if (objResponse != null && objResponse.Status.statusCode.Equals("0"))
                    {
                        _objShipment.Trackingnumber = objResponse.CreationState[0].shipmentNumber;

                        for(int iUrl=0; iUrl < objResponse.CreationState.Length; iUrl++)
                        {
                            string strLabelUrl = objResponse.CreationState[iUrl].Labelurl;
                            string strFile = Path.Combine(SettingController.DownloadFolder, iUrl.ToString() + "_" + _objShipment.Trackingnumber + "_" + SettingController.DHL_PDF_Filename);
                            ShipmentTools.Download(strLabelUrl, strFile);
                            _objShipment.DownloadedFiles.Add(strFile);
                        }

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

                        Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : " + objResponse.Status.statusCode + "/" + string.Join(",", objResponse.Status.statusMessage));

                        if (_strCmd.Equals("SHIP")) DeleteOldShipment();
                        new ShipmentException(null, ReadableException(objResponse, _strAssembly));
                    }
                }

                if(objResponse != null)
                    Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : " + objResponse.Status.statusCode + "/" + string.Join(",", objResponse.Status.statusMessage));
                else
                    Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : objResponse = null" );

                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Create Shipping with Retry FAILED!!");
                return null;
            }
            catch (Exception objException)
            {
                if (_strCmd.Equals("SHIP")) DeleteOldShipment();
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during executing CREATEDD request to DHL service!");
                return null;
            }
        }

        public void DoManifestToday()
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
                    if (DoAndPrintManifest(0))
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

            if (strDateBegin.Equals(strDateEnd))
            {
                objDateBegin = ShipmentTools.ParseDate(strDateBegin, false);
            }
            else
            {
                objDateBegin = ShipmentTools.ParseDate(strDateBegin, false);
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
                    if (GetAndPrintManifest(objDateBegin))
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

        private bool DoAndPrintManifest(int iDay)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var cobjBelege = _objDbController.GetShippedOrders(DateTime.Today.AddDays(iDay), DateTime.Today.AddDays(iDay), "DHL");
            if (cobjBelege != null && cobjBelege.Count <= 0)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": No Orders found to manifest for today!");

            List<string> cstrTrackingnumbers = cobjBelege.Select(a => a.Trackingnumbers).ToList<string>();

            if (cstrTrackingnumbers == null || cstrTrackingnumbers.Count == 0)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": No Trackingnumbers are given!");

            DoManifestRequest objRequest = DoManifestRequest(cstrTrackingnumbers.ToArray());

            try
            {
                DoManifestResponse objResponse = _objWebService.doManifest(_objAuthentication, objRequest);

                if (objResponse != null && objResponse.Status.statusCode.Equals("0"))
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Manifested " + objResponse.ManifestState.Length + " of " + cstrTrackingnumbers.Count);

                    List<ManifestedDetail> cobjManifests = new List<ManifestedDetail>();

                    foreach (var objState in objResponse.ManifestState)
                    {
                        if(objState.Status.Equals("0"))
                            Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": OK    Trackingnumber " + objState.shipmentNumber + " manifested!");
                        else
                            Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": ERROR Trackingnumber " + objState.shipmentNumber + " > " + string.Join(",", objState.Status.statusMessage) + "/" + string.Join(",", objState.Status.statusMessage));

                        cobjManifests.Add(new ManifestedDetail(objState.shipmentNumber, objState.Status.statusCode, string.Join(",", objState.Status.statusMessage)));
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
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during Manifest Data with Code/Error " + objResponse.Status.statusCode + "/" + string.Join(",", objResponse.Status.statusMessage));
                }

                return true;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during manifesting!");
                return false;
            }
        }

        private bool GetAndPrintManifest(DateTime objDateBegin)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var objRequest = GetManifestRequest(objDateBegin);

            try
            {
                ShipWebReference.GetManifestResponse objResponse = _objWebService.getManifest(_objAuthentication, objRequest);

                if (objResponse != null && objResponse.Status.statusCode.Equals("0"))
                {
                    if (objResponse.manifestData.Length > 0)
                    {
                        string strFile = Path.Combine(SettingController.DownloadFolder, SettingController.DHL_Manifest_Filename);
                        File.WriteAllBytes(strFile, objResponse.manifestData);
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Manifest Data downloaded and saved as " + strFile);

                        //print
                        bool bDesign = SettingController.LLDesign.Equals("1") ? true : false;
                        bool bPreview = SettingController.LLPreview.Equals("1") ? true : false;
                        _objLL.DesignLLHtml(SettingController.FileLLExportDocumentPdf, strFile, bDesign, bPreview);
                        return true;
                    }
                    else
                    {
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data with Code/Error " + objResponse.Status.statusCode + "/" + string.Join(",", objResponse.Status.statusMessage));
                    }
                }
                else
                {
                    if(objResponse == null)
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data with objResponse = null!");
                    else
                        new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data with Code/Error " + objResponse.Status.statusCode + "/" + string.Join(",", objResponse.Status.statusMessage));
                }

                return false;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during downloading Export Document!");
                return false;
            }
        }

        private bool DeleteShipmentRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            DeleteShipmentOrderRequest objRequest = GetDeleteShipmentOrderRequest(_objShipment.Trackingnumber);

            try
            {
                DeleteShipmentOrderResponse objResponse = _objWebService.deleteShipmentOrder(_objAuthentication, objRequest);

                if(objResponse == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Delete request failed for trackingnumber " + _objShipment.Trackingnumber + " because objResponse = null!");

                Statusinformation status = objResponse.Status;
                string statusMessage = string.Join(",", status.statusMessage);
                DeletionState[] aobjStates = objResponse.DeletionState;

                foreach (DeletionState objState in aobjStates)
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Status of Trackingnumber was " + objState.Status.statusCode + "/" + string.Join(",", objState.Status.statusMessage));

                    if(objState.Status.statusCode.Equals("0"))
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

        public static string ReadableException(ShipWebReference.CreateShipmentOrderResponse objResult, string strMethod)
        {
            string strDhlException = "ERROR: " + objResult.Status.statusCode + " \n";

            if (objResult.CreationState.Length > 0)
            {
                var messages = objResult.CreationState.ElementAt(0).LabelData.Status.statusMessage;
                if (messages.Length > 0)
                {
                    for (var i = 0; i < messages.Length; i++)
                    {
                        strDhlException += string.Format("{0}: {1}\n", strMethod, messages.ElementAt(i));
                    }
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
                    _objShipment.ProdCode = _objShipment.ProdCode_V62WP_DHLWarenpost;

                if (_objShipment.Order.PaketAnzahl > 0)
                    _objShipment.PackageCount = (int)_objShipment.Order.PaketAnzahl;

                for (int i = 0; i < _objShipment.PackageCount; i++)
                {
                    var objPackage = new Shipment.Package(_objShipment.Order.Gesamtgewicht, SettingController.Paket_Length, SettingController.Paket_Width, SettingController.Paket_Height);
                    _objShipment.Packages.Add(objPackage);
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
                    if (_strCmd.Equals("SHIP")) DeleteOldShipment();
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": List and Label file " + SettingController.FileLLPdf + " not found!");
                }
            }
            catch (Exception objException)
            {
                if (_strCmd.Equals("SHIP")) DeleteOldShipment();
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Printing Error!");
            }
        }

        private void InitRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            _objWebService = new ShipWebReference.GKV3XAPIServicePortTypeClient("ShipmentServiceSOAP11port0");
            _objWebService.Endpoint.Address = new System.ServiceModel.EndpointAddress(_objShipment.Endpoint);
            System.ServiceModel.BasicHttpBinding objBinding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport);
            objBinding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic;
            objBinding.MaxReceivedMessageSize = 10*1024*1024;
            _objWebService.Endpoint.Binding = objBinding;

            _objAuthentication = new AuthentificationType();

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

        private CreateShipmentOrderRequest CreateShipmentOrderRequest(int iCountOfPackages)
        {
            CreateShipmentOrderRequest createShipmentOrderRequest = new CreateShipmentOrderRequest();
            createShipmentOrderRequest.labelResponseType = CreateShipmentOrderRequestLabelResponseType.ZPL2;
            createShipmentOrderRequest.Version = CreateVersion();

            ShipmentOrderType[] shipmentOrderTypes = new ShipmentOrderType[iCountOfPackages];
            for (var i = 0; i < iCountOfPackages; i++)
            {
                ShipmentOrderTypeShipment shipment = CreateShipmentOrderTypeShipment(i);

                ShipmentOrderType shipmentOrderType = new ShipmentOrderType();
                shipmentOrderType.sequenceNumber = "";
                shipmentOrderType.Shipment = shipment;

                shipmentOrderTypes[i] = shipmentOrderType;
            }

            createShipmentOrderRequest.ShipmentOrder = shipmentOrderTypes;

            return createShipmentOrderRequest;
        }
        
        private ShipmentOrderTypeShipment CreateShipmentOrderTypeShipment(int index)
        {
            ShipmentOrderTypeShipment shipment = new ShipmentOrderTypeShipment();
            shipment.ShipmentDetails = CreateShipmentDetailsType(index);

            ShipperType shipper = new ShipperType();
            shipper.Name = CreateShipperCompany();
            shipper.Address = CreateShipperNativeAddressType();
            shipper.Communication = CreateShipperCommunicationType();
            shipment.Item = shipper;
            shipment.ReturnReceiver = shipper;

            ReceiverType receiver = new ReceiverType();
            receiver.name1 = CreateReceiverCompany().name1;
            receiver.Item = CreateReceiverNativeAddressType();
            receiver.Communication = CreateReceiverCommunicationType();
            shipment.Receiver = receiver;

            return shipment;
        }

        private ShipmentDetailsTypeType CreateShipmentDetailsType(int index)
        {
            ShipmentDetailsTypeType shipmentDetails = new ShipmentDetailsTypeType
            {
                product = _objShipment.ProdCode,
                shipmentDate = DateTime.Today.ToString(_objShipment.SDF).MaxLength(10),
                accountNumber = _objShipment.EKP,

                Notification = new ShipmentNotificationType
                {
                    recipientEmailAddress = _objShipment.ReceiverContactEMail
                }
            };

            ShipmentItemType shItemType = new ShipmentItemType();
            shipmentDetails.ShipmentItem = shItemType;
            shipmentDetails.ShipmentItem = CreateDefaultShipmentItemType(index);
            shipmentDetails.customerReference = _objShipment.ShipmentDesc;

            return shipmentDetails;
        }

        private GetLabelRequest GetDefaultLabelRequest(string shipmentId)
        {
            GetLabelRequest request = new GetLabelRequest();
            request.Version = CreateVersion();
            string shNumber = shipmentId;
            string[] shNumbers = new string[1];
            shNumbers[0] = shNumber;
            request.shipmentNumber = shNumbers;
            return request;
        }

        private DeleteShipmentOrderRequest GetDeleteShipmentOrderRequest(string shipmentId)
        {
            DeleteShipmentOrderRequest request = new DeleteShipmentOrderRequest();
            request.Version = CreateVersion();
            string[] shNumbers = new string[1];
            shNumbers[0] = shipmentId;
            request.shipmentNumber = shNumbers;
            return request;
        }

        private DoManifestRequest DoManifestRequest(string[] astrTrackingnumbers)
        {
            DoManifestRequest objRequest = new DoManifestRequest();
            objRequest.Version = CreateVersion();

            List<ShipmentNumberType> cobjShipmentNumberTypes = new List<ShipmentNumberType>();

            foreach(string strItem in astrTrackingnumbers)
            {
                cobjShipmentNumberTypes.Add(new ShipmentNumberType()
                {
                    Item = strItem,
                });
            }

            objRequest.Items = cobjShipmentNumberTypes.ToArray();

            return objRequest;
        }

        private GetManifestRequest GetManifestRequest(DateTime objDate)
        {
            GetManifestRequest objRequest = new ShipWebReference.GetManifestRequest();
            objRequest.Version = CreateVersion();
            objRequest.manifestDate = objDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return objRequest;
        }

        private ShipWebReference.Version CreateVersion()
        {
            ShipWebReference.Version version = new ShipWebReference.Version();
            version.majorRelease = _objShipment.MajorRelease;
            version.minorRelease = _objShipment.MinorRelease;
            return version;
        }

        private NativeAddressTypeNew CreateShipperNativeAddressType()
        {
            NativeAddressTypeNew address = new NativeAddressTypeNew();
            address.streetName = _objShipment.ShipperStreet;
            address.streetNumber = _objShipment.ShipperStreet;
            address.city = _objShipment.ShipperCity;
            address.zip = _objShipment.ShipperZip;
            NewCountryType origin = new NewCountryType();
            origin.countryISOCode = _objShipment.ShipperCountryCode;
            address.Origin = origin;

            return address;
        }

        private ReceiverNativeAddressType CreateReceiverNativeAddressType()
        {
            ReceiverNativeAddressType address = new ReceiverNativeAddressType();
            NewCountryType origin = new NewCountryType();
            address.streetName = _objShipment.ReceiverStreet;
            address.streetNumber = _objShipment.ReceiverStreentNr;
            address.city = _objShipment.ReceiverCity;
            address.zip = _objShipment.ReceiverZip;
            origin.countryISOCode = _objShipment.ReceiverCountryCode;
            address.Origin = origin;
            return address;
        }

        private CommunicationType CreateShipperCommunicationType()
        {
            CommunicationType communication = new CommunicationType();
            communication.email = _objShipment.ShipperEMail;
            communication.contactPerson = _objShipment.ShipperName;
            communication.phone = _objShipment.ShipperPhone;
            return communication;
        }

        private CommunicationType CreateReceiverCommunicationType()
        {
            CommunicationType communication = new CommunicationType();
            communication.email = _objShipment.ReceiverContactEMail;
            communication.contactPerson = _objShipment.ReceiverContactName;
            communication.phone = _objShipment.ReceiverContactPhone;
            return communication;
        }

        private NameType CreateShipperCompany()
        {
            NameType name = new NameType();
            name.name1 = _objShipment.ShipperCompanyName;
            return name;
        }

        private NameType CreateReceiverCompany()
        {
            NameType name = new NameType();
            name.name1 = _objShipment.ReceiverCompanyName;
            name.name2 = string.Format("{0} {1}", _objShipment.ReceiverFirstName, _objShipment.ReceiverLastName);
            return name;
        }

        private ShipmentItemType CreateDefaultShipmentItemType(int index)
        {
            ShipmentItemType shipmentItem = new ShipmentItemType();
            Shipment.Package objItemInfo = _objShipment.Packages[index];
            shipmentItem.weightInKG = decimal.Parse(objItemInfo.WeightInKG);
            shipmentItem.lengthInCM = objItemInfo.LengthInCM;
            shipmentItem.widthInCM = objItemInfo.WidthInCM;
            shipmentItem.heightInCM = objItemInfo.HeightInCM;
            return shipmentItem;
        }

        #endregion
    }
}
