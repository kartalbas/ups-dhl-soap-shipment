using System;
using System.Text;
using System.IO;
using System.Globalization;
using ShipmentLib;
using System.Diagnostics;
using System.Reflection;
using ShipmentLib.SoapDumper;
using System.Net;

namespace ShipmentUPS
{
    public class ShipmentRequestBuilder : ITraceable
    {
        private string _strOrderNr;
        private string _strCurrentPath;
        private string _strLaserPrinter;
        private string _strLabelPrinter;
        private string _strPackageTypeCode;
        private string _strServiceNotificationCode;
        private string _strServiceDescription;
        private string _strCODAmountCurrencyCode;
        private string _strShipUnitOfMeasurementTypeCode;
        private string _strShipUnitOfMeasurementTypeDescription;
        private string _strLabelImageFormatCode;
        private string _strLabelStockSizeHeight;
        private string _strLabelStockSizeWidth;
        private string _strDownloadFolder;
        private string _strDownloadPath;
        private DatabaseController _objDbController;
        private PrintListLabel _objLL;
        private string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;


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

        #region Public methods

        public ShipmentRequestBuilder(string strOrderNr)
        {
            string strMethod = "ShipmentRequestBuilder";

            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Belegnummer = " + strOrderNr);

            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _strOrderNr = strOrderNr;
            _objDbController = new DatabaseController();
            _objLL = new PrintListLabel();

            Initialize();

            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Executing Request in Test Mode? : " + SettingController.UPS_TestMode.ToString());
        }

        public Shipment InitShipment()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                Shipment objShip = new Shipment(SettingController.UPS_AccessLicenseNumber, SettingController.UPS_Username, SettingController.UPS_Password);

                objShip.RequestOption = new string[] { "nonvalidate" };
                objShip.ShipmentDescription = SettingController.ShipmentDescription;
                objShip.ShipmentChargeType = SettingController.ShipmentChargeType;

                objShip.ShipperName = SettingController.ShipperName;
                objShip.ShipperAttentionName = SettingController.ShipperAttentionName;
                objShip.ShipperAddressLine = new string[] { SettingController.ShipperAddressLine };
                objShip.ShipperPostalCode = SettingController.ShipperPostalCode;
                objShip.ShipperCity = SettingController.ShipperCity;
                objShip.ShipperCountryCode = SettingController.ShipperCountryCode;
                objShip.ShipperPhoneNumber = SettingController.ShipperPhoneNumber;
                objShip.ShipperStateProvinceCode = SettingController.ShipperStateProvinceCode;
                objShip.ShipperEmail = SettingController.ShipperEmail;

                objShip.ShipperFromName = SettingController.ShipperFromName;
                objShip.ShipperFromAttentionName = SettingController.ShipperFromAttentionName;
                objShip.ShipperFromAddressLine = new string[] { SettingController.ShipperFromAddressLine };
                objShip.ShipperFromPostalCode = SettingController.ShipperFromPostalCode;
                objShip.ShipperFromCity = SettingController.ShipperFromCity;
                objShip.ShipperFromCountryCode = SettingController.ShipperFromCountryCode;
                objShip.ShipperFromPhoneNumber = SettingController.ShipperPhoneNumber;
                objShip.ShipperFromStateProvinceCode = SettingController.ShipperFromStateProvinceCode;
                objShip.ShipperFromEmail = SettingController.ShipperFromEmail;

                objShip.ServiceDescription = SettingController.ServiceDescription;
                objShip.ServiceNotificationCode = SettingController.ServiceNotificationCode;
                objShip.CODAmountCurrencyCode = SettingController.CODAmountCurrencyCode;
                objShip.PackageTypeCode = SettingController.PackageTypeCode;
                objShip.ShipUnitOfMeasurementTypeCode = SettingController.ShipUnitOfMeasurementTypeCode;
                objShip.ShipUnitOfMeasurementTypeDescription = SettingController.ShipUnitOfMeasurementTypeDescription;
                objShip.LabelImageFormatCode = SettingController.LabelImageFormatCode;
                objShip.LabelStockSizeHeight = SettingController.LabelStockSizeHeight;
                objShip.LabelStockSizeWidth = SettingController.LabelStockSizeWidth;

                objShip.LabelPrinter = SettingController.LabelPrinter;
                objShip.LaserPrinter = SettingController.LaserPrinter;

                int iOrderNr = ShipmentTools.SafeParseInt(_strOrderNr);
                if (iOrderNr == 0)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Error during ship request initialisation, Ordernumber is 0!");

                var objOrder = _objDbController.GetOrder(iOrderNr);
                if (objOrder == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Error during ship request initialisation, Ordernumber=" + iOrderNr.ToString() + " not found!");

                var objLieferadress = _objDbController.GetLieferadressen(objOrder.LieferadressenID);
                if (objLieferadress == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Error during ship request initialisation - LieferadressID=" + objOrder.LieferadressenID.ToString() + " not found!");

                objShip.ShipmentReferenceNumber = objOrder.Kundennummer;

                if((int)objOrder.PaketAnzahl > 0)
                    objShip.Packages = (int)objOrder.PaketAnzahl;


                var objAddress = _objDbController.GetAdress(objOrder.AdressenID);
                if (objAddress == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Error during ship request initialisation, addressenId=" + objOrder.AdressenID.ToString() + " not found!");

                var objCountry = _objDbController.GetLand(objLieferadress.Land);
                if (objCountry == null)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Error during ship request initialisation, countryname=" + objOrder.LieferLand + " not found!");

                objShip.ShipToName = ShipmentTools.ChangeUmlaute(objLieferadress.Name1);
                if (objShip.ShipToName.Length > 35)
                    objShip.ShipToName = objShip.ShipToName.Substring(0, 34);

                if (objLieferadress.Name2 == "")
                    objShip.ShipToAttentionName = ShipmentTools.ChangeUmlaute(objLieferadress.Name1);
                else
                    objShip.ShipToAttentionName = ShipmentTools.ChangeUmlaute(objLieferadress.Name2);

                if (objShip.ShipToAttentionName.Length > 35)
                    objShip.ShipToAttentionName = objShip.ShipToAttentionName.Substring(0, 34);

                if (objLieferadress.Strasse.Length > 31)
                    objShip.ShipToAddressLine = new string[] { ShipmentTools.ChangeUmlaute(objLieferadress.Strasse.Substring(0, 30))  + " " + objLieferadress.HausNr };
                else
                    objShip.ShipToAddressLine = new string[] { ShipmentTools.ChangeUmlaute(objLieferadress.Strasse) + " " + objLieferadress.HausNr };

                if (objLieferadress.Ort.Length > 30)
                    objShip.ShipToCity = ShipmentTools.ChangeUmlaute(objLieferadress.Ort.Substring(0, 29));
                else
                    objShip.ShipToCity = ShipmentTools.ChangeUmlaute(objLieferadress.Ort);

                if (objLieferadress.Plz.Length > 9)
                    objShip.ShipToPostalCode = objLieferadress.Plz.Substring(0, 9);
                else
                    objShip.ShipToPostalCode = objLieferadress.Plz;

                objShip.ShipToCountryCode = objCountry.Kuerzel;
                objShip.ShipToPhoneNumber = SettingController.ShipperPhoneNumber;
                objShip.ShipToStateProvinceCode = "";

                string strMail = ShipmentTools.IsValidEmail(objAddress.eMail);

                if (!strMail.Equals(string.Empty))
                    objShip.ShipToEmail = strMail;
                else
                    objShip.ShipToEmail = SettingController.ShipperEmail;

                if (objOrder.ZahlungsartID == (int)Paymenttypes.Bar)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": This shipment must be payed in Cash/Bar!");

                string strConfigFile = _strAssembly + ".dll.config";
                var objPtP = ShipmentTools.GetShippingTypes("UPS", strConfigFile);
                var objPackageType = objPtP.Find((int)objOrder.VersandartenID);

                if (objPackageType == null)
                {
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": UPS ID " + (int)objOrder.VersandartenID + " not found in config file " + strConfigFile + "!");
                }
                else if (objPackageType.InternalId.Length > 0 && objOrder.ZahlungsartID != (int)Paymenttypes.Uberweisung)
                {
                    objShip.ServiceTypeCode = objPackageType.InternalId;
                    objShip.ServiceTypeName = objPackageType.Naming;
                }
                else
                {
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": UPS ProductCode not found in config file " + strConfigFile + "!");
                }

                if (objOrder.ZahlungsartID == (int)Paymenttypes.Nachnahme)
                {
                    objShip.CODFundsCode = "1";
                    objShip.CODAmountMonetaryValue = string.Format(CultureInfo.GetCultureInfo("en-US").NumberFormat, "{0:0.00}", objOrder.Buchungsbetrag);
                }
                else
                {
                    objShip.CODFundsCode = "0";
                    objShip.CODAmountMonetaryValue = "0";
                }

                if (objOrder.Gesamtgewicht < 1)
                    objShip.PackageWeight = 1.0;
                else
                    objShip.PackageWeight = (double)objOrder.Gesamtgewicht;

                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Ship request initialized successfull");

                return objShip;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during ship request initialisation!");
                return null;
            }
        }

        public void CreateNewShipment(ShipmentRequestBuilder objRequest, Shipment objShipment)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            int iNumRetriesAllowed = ShipmentTools.SafeParseInt(SettingController.Retries);

            if (iNumRetriesAllowed <= 0)
            {
                Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Create Shipping with Retry FAILED!");
                return;
            }

            objShipment.Response = null;

            for (int i = 0; i <= iNumRetriesAllowed; i++)
            {
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Retry count: " + i.ToString());

                if (objShipment.Response == null)
                {
                    objShipment.Response = objRequest.CreateShipmentRequest(objShipment);

                    if (objShipment.Response != null)
                    {
                        if (objShipment.Response.Response.ResponseStatus.Code.Equals("1") || objShipment.Response.Response.ResponseStatus.Description.Equals("Success"))
                        {
                            PrintLabels(objShipment);
                            return;
                        }
                    }
                }
            }

            new ShipmentException(new Exception(), _strAssembly + ":" + strMethod + ": Create Shipping with Retry FAILED!");
        }

        public void DeleteOldShipment(ShipmentRequestBuilder objRequest, string strTrackingnumber)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            int iNumRetriesAllowed = ShipmentTools.SafeParseInt(SettingController.Retries);

            if (iNumRetriesAllowed <= 0)
            {
                Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Delete with Retry FAILED!");
                return;
            }

            for (int i = 0; i <= iNumRetriesAllowed; i++)
            {
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Retry count: " + i.ToString());

                if (DeleteShipment(strTrackingnumber))
                {
                    string strNewFilename = Logger.Instance.GetLogDir();
                    strNewFilename += "UPS_DELETE_" + strTrackingnumber + ".txt";
                    Logger.Instance.RenameLogfile(strNewFilename);
                    break;
                }
            }
        }

        public void PrintSummary(string strDateBegin, string strDateEnd)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                DateTime objDateBegin = DateTime.Today;
                DateTime objDateEnd = DateTime.Today;

                if (strDateBegin.Length > 0)
                {
                    try
                    {
                        objDateBegin = DateTime.ParseExact(strDateBegin + " 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    catch (Exception objException)
                    {
                        new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Input Error! Date is invalid formatted!");
                    }
                }

                if (strDateEnd.Length > 0)
                {
                    try
                    {
                        objDateEnd = DateTime.ParseExact(strDateEnd + " 23:59:59", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    catch (Exception objException)
                    {
                        new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Input Error! Date is invalid formatted!");
                    }
                }

                var cobjOrders = _objDbController.GetShippedOrders(objDateBegin, objDateEnd, "UPS");

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
                _objLL.DesignLLSummary(SettingController.FileLLSummaryUPS, bDesign, bPreview, objShipmentSummary);
                _objLL.DesignLLSummary(SettingController.FileLLListUPS, bDesign, bPreview, objShipmentSummary);
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during printing summary!");
            }
        }

        #endregion

        #region Private methods
        private bool DeleteShipment(string strTrackingnumber)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                VoidWebReference.VoidService objVoidService = new VoidWebReference.VoidService();
                VoidWebReference.VoidShipmentRequest objVoidRequest = new VoidWebReference.VoidShipmentRequest();
                VoidWebReference.RequestType objRequest = new VoidWebReference.RequestType();
                string[] astrRequestOption = { "1" };
                objRequest.RequestOption = astrRequestOption;
                objVoidRequest.Request = objRequest;
                VoidWebReference.VoidShipmentRequestVoidShipment objVoidShipment = new VoidWebReference.VoidShipmentRequestVoidShipment();
                objVoidShipment.ShipmentIdentificationNumber = strTrackingnumber;
                objVoidRequest.VoidShipment = objVoidShipment;

                VoidWebReference.UPSSecurity objUpss = new VoidWebReference.UPSSecurity();
                VoidWebReference.UPSSecurityServiceAccessToken objUpssSvcAccessToken = new VoidWebReference.UPSSecurityServiceAccessToken();
                objUpssSvcAccessToken.AccessLicenseNumber = SettingController.UPS_AccessLicenseNumber;
                objUpss.ServiceAccessToken = objUpssSvcAccessToken;
                VoidWebReference.UPSSecurityUsernameToken objUpssUsrNameToken = new VoidWebReference.UPSSecurityUsernameToken();
                objUpssUsrNameToken.Username = SettingController.UPS_Username;
                objUpssUsrNameToken.Password = SettingController.UPS_Password;
                objUpss.UsernameToken = objUpssUsrNameToken;
                objVoidService.UPSSecurityValue = objUpss;

                VoidWebReference.VoidShipmentResponse objVoidResponse = objVoidService.ProcessVoid(objVoidRequest);

                if (objVoidResponse != null)
                {
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": The transaction was a " + objVoidResponse.Response.ResponseStatus.Description);
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": " + strTrackingnumber + " has been " + objVoidResponse.SummaryResult.Status.Description);
                    RemoveTrackingnumber(strTrackingnumber);
                    return true;
                }
                else
                    return false;

            }
            catch (System.Web.Services.Protocols.SoapException objException)
            {
                if (objException.Detail.InnerText.StartsWith("Hard190102"))
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Error for Trackingnumber was " + objException.Detail.InnerText);
                    return true;
                }

                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": SoapException Message= " + objException.Message);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": SoapException Category:Code:Message= " + objException.Detail.LastChild.InnerText);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": SoapException XML String for all= " + objException.Detail.LastChild.OuterXml);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": SoapException StackTrace= " + objException.StackTrace);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": ");
                return false;
            }
            catch (System.ServiceModel.CommunicationException objException)
            {
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": " + objException.Message);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": " + objException.InnerException.ToString());
                return false;
            }
            catch (Exception objException)
            {
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": " + objException.Message);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": " + objException.InnerException.ToString());
                return false;
            }
        }

        private ShipWebReference.ShipmentResponse CreateShipmentRequest(Shipment objShipment)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (objShipment == null || objShipment.Packages == 0)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Can't send request to UPS, because the Shipment object is null!");

            try
            {
                //CREATE SHIPPER
                ShipWebReference.ShipAddressType objShipperAddress = new ShipWebReference.ShipAddressType();
                String[] addressLine = objShipment.ShipperAddressLine;
                objShipperAddress.AddressLine = addressLine;
                objShipperAddress.City = objShipment.ShipperCity;
                objShipperAddress.PostalCode = objShipment.ShipperPostalCode;
                objShipperAddress.StateProvinceCode = objShipment.ShipperStateProvinceCode;
                objShipperAddress.CountryCode = objShipment.ShipperCountryCode;
                objShipperAddress.AddressLine = addressLine;
                //shipper phone
                ShipWebReference.ShipPhoneType objShipperPhone = new ShipWebReference.ShipPhoneType();
                objShipperPhone.Number = objShipment.ShipperPhoneNumber;
                //assign all shipper data
                ShipWebReference.ShipperType objShipper = new ShipWebReference.ShipperType();
                objShipper.Address = objShipperAddress;
                objShipper.Phone = objShipperPhone;
                objShipper.Name = objShipment.ShipperName;
                objShipper.AttentionName = objShipment.ShipperAttentionName;
                objShipper.EMailAddress = objShipment.ShipperEmail;
                objShipper.ShipperNumber = objShipment.ShipperNumber;
                objShipper.EMailAddress = objShipment.ShipperEmail;

                //CREATE SHIPMENT
                ShipWebReference.BillShipperType objBillShipper = new ShipWebReference.BillShipperType();
                objBillShipper.AccountNumber = objShipment.Username;
                ShipWebReference.ShipmentChargeType objShipmentCharge = new ShipWebReference.ShipmentChargeType();
                objShipmentCharge.BillShipper = objBillShipper;
                objShipmentCharge.Type = objShipment.ShipmentChargeType;
                ShipWebReference.ShipmentChargeType[] aobjShipmentChargeArray = { objShipmentCharge };
                ShipWebReference.PaymentInfoType paymentInfo = new ShipWebReference.PaymentInfoType();
                paymentInfo.ShipmentCharge = aobjShipmentChargeArray;
                ShipWebReference.ShipmentType objShipmentType = new ShipWebReference.ShipmentType();
                objShipmentType.PaymentInformation = paymentInfo;
                objShipmentType.Description = objShipment.ShipmentDescription;
                objShipmentType.Shipper = objShipper;
                ShipWebReference.ReferenceNumberType[] aobjRefNumber = new ShipWebReference.ReferenceNumberType[1];
                ShipWebReference.ReferenceNumberType objRefType = new ShipWebReference.ReferenceNumberType();
                objRefType.Value = objShipment.ShipmentReferenceNumber;
                aobjRefNumber[0] = objRefType;
                objShipmentType.ReferenceNumber = aobjRefNumber;

                //COD                
                if (objShipment.CODFundsCode != null & objShipment.CODFundsCode != "0")
                {
                    ShipWebReference.ShipmentTypeShipmentServiceOptions objServiceOption = new ShipWebReference.ShipmentTypeShipmentServiceOptions();
                    ShipWebReference.CODType cod = new ShipWebReference.CODType();
                    cod.CODFundsCode = objShipment.CODFundsCode;
                    ShipWebReference.CurrencyMonetaryType objMonetaryType = new ShipWebReference.CurrencyMonetaryType();
                    objMonetaryType.CurrencyCode = objShipment.CODAmountCurrencyCode;
                    objMonetaryType.MonetaryValue = objShipment.CODAmountMonetaryValue;
                    cod.CODAmount = objMonetaryType;
                    objServiceOption.COD = cod;
                    objShipmentType.ShipmentServiceOptions = objServiceOption;
                }

                //create SHIPPER FROM
                ShipWebReference.ShipFromType objShipFrom = new ShipWebReference.ShipFromType();
                ShipWebReference.ShipAddressType objShipFromAddress = new ShipWebReference.ShipAddressType();
                //String[] shipFromAddressLine = ship.ShipperFromAddressLine;
                objShipFromAddress.AddressLine = addressLine;
                objShipFromAddress.City = objShipment.ShipperFromCity;
                objShipFromAddress.PostalCode = objShipment.ShipperFromPostalCode;
                objShipFromAddress.StateProvinceCode = objShipment.ShipperFromStateProvinceCode;
                objShipFromAddress.CountryCode = objShipment.ShipperFromCountryCode;
                objShipFrom.Address = objShipFromAddress;
                objShipFrom.AttentionName = objShipment.ShipperFromAttentionName;
                objShipFrom.Name = objShipment.ShipperFromName;
                objShipmentType.ShipFrom = objShipFrom;

                //CREATE SHIPPER TO
                ShipWebReference.ShipToType objShipTo = new ShipWebReference.ShipToType();
                ShipWebReference.ShipToAddressType objShipToAddress = new ShipWebReference.ShipToAddressType();
                String[] astrAddressLine1 = objShipment.ShipToAddressLine;
                objShipToAddress.AddressLine = astrAddressLine1;
                objShipToAddress.City = objShipment.ShipToCity;
                objShipToAddress.PostalCode = objShipment.ShipToPostalCode;
                objShipToAddress.StateProvinceCode = objShipment.ShipToStateProvinceCode;
                objShipToAddress.CountryCode = objShipment.ShipToCountryCode;
                objShipTo.Address = objShipToAddress;
                objShipTo.AttentionName = objShipment.ShipToAttentionName;
                objShipTo.Name = objShipment.ShipToName;
                objShipTo.EMailAddress = objShipment.ShipToEmail;
                ShipWebReference.ShipPhoneType objShipToPhone = new ShipWebReference.ShipPhoneType();
                objShipToPhone.Number = objShipment.ShipToPhoneNumber;
                objShipTo.Phone = objShipToPhone;
                objShipmentType.ShipTo = objShipTo;

                //CREATE SERVICE
                ShipWebReference.ServiceType objService = new ShipWebReference.ServiceType();
                objService.Code = objShipment.ServiceTypeCode;
                objShipmentType.Service = objService;

                //CREATE PACKAGE
                ShipWebReference.PackageType[] aobjPackages = new ShipWebReference.PackageType[objShipment.Packages];

                double dPackWeight = 0;

                if (objShipment.PackageWeight > 0)
                    dPackWeight = objShipment.PackageWeight / objShipment.Packages;
                else
                    dPackWeight = 1.0;

                for (int i = 0; i < objShipment.Packages; i++)
                {
                    ShipWebReference.PackageType objPackage = new ShipWebReference.PackageType();
                    aobjPackages[i] = objPackage;

                    ShipWebReference.PackageWeightType objPackageWeight = new ShipWebReference.PackageWeightType();

                    objPackageWeight.Weight = string.Format(CultureInfo.GetCultureInfo("en-US").NumberFormat, "{0:0.0}", dPackWeight);
                    ShipWebReference.ShipUnitOfMeasurementType objUOM = new ShipWebReference.ShipUnitOfMeasurementType();
                    objUOM.Code = objShipment.ShipUnitOfMeasurementTypeCode;
                    objUOM.Description = objShipment.ShipUnitOfMeasurementTypeDescription;
                    objPackageWeight.UnitOfMeasurement = objUOM;
                    aobjPackages[i].PackageWeight = objPackageWeight;

                    ShipWebReference.PackagingType objPackType = new ShipWebReference.PackagingType();
                    objPackType.Code = objShipment.PackageTypeCode;
                    aobjPackages[i].Packaging = objPackType;
                }

                objShipmentType.Package = aobjPackages;

                //CREATE LABEL
                ShipWebReference.LabelSpecificationType objLabelSpec = new ShipWebReference.LabelSpecificationType();
                ShipWebReference.LabelStockSizeType objLabelStockSize = new ShipWebReference.LabelStockSizeType();
                objLabelStockSize.Height = objShipment.LabelStockSizeHeight;
                objLabelStockSize.Width = objShipment.LabelStockSizeWidth;
                objLabelSpec.LabelStockSize = objLabelStockSize;
                ShipWebReference.LabelImageFormatType objLabelImageFormat = new ShipWebReference.LabelImageFormatType();
                objLabelImageFormat.Code = objShipment.LabelImageFormatCode;
                objLabelSpec.LabelImageFormat = objLabelImageFormat;

                //CREATE SHIPREQUEST
                ShipWebReference.RequestType objRequest = new ShipWebReference.RequestType();
                string[] aobjRequestOption = objShipment.RequestOption;
                objRequest.RequestOption = aobjRequestOption;
                ShipWebReference.ShipmentRequest objShipmentRequest = new ShipWebReference.ShipmentRequest();
                objShipmentRequest.Request = objRequest;
                objShipmentRequest.LabelSpecification = objLabelSpec;
                objShipmentRequest.Shipment = objShipmentType;

                //CREATE WEB SERVICE
                ShipWebReference.ShipService objShpSvc = new ShipWebReference.ShipService();
                ShipWebReference.UPSSecurity objUpss = new ShipWebReference.UPSSecurity();
                ShipWebReference.UPSSecurityServiceAccessToken objUpssSvcAccessToken = new ShipWebReference.UPSSecurityServiceAccessToken();
                objUpssSvcAccessToken.AccessLicenseNumber = objShipment.AccessLicenseNumber;
                objUpss.ServiceAccessToken = objUpssSvcAccessToken;
                ShipWebReference.UPSSecurityUsernameToken objUpssUsrNameToken = new ShipWebReference.UPSSecurityUsernameToken();
                objUpssUsrNameToken.Username = objShipment.Username;
                objUpssUsrNameToken.Password = objShipment.Password;
                objUpss.UsernameToken = objUpssUsrNameToken;
                objShpSvc.UPSSecurityValue = objUpss;

                //GET RESPONSE
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Sending Shipment object to UPS");
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": ShipToName: " + objShipment.ShipToName);
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": ShipToAttentionName: " + objShipment.ShipToAttentionName);
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": ShipToAddressLine: " + objShipment.ShipToAddressLine[0]);
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": ShipperPostalCode: " + objShipment.ShipperPostalCode);
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": ShipToCity: " + objShipment.ShipToCity);
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": ShipToStateProvinceCode: " + objShipment.ShipToStateProvinceCode);
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": ShipToCountryCode: " + objShipment.ShipToCountryCode);
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": ShipToServiceType: " + objShipment.ServiceTypeName);

                ShipWebReference.ShipmentResponse objShipmentResponse = objShpSvc.ProcessShipment(objShipmentRequest);

                if (objShipmentResponse != null)
                {
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Got a response! The transaction was a " + objShipmentResponse.Response.ResponseStatus.Description);
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": The 1Z number of the new shipment is " + objShipmentResponse.ShipmentResults.ShipmentIdentificationNumber);
                }

                return objShipmentResponse;
            }
            catch (System.Web.Services.Protocols.SoapException objException)
            {
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": ---------\"Hard\" is USER ERROR \"Transient\" is SYSTEM ERROR----------------");
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": SoapException Message= " + objException.Message);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": SoapException Category:Code:Message= " + objException.Detail.LastChild.InnerText);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": SoapException XML String for all= " + objException.Detail.LastChild.OuterXml);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": SoapException StackTrace= " + objException.StackTrace);
                return null;
            }
            catch (System.ServiceModel.CommunicationException objException)
            {
                Logger.Instance.LogEx(TraceEventType.Critical, 8888, objException, _strAssembly + ":" + strMethod + ": UPS response CommunicationException");
                return null;
            }
            catch (Exception objException)
            {
                Logger.Instance.LogEx(TraceEventType.Critical, 8888, objException, _strAssembly + ":" + strMethod + ": UPS response Exception!");
                return null;
            }
        }

        private void PrintLabels(Shipment objShipment)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Can't print lables because the instance for the result of the web serivce is null!");

            try
            {
                ShipmentTools.DeleteFiles(_strDownloadPath, "*.html");
                ShipmentTools.DeleteFiles(_strDownloadPath, "*.gif");

                string strFilename = string.Empty;
                string strTrackingnumber = string.Empty;
                string strTrackingnumbers = string.Empty;
                string strImage64 = string.Empty;
                byte[] abyteImage64Binary;

                if (objShipment.CODFundsCode != null & objShipment.CODFundsCode != "0")
                {
                    if (objShipment.Response == null || objShipment.Response.ShipmentResults.PackageResults.Length == 0)
                        return;

                    //save gif file
                    for (int i = 0; i < objShipment.Packages; i++)
                    {
                        strTrackingnumber = objShipment.Response.ShipmentResults.PackageResults[i].TrackingNumber;
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Printing COD for " + strTrackingnumber);

                        if (i == 0)
                        {
                            //get label gif
                            strImage64 = objShipment.Response.ShipmentResults.PackageResults[i].ShippingLabel.GraphicImage;
                            abyteImage64Binary = Convert.FromBase64String(strImage64);
                            strFilename = _strDownloadPath + "label" + strTrackingnumber + ".gif";
                            File.WriteAllBytes(strFilename, abyteImage64Binary);
                            objShipment.Gif_File = strFilename;

                            //get label html
                            strImage64 = objShipment.Response.ShipmentResults.PackageResults[i].ShippingLabel.HTMLImage;
                            abyteImage64Binary = Convert.FromBase64String(strImage64);
                            strFilename = _strDownloadPath + "page" + strTrackingnumber + ".html";
                            File.WriteAllBytes(strFilename, abyteImage64Binary);
                            objShipment.Label_HtmlFile = strFilename;

                            //get turnin html
                            strImage64 = objShipment.Response.ShipmentResults.CODTurnInPage.Image.GraphicImage;
                            abyteImage64Binary = Convert.FromBase64String(strImage64);
                            strFilename = _strDownloadPath + "turnin" + strTrackingnumber + ".html";
                            File.WriteAllBytes(strFilename, abyteImage64Binary);
                            objShipment.TurnIn_HtmlFile = strFilename;

                            //print the first page to laser printer
                            bool bDesign = SettingController.LLDesign.Equals("1") ? true : false;
                            bool bPreview = SettingController.LLPreview.Equals("1") ? true : false;

                            if (SettingController.PrintBarcode.Equals("1"))
                                _objLL.DesignLLHtml(SettingController.FileLLPic, objShipment.Gif_File, bDesign, bPreview);

                            if (SettingController.PrintTurnIn.Equals("1"))
                                _objLL.DesignLLHtml(SettingController.FileLLHtml, objShipment.TurnIn_HtmlFile, bDesign, bPreview);

                            strTrackingnumbers = strTrackingnumber;
                        }
                        else
                        {
                            //Print the others as to thermal printer
                            strImage64 = objShipment.Response.ShipmentResults.PackageResults[i].ShippingLabel.GraphicImage;
                            abyteImage64Binary = Convert.FromBase64String(strImage64);
                            string result = ASCIIEncoding.UTF8.GetString(abyteImage64Binary);
                            PrintDirectToLabel.PrintRaw(objShipment.LabelPrinter, result, strTrackingnumber, Logger.Instance);
                            strTrackingnumbers += "," + strTrackingnumber;
                        }

                    }
                }
                else if (objShipment.CODFundsCode != null & objShipment.CODFundsCode == "0")
                {
                    for (int i = 0; i < objShipment.Packages; i++)
                    {
                        //Print onyl EPL Label
                        strTrackingnumber = objShipment.Response.ShipmentResults.PackageResults[i].TrackingNumber;
                        if (strTrackingnumbers.Length == 0)
                            strTrackingnumbers = strTrackingnumber;
                        else
                            strTrackingnumbers += "," + strTrackingnumber;

                        strImage64 = objShipment.Response.ShipmentResults.PackageResults[i].ShippingLabel.GraphicImage;
                        abyteImage64Binary = Convert.FromBase64String(strImage64);
                        string result = ASCIIEncoding.UTF8.GetString(abyteImage64Binary);
                        PrintDirectToLabel.PrintRaw(objShipment.LabelPrinter, result, strTrackingnumber, Logger.Instance);

                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Printing Label for " + strTrackingnumber);

                    }
                }

                //Last step: Save all trackingnumbers back to database!
                SaveTrackingnumbers(strTrackingnumbers);
            }
            catch (Exception objException)
            {
                if(DeleteAllShipments(objShipment.Response))
                    new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during printing! Trackingnumbers ARE canceled. Please try again!");
                else
                    new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during printing! Trackingnumbers NOT canceled. Please try again!");
            }
        }

        private void Initialize()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                _strCurrentPath = System.IO.Path.GetFullPath(@".\");
                _strDownloadFolder = SettingController.DownloadFolder;
                _strDownloadPath = Path.Combine(_strCurrentPath, _strDownloadFolder);

                if (!Directory.Exists(_strDownloadPath))
                {
                    System.IO.Directory.CreateDirectory(_strDownloadPath);
                    Logger.Instance.Log(TraceEventType.Information, 0, "Folder " + _strDownloadPath + " created");
                }

                _strDownloadPath += @"\";

                _strLaserPrinter = SettingController.LaserPrinter;
                _strLabelPrinter = SettingController.LabelPrinter;
                _strPackageTypeCode = SettingController.PackageTypeCode;
                _strServiceNotificationCode = SettingController.ServiceNotificationCode;
                _strServiceDescription = SettingController.ServiceDescription;
                _strCODAmountCurrencyCode = SettingController.CODAmountCurrencyCode;
                _strShipUnitOfMeasurementTypeCode = SettingController.ShipUnitOfMeasurementTypeCode;
                _strShipUnitOfMeasurementTypeDescription = SettingController.ShipUnitOfMeasurementTypeDescription;
                _strLabelImageFormatCode = SettingController.LabelImageFormatCode;
                _strLabelStockSizeHeight = SettingController.LabelStockSizeHeight;
                _strLabelStockSizeWidth = SettingController.LabelStockSizeWidth;
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during controller initialisation!");
            }
        }

        private void SaveTrackingnumbers(string strTrackingnumbers)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                var objOrder = _objDbController.SaveTrackingnumbers(strTrackingnumbers, _strOrderNr);
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": " + objOrder.trackingnumbers.Replace(", ", "-") + " has been saved to database");

                string[] astrTNR = strTrackingnumbers.Split(new char[] { ',' });

                if (astrTNR.Length > 0)
                {
                    string strNewFilename = Logger.Instance.GetLogDir();
                    strNewFilename += "UPS_SHIP_" + objOrder.trackingnumbers.Replace(",", "-") + "-" + astrTNR.Length + "_" + objOrder.Belegnummer + "_" + objOrder.Kundennummer + ".txt";
                    Logger.Instance.RenameLogfile(strNewFilename);
                }
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during saving trackingnumbers!");
            }
        }

        private void RemoveTrackingnumber(string strTrackingnumber)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                string strNewFilename = string.Empty;

                var objOrder = _objDbController.RemoveTrackingnumber(strTrackingnumber);

                if (objOrder == null)
                {
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Trackingnumber not found in database!");

                    strNewFilename = Logger.Instance.GetLogDir();
                    strNewFilename += "DELETE_" + strTrackingnumber + ".txt";
                    Logger.Instance.RenameLogfile(strNewFilename);

                    return;
                }

                strNewFilename = Logger.Instance.GetLogDir();
                strNewFilename += "DELETE_" + strTrackingnumber + "_" + objOrder.Belegnummer + "_" + objOrder.Kundennummer + ".txt";
                Logger.Instance.RenameLogfile(strNewFilename);
            }
            catch (Exception objException)
            {
                new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Error during removing trackingnumbers!");
            }
        }

        private bool DeleteAllShipments(ShipWebReference.ShipmentResponse objShipResponse)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                for (int i = 0; i < objShipResponse.ShipmentResults.PackageResults.Length; i++)
                {                  
                    string strTrackingnumber = objShipResponse.ShipmentResults.PackageResults[i].TrackingNumber;
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Starting deleting for " + strTrackingnumber);
 
                    if(!DeleteShipment(strTrackingnumber))
                        Logger.Instance.Log(TraceEventType.Error, 7777, _strAssembly + ":" + strMethod + ": Delete for " + strTrackingnumber + " failed!");                    
                    else
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Delete for " + strTrackingnumber + " succeeded!");
                }

                return true;
            }
            catch (Exception objException)
            {
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": " + objException.Message);
                Logger.Instance.Log(TraceEventType.Critical, 8888, _strAssembly + ":" + strMethod + ": " + objException.InnerException.ToString());
                return false;
            }
        }

        #endregion
    }
}
