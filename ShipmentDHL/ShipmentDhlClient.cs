using ShipmentLib;
using ShipmentLib.SoapDumper;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace ShipmentDHL
{
    public class ShipmentDhlClient : ITraceable
    {
        private string _strCmd;
        private string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        private Shipment _objShipment;
        private DatabaseController _objDbController;
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

        public bool OldVersion { get; set; }

        public ShipmentDhlClient(string strNumber, string strCmd)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (string.IsNullOrEmpty(strNumber))
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": strNumber is NULL!");

            if (string.IsNullOrEmpty(strCmd))
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": strCmd is NULL!");

            OldVersion = true;
            _strCmd = strCmd;
            _objShipment = new Shipment();
            _objLL = new PrintListLabel();

            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Command = " + strCmd);
            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Executing Request in Test Mode? : " + SettingController.DHL_TestMode.ToString());

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
                {
                    OldVersion = InitShipment();
                }
            }
        }

        public void CreateNewShipment()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                var result = false;
                if (OldVersion)
                {
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Sending Shipment to DHL using old version: " + OldVersion.ToString());
                    result = new ShipmentRequestBuilderOld(_objDbController, _objShipment).CreateShipment();
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Result of CreateShipment " + result.ToString());
                    if (result)
                    {
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Printing labels ...");
                        PrintLabels(true);
                    }
                }
                else
                {
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Sending Shipment to DHL using old version: " + OldVersion.ToString());
                    result = new ShipmentRequestBuilderNew(_objDbController, _objShipment, ShipWebReference3.CreateShipmentOrderRequestLabelResponseType.ZPL2).CreateShipment();
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Result of CreateShipment " + result.ToString());
                    if (result)
                    {
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Printing labels ...");
                        PrintLabels(false);
                    }
                }

            }
            catch (Exception e)
            {
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": " + e.Message);
                if (_strCmd.Equals("SHIP")) DeleteOldShipmentDD();
            }
        }

        private bool CreateShipment()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                int iOrderNr = ShipmentTools.SafeParseInt(_objShipment.InvoiceNumber);
                if (iOrderNr == 0)
                {
                    var message = _strAssembly + ":" + strMethod + ": Ordernumber must be greater than 0";
                    new ShipmentException(null, message);
                    throw new Exception(message);
                }

                _objShipment.Order = _objDbController.GetOrder(iOrderNr);

                if (_objShipment.Order == null)
                {
                    var message = _strAssembly + ":" + strMethod + ": No order found for Ordernumber=" + iOrderNr.ToString();
                    new ShipmentException(null, message);
                    throw new Exception(message);
                }

                _objShipment.Lieferaddress = _objDbController.GetLieferadressen(_objShipment.Order.LieferadressenID);

                _objShipment.Address = _objDbController.GetAdress(_objShipment.Order.AdressenID);
                if (_objShipment.Address == null)
                {
                    var message = _strAssembly + ":" + strMethod + ": No Address found for addressenId=" + _objShipment.Order.AdressenID.ToString();
                    new ShipmentException(null, message);
                    throw new Exception(message);
                }

                _objShipment.OrderCountryCode = _objDbController.GetCountryCode(_objShipment.Order.LieferLand).MaxLength(2);

                if (_objShipment.Order.PaketAnzahl > 0)
                    _objShipment.PackageCount = (int)_objShipment.Order.PaketAnzahl;

                string strConfigFile = _strAssembly + ".dll.config";
                var objPtP = ShipmentTools.GetShippingTypes("DHL", strConfigFile);
                var objPackageType = objPtP.Find((int)_objShipment.Order.VersandartenID);

                //Assign DHL ProductCode
                if (objPackageType == null)
                {
                    //DHL National/International && EPI International use without ID
                    if (_objShipment.OrderCountryCode.Equals("DE"))
                        _objShipment.DDProdCode = _objShipment.DDProdCode_EPN_EUROPACK_NATIONAL;
                    else
                        _objShipment.DDProdCode = _objShipment.DDProdCode_EPI_EUROPAK_INTERNATIONAL;

                    if (_objShipment.Order.ZahlungsartID == (int)Paymenttypes.Nachnahme)
                    {
                        if (_objShipment.DDProdCode == _objShipment.DDProdCode_EPI_EUROPAK_INTERNATIONAL)
                            new ShipmentException(null, _strAssembly + ":" + strMethod + ": COD with DHL ProductCode = EPI not possible!");

                        _objShipment.Order.Buchungsbetrag = _objShipment.Order.Buchungsbetrag + SettingController.PauschalBetrag;
                        _objShipment.CODCurrency = "EUR";
                        _objShipment.CODAmount = (decimal)_objShipment.Order.Buchungsbetrag;
                        Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": COD Amount is " + _objShipment.CODAmount.ToString(CultureInfo.InvariantCulture));
                    }

                    CreatePackages(_objShipment.Order.Gesamtgewicht, SettingController.Paket_Length, SettingController.Paket_Width, SettingController.Paket_Height);
                    return true;
                }
                else if (objPackageType.InternalId.Length >= 3)
                {
                    //DHL Warenpost use with ID
                    _objShipment.DDProdCode = objPackageType.InternalId;
                    CreatePackages(1, "25", "15", "1");
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": ProductCode is " + _objShipment.DDProdCode);
                    return false;
                }
                else
                {
                    var message = _strAssembly + ":" + strMethod + ": DHL ProductCode not found in config file " + strConfigFile + "!";
                    new ShipmentException(null, message);
                    throw new Exception(message);
                }


            }
            catch (Exception objException)
            {
                var message = _strAssembly + ":" + strMethod + ": Shipment could not be created!";
                new ShipmentException(objException, message);
                throw new Exception(message);
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
                    var cobjManifests = new ShipmentRequestBuilderOld(_objDbController, _objShipment).DoAndPrintManifestDD(0);
                    if (cobjManifests == null || cobjManifests.Count == 0)
                    {
                        break;
                    }

                    if (cobjManifests != null && cobjManifests.Count > 0)
                    {
                        ShipmentSummary objShipmentSummary = ShipmentSummary.CreateDefault();
                        objShipmentSummary.ManifestedDetails = cobjManifests;
                        objShipmentSummary.DateBegin = DateTime.Today;
                        objShipmentSummary.DateEnd = DateTime.Today;

                        bool bDesign = SettingController.LLDesign.Equals("1") ? true : false;
                        bool bPreview = SettingController.LLPreview.Equals("1") ? true : false;
                        _objLL.DesignLLSummary(SettingController.FileLLManifestedDHL, bDesign, bPreview, objShipmentSummary);

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
                    var strFile = new ShipmentRequestBuilderOld(_objDbController, _objShipment).GetAndPrintManifestDD(objDateBegin, objDateEnd);
                    if (string.IsNullOrEmpty(strFile))
                    {
                        if(File.Exists(strFile))
                        {
                            //print
                            bool bDesign = SettingController.LLDesign.Equals("1") ? true : false;
                            bool bPreview = SettingController.LLPreview.Equals("1") ? true : false;
                            _objLL.DesignLLHtml(SettingController.FileLLExportDocumentPdf, strFile, bDesign, bPreview);

                            string strNewFilename = Logger.Instance.GetLogDir();
                            strNewFilename += "GETMANIFEST_" + strDateBegin.Replace(".", "-") + "_" + strDateEnd.Replace(".", "-") + ".txt";
                            Logger.Instance.RenameLogfile(strNewFilename);
                            break;
                        }
                        else
                        {
                            Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Label File not exists in " + strFile);
                        }
                    }
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data");
            }
        }

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
                    if (_strCmd.Equals("SHIP"))
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Canceling Trackingnumber " + _objShipment.Trackingnumber);

                    if (new ShipmentRequestBuilderOld(_objDbController, _objShipment).DeleteShipmentDDRequest())
                    {
                        if (_strCmd.Equals("DELETE"))
                        {
                            string strNewFilename = Logger.Instance.GetLogDir();
                            strNewFilename += "DELETE_" + _objShipment.Trackingnumber + ".txt";
                            Logger.Instance.RenameLogfile(strNewFilename);
                        }
                        break;
                    }
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during executing CREATEDD request to DHL service!");
            }
        }

        private bool InitShipment()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                var oldVersion = CreateShipment();
                CreateReceiver();
                CreateExportDocuments();
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Ship request initialized successfull");
                return oldVersion;
            }
            catch (Exception objException)
            {
                var message = _strAssembly + ":" + strMethod + ": Error during ship request initialisation!";
                new ShipmentException(objException, message);
                throw new Exception(message);
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

        private void CreatePackages(double? weight, string length, string width, string height)
        {
            for (int i = 0; i < _objShipment.PackageCount; i++)
            {
                var objPackage = new Package(weight, length, width, height, "PK");
                _objShipment.Packages.Add(objPackage);
            }
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

            if (_objShipment.DDProdCode.Equals(_objShipment.DDProdCode_BPI_BUSINESS_PAKET_INTERNATIONAL))
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
                _objShipment.ItemPositions.Add(new ItemPosition()
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

        private void PrintLabels(bool toLaserPrinter)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            try
            {
                if(toLaserPrinter)
                {
                    //print the first page to laser printer
                    bool bDesign = SettingController.LLDesign.Equals("1") ? true : false;
                    bool bPreview = SettingController.LLPreview.Equals("1") ? true : false;

                    if (File.Exists(SettingController.FileLLPdf))
                    {
                        foreach (string strFile in _objShipment.DownloadedFiles)
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
                else
                {
                    foreach (string strFile in _objShipment.DownloadedFiles)
                    {
                        PrintDirectToLabel.PrintRaw(SettingController.LabelPrinter, strFile, _objShipment.Trackingnumber, Logger.Instance);
                    }
                }
            }
            catch (Exception objException)
            {
                if (_strCmd.Equals("SHIP")) DeleteOldShipmentDD();
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Printing Error!");
            }
        }
    }
}
