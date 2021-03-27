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
    public class ShipmentRequestBuilderOld : ITraceable
    {
        private string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;
        private string componentName;
        private bool isTraceRequestEnabled;
        private bool isTraceResponseEnabled;

        private Shipment _objShipment;
        private DatabaseController _objDbController;
        private SWSServicePortTypeClient _objWebService;
        private AuthentificationType _objAuthentication;

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

        public ShipmentRequestBuilderOld(DatabaseController objDbController, Shipment objShipment)
        {
            _objShipment = objShipment;
            _objDbController = objDbController;
            InitRequest();
        }

        public ShipmentRequestBuilderOld(Shipment objShipment)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": objShipment is NULL!");

            _objShipment = objShipment;

            if (objShipment.InvoiceNumber.Length < 6 && !objShipment.InvoiceNumber.Equals("0"))
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": InvoiceNumber is lower than 5 characters!");

            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Belegnummer = " + objShipment.InvoiceNumber);

            _objDbController = new DatabaseController();

            InitRequest();
        }

        #region Public methods

        public bool CreateShipment()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                int iNumRetriesAllowed = ShipmentTools.SafeParseInt(SettingController.Retries);
                if (iNumRetriesAllowed < 2)
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Retry count is lower than 2!");
                    return false;
                }

                CreateShipmentDDRequest objRequest = CreateShipmentDDRequest();
                CreateShipmentResponse objResponse = null;

                for (int i = 0; i <= iNumRetriesAllowed; i++)
                {
                    objResponse = _objWebService.createShipmentDD(_objAuthentication, objRequest);

                    if (objResponse != null && objResponse.status.StatusCode.Equals("0"))
                    {
                        _objShipment.Trackingnumber = objResponse.CreationState[0].ShipmentNumber.Item;

                        for (int iUrl = 0; iUrl < objResponse.CreationState.Length; iUrl++)
                        {
                            string strLabelUrl = objResponse.CreationState[iUrl].Labelurl;
                            string strFile = Path.Combine(SettingController.DownloadFolder, iUrl.ToString() + "_" + _objShipment.Trackingnumber + "_" + SettingController.DHL_PDF_Filename);
                            ShipmentTools.Download(strLabelUrl, strFile);
                            _objShipment.DownloadedFiles.Add(strFile);
                        }

                        GetExportDocumentDD();

                        if (!_objShipment.ExecutedByShipmentTests)
                        {
                            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Saving Trackingnumber ...");
                            SaveTrackingnumbers();
                        }
                        return true;
                    }
                    else
                    {
                        Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);
                        throw new ShipmentException(null, ReadableException(objResponse, _strAssembly));
                    }
                }

                if (objResponse != null)
                    Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);
                else
                    Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : objResponse = null");

                throw new ShipmentException(null, _strAssembly + ":" + strMethod + ": Create Shipping with Retry FAILED!!");
            }
            catch (Exception objException)
            {
                throw new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during executing CREATEDD request to DHL service!");
            }
        }

        public List<ManifestedDetail> DoAndPrintManifestDD(int iDay)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var cobjBelege = _objDbController.GetShippedOrders(DateTime.Today.AddDays(iDay), DateTime.Today.AddDays(iDay), "DHL");
            if (cobjBelege != null && cobjBelege.Count <= 0)
                throw new ShipmentException(null, _strAssembly + ":" + strMethod + ": No Orders found to manifest for today!");

            List<string> cstrTrackingnumbers = cobjBelege.Select(a => a.Trackingnumbers).ToList<string>();

            if (cstrTrackingnumbers == null || cstrTrackingnumbers.Count == 0)
                throw new ShipmentException(null, _strAssembly + ":" + strMethod + ": No Trackingnumbers are given!");

            DoManifestDDRequest objRequest = DoManifestDDRequest(cstrTrackingnumbers.ToArray());

            List<ManifestedDetail> cobjManifests = new List<ManifestedDetail>();
            try
            {
                DoManifestResponse objResponse = _objWebService.doManifestDD(_objAuthentication, objRequest);

                if (objResponse != null && objResponse.Status.StatusCode.Equals("0"))
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Manifested " + objResponse.ManifestState.Length + " of " + cstrTrackingnumbers.Count);

                    foreach (var objState in objResponse.ManifestState)
                    {
                        if (objState.Status.Equals("0"))
                            Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": OK    Trackingnumber " + objState.ShipmentNumber.Item + " manifested!");
                        else
                            Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": ERROR Trackingnumber " + objState.ShipmentNumber.Item + " > " + objState.Status.StatusCode + "/" + objState.Status.StatusMessage);

                        cobjManifests.Add(new ManifestedDetail(objState.ShipmentNumber.Item, objState.Status.StatusCode, objState.Status.StatusMessage));
                    }

                }
                else
                {
                    if (objResponse == null)
                        throw new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during Manifest Data with objResponse = null!");
                    else
                        throw new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during Manifest Data with Code/Error " + objResponse.Status.StatusCode + "/" + objResponse.Status.StatusMessage);
                }

                return cobjManifests;
            }
            catch (Exception objException)
            {
                throw new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during manifesting!");
            }
        }

        public string GetAndPrintManifestDD(DateTime objDateBegin, DateTime objDateEnd)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            GetManifestDDRequest objRequest;

            if (objDateEnd == default(DateTime))
                objRequest = GetManifestDDRequest(objDateBegin);
            else
                objRequest = GetManifestDDRequest(objDateBegin, objDateEnd);

            try
            {
                GetManifestDDResponse objResponse = _objWebService.getManifestDD(_objAuthentication, objRequest);

                if (objResponse != null && objResponse.status.StatusCode.Equals("0"))
                {
                    if (objResponse.ManifestPDFData.Length > 0)
                    {
                        string strExportDocPDFData = objResponse.ManifestPDFData;
                        var abyteExportDocPDFData = Convert.FromBase64String(strExportDocPDFData);
                        string strFile = Path.Combine(SettingController.DownloadFolder, SettingController.DHL_Manifest_Filename);
                        File.WriteAllBytes(strFile, abyteExportDocPDFData);
                        Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Manifest Data downloaded and saved as " + strFile);
                        return strFile;
                    }
                    else
                    {
                        throw new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data with Code/Error " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);
                    }
                }
                else
                {
                    if (objResponse == null)
                        throw new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data with objResponse = null!");
                    else
                        throw  new ShipmentException(null, _strAssembly + ":" + strMethod + ": Exception during downloading Manifest Data with Code/Error " + objResponse.status.StatusCode + "/" + objResponse.status.StatusMessage);
                }
            }
            catch (Exception objException)
            {
                throw new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during downloading Export Document!");
            }
        }

        public bool DeleteShipmentDDRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            DeleteShipmentDDRequest objRequest = GetDeleteShipmentDDRequest(_objShipment.Trackingnumber);

            try
            {
                DeleteShipmentResponse objResponse = _objWebService.deleteShipmentDD(_objAuthentication, objRequest);

                if (objResponse == null)
                    throw new ShipmentException(null, _strAssembly + ":" + strMethod + ": Delete request failed for trackingnumber " + _objShipment.Trackingnumber + " because objResponse = null!");

                Statusinformation status = objResponse.Status;
                string statusMessage = status.StatusMessage;
                DeletionState[] aobjStates = objResponse.DeletionState;

                foreach (DeletionState objState in aobjStates)
                {
                    Logger.Instance.Log(TraceEventType.Error, 0, _strAssembly + ":" + strMethod + ": Status of Trackingnumber was " + objState.Status.StatusCode + "/" + objState.Status.StatusMessage);

                    if (objState.Status.StatusCode.Equals("0"))
                        RemoveTrackingnumber();
                }

                return true;
            }
            catch (Exception objException)
            {
                throw new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during executing DELETEDD request to DHL service!");
            }
        }

        #endregion

        #region Private methods

        private void GetExportDocumentDD()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (ShipmentTools.IsEU(_objShipment.ReceiverCountryCode))
                return;

            GetExportDocDDRequest objRequest = GetExportDocDDRequest();

            try
            {
                GetExportDocResponse objResponse = _objWebService.getExportDocDD(_objAuthentication, objRequest);

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

        private void RemoveTrackingnumber()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            try
            {
                var objOrder = _objDbController.RemoveTrackingnumber(_objShipment.Trackingnumber);

                if (objOrder == null)
                    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Trackingnumber " + _objShipment.Trackingnumber + " not found in database!");
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

        private string ReadableException(CreateShipmentResponse objResult, string strMethod)
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

        private void InitRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            _objWebService = new SWSServicePortTypeClient("ShipmentServiceSOAP11port0");

            _objWebService.Endpoint.Address = new System.ServiceModel.EndpointAddress(_objShipment.Endpoint);
            System.ServiceModel.BasicHttpBinding objBinding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport);
            objBinding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic;
            objBinding.MaxReceivedMessageSize = 10*1024*1024;
            _objWebService.Endpoint.Binding = objBinding;

            _objAuthentication = new AuthentificationType();
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

        private CreateShipmentDDRequest CreateShipmentDDRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipmentOrderDDTypeShipment objShipment = new ShipmentOrderDDTypeShipment();
            objShipment.ShipmentDetails = CreateShipmentDetailsDDType(_objShipment.PackageCount);

            ShipperDDType objShipper = new ShipperDDType();
            objShipper.Company = CreateShipperCompany();
            objShipper.Address = CreateShipperNativeAddressType();
            objShipper.Communication = CreateShipperCommunicationType();
            objShipment.Shipper = objShipper;

            ReceiverDDType objReceiver = new ReceiverDDType();
            objReceiver.Company = CreateReceiverCompany();
            objReceiver.Item = CreateReceiverNativeAddressType();
            objReceiver.Communication = CreateReceiverCommunicationType();
            objShipment.Receiver = objReceiver;

            if(((NativeAddressType)objReceiver.Item).Zip.ItemElementName == ItemChoiceType6.other)
            {
                var objResult = CreateExportDocDDType();
                if(objResult != null)
                    objShipment.ExportDocument = objResult;
            }

            ShipmentOrderDDType objOrder = new ShipmentOrderDDType();
            objOrder.SequenceNumber = "1";
            objOrder.LabelResponseType = ShipmentOrderDDTypeLabelResponseType.URL;
            objOrder.Shipment = objShipment;

            ShipmentOrderDDType[] aOrder = new ShipmentOrderDDType[] { objOrder };

            CreateShipmentDDRequest objRequest = new CreateShipmentDDRequest();
            objRequest.ShipmentOrder = aOrder;
            objRequest.Version = CreateVersion();
            return objRequest;
        }

        private NameType CreateShipperCompany()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            NameTypeCompany company = new NameTypeCompany();
            company.name1 = _objShipment.ShipperCompanyName;

            NameType name = new NameType();
            name.Item = company;
            return name;
        }

        private NativeAddressType CreateShipperNativeAddressType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            NativeAddressType objAddress = new NativeAddressType();
            objAddress.streetName = _objShipment.ShipperStreet;
            objAddress.streetNumber = _objShipment.ShipperStreetNr;
            objAddress.city = _objShipment.ShipperCity;

            ZipType objZip = new ZipType();
            objZip.ItemElementName = ItemChoiceType6.germany;
            objZip.Item = _objShipment.ShipperZip;
            objAddress.Zip = objZip;

            CountryType objOrigin = new CountryType();
            objOrigin.countryISOCode = _objShipment.ShipperCountryCode;
            objAddress.Origin = objOrigin;

            return objAddress;
        }

        private NativeAddressType CreateReceiverNativeAddressType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            NativeAddressType address = new NativeAddressType();
            ZipType zip = new ZipType();
            CountryType origin = new CountryType();

            address.streetName = _objShipment.ReceiverStreet;
            address.streetNumber = _objShipment.ReceiverStreentNr;
            address.city = _objShipment.ReceiverCity;

            if (_objShipment.ReceiverCountryCode.Equals("DE"))
                zip.ItemElementName = ItemChoiceType6.germany;
            else if (_objShipment.ReceiverCountryCode.Equals("GB"))
                zip.ItemElementName = ItemChoiceType6.england;

            else
                zip.ItemElementName = ItemChoiceType6.other;

            zip.Item = _objShipment.ReceiverZip;
            //origin.country = _objShipment.ReceiverCountry;
            origin.countryISOCode = _objShipment.ReceiverCountryCode;
            address.Zip = zip;
            address.Origin = origin;

            return address;
        }

        private CommunicationType CreateShipperCommunicationType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            CommunicationType objCommunication = new CommunicationType();
            objCommunication.email = _objShipment.ShipperEMail;
            objCommunication.contactPerson = _objShipment.ShipperName;
            objCommunication.phone = _objShipment.ShipperPhone;
            return objCommunication;
        }
        
        private CommunicationType CreateReceiverCommunicationType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            CommunicationType objCommunication = new CommunicationType();
            objCommunication.email = _objShipment.ReceiverContactEMail;
            objCommunication.contactPerson = _objShipment.ReceiverContactName;
            objCommunication.phone = _objShipment.ReceiverContactPhone;
            return objCommunication;
        }
        
        private NameType CreateReceiverCompany()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            NameType objName = new NameType();
            NameTypeCompany objCompany = new NameTypeCompany();
            objCompany.name1 = _objShipment.ReceiverCompanyName;
            objCompany.name2 = string.Format("{0} {1}", _objShipment.ReceiverFirstName, _objShipment.ReceiverLastName);
            objName.Item = objCompany;
            return objName;
        }
        
        private ShipmentDetailsDDType CreateShipmentDetailsDDType(int iCountOfPackages)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipmentDetailsDDType objShipmentDetails = new ShipmentDetailsDDType();
            objShipmentDetails.ProductCode = _objShipment.DDProdCode;
            objShipmentDetails.EKP = _objShipment.EKP;
            objShipmentDetails.ShipmentDate = DateTime.Today.ToString(_objShipment.SDF).MaxLength(10);

            if (_objShipment.CODAmount > 0)
            {
                objShipmentDetails.BankData = CreateShipperBankData();
                objShipmentDetails.DeclaredValueOfGoods = (float)_objShipment.CODAmount;
                objShipmentDetails.DeclaredValueOfGoodsCurrency = _objShipment.CODCurrency;
            }

            ShipmentDetailsDDTypeAttendance objAttendance = new ShipmentDetailsDDTypeAttendance();
            objAttendance.partnerID = _objShipment.GetPartnerCode();

            if (objAttendance.partnerID.Equals(string.Empty))
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Shipment could not be created! PartnerID is empty!");

            objShipmentDetails.Attendance = objAttendance;

            List<ShipmentServiceDD> cobjShipmentServices = new List<ShipmentServiceDD>();

            //COD Service
            if (_objShipment.CODAmount > 0)
            {
                DDServiceGroupOtherTypeCOD objCOD = new DDServiceGroupOtherTypeCOD();
                objCOD.CODAmount = _objShipment.CODAmount;
                objCOD.CODCurrency = _objShipment.CODCurrency;

                DDServiceGroupOtherType objServiceOther = new DDServiceGroupOtherType();
                objServiceOther.ItemElementName = ItemChoiceType5.COD;
                objServiceOther.Item = objCOD;

                ShipmentServiceDD objService = new ShipmentServiceDD();
                objService.Item = objServiceOther;

                cobjShipmentServices.Add(objService);
            }

            //Multipack Service
            if(iCountOfPackages > 1)
            {
                DDServiceGroupDHLPaketType objServiceDHL = new DDServiceGroupDHLPaketType();
                objServiceDHL.ItemElementName = ItemChoiceType4.Multipack;
                objServiceDHL.Item = true;

                ShipmentServiceDD objService = new ShipmentServiceDD();
                objService.Item = objServiceDHL;

                cobjShipmentServices.Add(objService);
            }

            if (cobjShipmentServices.Count > 0)
                objShipmentDetails.Service = cobjShipmentServices.ToArray();

            objShipmentDetails.ShipmentItem = new ShipmentItemDDType[iCountOfPackages];

            for (int i = 0; i < iCountOfPackages; i++)
            {
                objShipmentDetails.ShipmentItem[i] = CreateShipmentItemDDType(i);
            }
            objShipmentDetails.Description = _objShipment.ShipmentDesc;

            return objShipmentDetails;
        }

        private BankType CreateShipperBankData()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            BankType objBankData = new BankType()
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

        private ShipmentItemDDType CreateShipmentItemDDType(int iIndex)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            ShipmentItemDDType objShipmentItem = new ShipmentItemDDType();
            Package objItemInfo = _objShipment.Packages[iIndex];
            objShipmentItem.WeightInKG = decimal.Parse(objItemInfo.WeightInKG);
            objShipmentItem.LengthInCM = objItemInfo.LengthInCM;
            objShipmentItem.WidthInCM = objItemInfo.WidthInCM;
            objShipmentItem.HeightInCM = objItemInfo.HeightInCM;
            objShipmentItem.PackageType = objItemInfo.PackageType;
            return objShipmentItem;
        }

        private ExportDocumentDDType CreateExportDocDDType()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            if (_objShipment.ItemPositions.Count <= 0)
                return null;

            List<ExportDocumentDDTypeExportDocPosition> aobjExportDocumentPosition = new List<ExportDocumentDDTypeExportDocPosition>();

            foreach (var objPos in _objShipment.ItemPositions)
            {
                ExportDocumentDDTypeExportDocPosition objExportDocumentPosition = new ExportDocumentDDTypeExportDocPosition();
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

            ExportDocumentDDType objExportDocument = new ExportDocumentDDType();
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
            objExportDocument.InvoiceType = ExportDocumentDDTypeInvoiceType.commercial;
            objExportDocument.InvoiceDate = _objShipment.ExportDocumentInvoiceDate;
            objExportDocument.InvoiceNumber = _objShipment.ExportDocumentInvoiceNumber;
            objExportDocument.ExportType = ExportDocumentDDTypeExportType.Item0;
            objExportDocument.ExportTypeSpecified = true;
            objExportDocument.InvoiceTypeSpecified = true;

            return objExportDocument;
        }
               
        private GetLabelDDRequest GetLabelDDRequest(string strShipmentId)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            GetLabelDDRequest ddRequest = new GetLabelDDRequest();
            ddRequest.Version = CreateVersion();
            ShipmentNumberType shNumber = new ShipmentNumberType();
            shNumber.ItemElementName = ItemChoiceType7.shipmentNumber;
            shNumber.Item = strShipmentId;

            ShipmentNumberType[] shNumbers = new ShipmentNumberType[1];
            shNumbers[0] = shNumber;
            ddRequest.ShipmentNumber = shNumbers;
            return ddRequest;
        }

        private DeleteShipmentDDRequest GetDeleteShipmentDDRequest(string strShipmentId)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            DeleteShipmentDDRequest ddRequest = new DeleteShipmentDDRequest();
            ddRequest.Version = CreateVersion();
            ShipmentNumberType shNumber = new ShipmentNumberType();
            shNumber.ItemElementName = ItemChoiceType7.shipmentNumber;
            shNumber.Item = strShipmentId;

            ShipmentNumberType[] shNumbers = new ShipmentNumberType[1];
            shNumbers[0] = shNumber;
            ddRequest.ShipmentNumber = shNumbers;
            return ddRequest;
        }

        private DoManifestDDRequest DoManifestDDRequest(string[] astrTrackingnumbers)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            DoManifestDDRequest objRequest = new DoManifestDDRequest();
            objRequest.Version = CreateVersion();

            List<ShipmentNumberType> cobjShipmentNumberTypes = new List<ShipmentNumberType>();

            foreach(string strItem in astrTrackingnumbers)
            {
                cobjShipmentNumberTypes.Add(new ShipmentNumberType()
                {
                    Item = strItem,
                    ItemElementName = ItemChoiceType7.shipmentNumber
                });
            }

            objRequest.ShipmentNumber = cobjShipmentNumberTypes.ToArray();

            return objRequest;
        }

        private GetManifestDDRequest GetManifestDDRequest(DateTime objDate)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            GetManifestDDRequest objRequest = new GetManifestDDRequest();
            objRequest.Version = CreateVersion();

            objRequest.Item = objDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            return objRequest;
        }

        private GetManifestDDRequest GetManifestDDRequest(DateTime objFromDate, DateTime objToDate)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            GetManifestDDRequest objRequest = new GetManifestDDRequest();
            objRequest.Version = CreateVersion();

            objRequest.Item = new GetManifestDDRequestManifestDateRange()
            {
                fromDate = objFromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                toDate = objToDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };

            return objRequest;
        }

        private GetExportDocDDRequest GetExportDocDDRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            GetExportDocDDRequest objRequest = new GetExportDocDDRequest();
            objRequest.Version = CreateVersion();

            ShipmentNumberType objShipmentNumber = new ShipmentNumberType()
            {
                Item = _objShipment.Trackingnumber,
                ItemElementName = ItemChoiceType7.shipmentNumber
            };

            objRequest.ShipmentNumber = new ShipmentNumberType[] { objShipmentNumber };

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
