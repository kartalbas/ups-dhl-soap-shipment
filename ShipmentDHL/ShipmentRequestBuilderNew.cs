using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.IO;
using ShipmentLib;
using ShipmentLib.SoapDumper;
using ShipmentDHL.ShipWebReference3;

namespace ShipmentDHL
{
    public class ShipmentRequestBuilderNew : ITraceable
    {
        private Shipment _objShipment;
        private GKV3XAPIServicePortTypeClient _objWebService;
        private AuthentificationType _objAuthentication;
        private DatabaseController _objDbController;

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

        public ShipmentRequestBuilderNew(DatabaseController objDbController, Shipment objShipment)
        {
            _objShipment = objShipment;
            _objDbController = objDbController;
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

                CreateShipmentOrderRequest objRequest = CreateShipmentOrderRequest(_objShipment.PackageCount);
                CreateShipmentOrderResponse objResponse = null;

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
                            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Saving Trackingnumber ...");
                            SaveTrackingnumbers();
                        }
                        return true;
                    }
                    else
                    {

                        Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : " + objResponse.Status.statusCode + "/" + string.Join(",", objResponse.Status.statusMessage));

                        throw new ShipmentException(null, ReadableException(objResponse, _strAssembly));
                    }
                }

                if(objResponse != null)
                    Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : " + objResponse.Status.statusCode + "/" + string.Join(",", objResponse.Status.statusMessage));
                else
                    Logger.Instance.Log(TraceEventType.Error, 9999, _strAssembly + ":" + strMethod + ": Request end with Error : objResponse = null" );

                throw new ShipmentException(null, _strAssembly + ":" + strMethod + ": Create Shipping with Retry FAILED!!");
            }
            catch (Exception objException)
            {
                throw new ShipmentException(objException, _strAssembly + ":" + strMethod + ": Exception during executing CREATEDD request to DHL service!");
            }
        }

        #endregion

        #region Private methods

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

        public static string ReadableException(CreateShipmentOrderResponse objResult, string strMethod)
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

        private void InitRequest()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (_objShipment == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": _objShipment is NULL!");

            _objWebService = new GKV3XAPIServicePortTypeClient("ShipmentServiceSOAP11port0");
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
                product = _objShipment.DDProdCode,
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
            GetManifestRequest objRequest = new GetManifestRequest();
            objRequest.Version = CreateVersion();
            objRequest.manifestDate = objDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return objRequest;
        }

        private ShipWebReference3.Version CreateVersion()
        {
            ShipWebReference3.Version version = new ShipWebReference3.Version();
            version.majorRelease = "3";
            version.minorRelease = "1";
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
            Package objItemInfo = _objShipment.Packages[index];
            shipmentItem.weightInKG = decimal.Parse(objItemInfo.WeightInKG);
            shipmentItem.lengthInCM = objItemInfo.LengthInCM;
            shipmentItem.widthInCM = objItemInfo.WidthInCM;
            shipmentItem.heightInCM = objItemInfo.HeightInCM;
            return shipmentItem;
        }

        #endregion
    }
}
