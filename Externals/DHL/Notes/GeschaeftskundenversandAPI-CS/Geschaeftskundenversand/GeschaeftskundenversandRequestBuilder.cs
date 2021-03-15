using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHL_WS_Test.GeschaeftskundenversandWS;

namespace DHL_WS_Test.Geschaeftskundenversand
{
    class GeschaeftskundenversandRequestBuilder
    {
        private static String SHIPPER_STREET = "Heinrich-Bruening-Str.";
        private static String SHIPPER_STREETNR = "7";
        private static String SHIPPER_CITY = "Bonn";
        private static String SHIPPER_ZIP = "53113";
        private static String SHIPPER_COUNTRY_CODE = "DE";
        private static String SHIPPER_CONTACT_EMAIL = "max@muster.de";
        private static String SHIPPER_CONTACT_NAME = "Max Muster";
        private static String SHIPPER_CONTACT_PHONE = "030244547777778";
        private static String SHIPPER_COMPANY_NAME = "Deutsche Post IT Brief GmbH";
        //	private static String ENCODING = "UTF8";
        private static String MAJOR_RELEASE = "1";
        private static String MINOR_RELEASE = "0";
        private static String SDF = "yyyy-MM-dd";
        private static String DD_PROD_CODE = "EPN";
        private static String TD_PROD_CODE = "WPX";
        private static String EKP = "5000000008";
        private static String PARTNER_ID = "01";
        private static String SHIPMENT_DESC = "Interessanter Artikel";
        private static String TD_SHIPMENT_REF = "DDU";
        private static float TD_VALUE_GOODS = 250;
        private static String TD_CURRENCY = "EUR";
        private static String TD_ACC_NUMBER_EXPRESS = "144405785";


        //Beispieldaten Für DD-Sendungen aus/nach Deutschland
        private static String RECEIVER_FIRST_NAME = "Kai";
        private static String RECEIVER_LAST_NAME = "Wahn";
        private static String RECEIVER_LOCAL_STREET = "Marktplatz";
        private static String RECEIVER_LOCAL_STREETNR = "1";
        private static String RECEIVER_LOCAL_CITY = "Stuttgart";
        private static String RECEIVER_LOCAL_ZIP = "70173";
        private static String RECEIVER_LOCAL_COUNTRY_CODE = "DE";

        //Beispieldaten Für TD-Sendungen weltweit
        private static String RECEIVER_WWIDE_STREET = "Chung Hsiao East Road.";
        private static String RECEIVER_WWIDE_STREETNR = "55";
        private static String RECEIVER_WWIDE_CITY = "Taipeh";
        private static String RECEIVER_WWIDE_ZIP = "100";
        private static String RECEIVER_WWIDE_COUNTRY = "Taiwan";
        private static String RECEIVER_WWIDE_COUNTRY_CODE = "TW";

        private static String RECEIVER_CONTACT_EMAIL = "kai@wahn.de";
        private static String RECEIVER_CONTACT_NAME = "Kai Wahn";
        private static String RECEIVER_CONTACT_PHONE = "+886 2 27781-8";
        private static String RECEIVER_COMPANY_NAME = "Klammer Company";
        private static String DUMMY_SHIPMENT_NUMBER = "0000000";

        private static String EXPORT_REASON = "Sale";
        private static String SIGNER_TITLE = "Director Asia Sales";
        private static String INVOICE_NUMBER = "200601xx417";
        private static String DUMMY_AIRWAY_BILL = "0000000000";

        public static DHL_WS_Test.GeschaeftskundenversandWS.Version createVersion()
        {
            DHL_WS_Test.GeschaeftskundenversandWS.Version version = new DHL_WS_Test.GeschaeftskundenversandWS.Version();
            version.majorRelease = MAJOR_RELEASE;
            version.minorRelease = MINOR_RELEASE;
            return version;
        }

        public static NativeAddressType createShipperNativeAddressType()
        {

            NativeAddressType address = new NativeAddressType();
            address.streetName = SHIPPER_STREET;
            address.streetNumber = SHIPPER_STREETNR;
            address.city = SHIPPER_CITY;
            ZipType zip = new ZipType();
            zip.ItemElementName = DHL_WS_Test.GeschaeftskundenversandWS.ItemChoiceType6.germany;
            zip.Item = SHIPPER_ZIP;
            address.Zip = zip;
            CountryType origin = new CountryType();
            origin.countryISOCode = SHIPPER_COUNTRY_CODE;
            address.Origin = origin;


            return address;
        }

        public static NativeAddressType createReceiverNativeAddressType(Boolean worldwide)
        {

            NativeAddressType address = new NativeAddressType();
            ZipType zip = new ZipType();
            CountryType origin = new CountryType();
            if (!worldwide)
            {
                address.streetName = RECEIVER_LOCAL_STREET;
                address.streetNumber = RECEIVER_LOCAL_STREETNR;
                address.city = RECEIVER_LOCAL_CITY;
                zip.ItemElementName = ItemChoiceType6.germany;
                zip.Item = RECEIVER_LOCAL_ZIP;
                origin.countryISOCode = RECEIVER_LOCAL_COUNTRY_CODE;
            }
            else
            {
                address.streetName = RECEIVER_WWIDE_STREET;
                address.streetNumber = RECEIVER_WWIDE_STREETNR;
                address.city = RECEIVER_WWIDE_CITY;
                zip.ItemElementName = ItemChoiceType6.other;
                zip.Item = RECEIVER_WWIDE_ZIP;
                origin.country = RECEIVER_WWIDE_COUNTRY;
                origin.countryISOCode = RECEIVER_WWIDE_COUNTRY_CODE;
            }
            address.Zip = zip;
            address.Origin = origin;
            return address;
        }


        public static CommunicationType createShipperCommunicationType()
        {
            CommunicationType communication = new CommunicationType();
            communication.email = SHIPPER_CONTACT_EMAIL;
            communication.contactPerson = SHIPPER_CONTACT_NAME;
            communication.phone = SHIPPER_CONTACT_PHONE;
            return communication;
        }


        public static CommunicationType createReceiverCommunicationType()
        {
            CommunicationType communication = new CommunicationType();
            communication.email = RECEIVER_CONTACT_EMAIL;
            communication.contactPerson = RECEIVER_CONTACT_NAME;
            communication.phone = RECEIVER_CONTACT_PHONE;
            return communication;
        }

        public static NameType createShipperCompany()
        {
            NameTypeCompany company = new NameTypeCompany();
            company.name1 = SHIPPER_COMPANY_NAME;

            NameType name = new NameType();
            name.Item = company;
            return name;
        }


        public static NameType createReceiverCompany(Boolean isPerson)
        {
            NameType name = new NameType();
            if (isPerson)
            {
                NameTypePerson person = new NameTypePerson();
                person.firstname = RECEIVER_FIRST_NAME;
                person.lastname = RECEIVER_LAST_NAME;
                name.Item = person;
            }
            else
            {
                NameTypeCompany company = new NameTypeCompany();
                company.name1 = RECEIVER_COMPANY_NAME;
                name.Item = company;
            }
            return name;
        }


        public static ShipmentDetailsDDType createShipmentDetailsDDType(
                int shipmentItemNb)
        {
            DateTime today = DateTime.Today;
            today.AddDays(2);

            ShipmentDetailsDDType shipmentDetails = new ShipmentDetailsDDType();
            shipmentDetails.ProductCode = DD_PROD_CODE;
            shipmentDetails.ShipmentDate = today.ToString(SDF);
            shipmentDetails.EKP = EKP;

            ShipmentDetailsDDTypeAttendance attendance = new ShipmentDetailsDDTypeAttendance();
            attendance.partnerID = PARTNER_ID;
            shipmentDetails.Attendance = attendance;
            ShipmentItemDDType[] shItemTypeArray = new ShipmentItemDDType[shipmentItemNb];
            shipmentDetails.ShipmentItem = shItemTypeArray;

            for (int i = 0; i < shipmentItemNb; i++)
            {
                shipmentDetails.ShipmentItem[i] = createDefaultShipmentItemDDType();
            }
            shipmentDetails.Description = SHIPMENT_DESC;

            return shipmentDetails;
        }


        public static ShipmentDetailsTDType createShipmentDetailsTDType(
                int shipmentItemNb)
        {
            DateTime today = DateTime.Today;
            today.AddDays(2);

            ShipmentDetailsTDType shipmentDetails = new ShipmentDetailsTDType();
            shipmentDetails.ProductCode = TD_PROD_CODE;
            shipmentDetails.ShipmentDate = today.ToString(SDF);
            ShipmentDetailsTDTypeAccount acc = new ShipmentDetailsTDTypeAccount();
            acc.accountNumberExpress = TD_ACC_NUMBER_EXPRESS;
            shipmentDetails.Account = acc;
            shipmentDetails.Dutiable = ShipmentDetailsTDTypeDutiable.Item1;
            shipmentDetails.DescriptionOfContent = SHIPMENT_DESC;

            ShipmentItemTDType[] shItems = new ShipmentItemTDType[shipmentItemNb];

            //ShipmentItems setzen
            for (int i = 0; i < shipmentItemNb; i++)
            {
                shItems[i] = createDefaultShipmentItemTDType();
            }

            shipmentDetails.ShipmentItem = shItems;

            shipmentDetails.ShipmentReference = TD_SHIPMENT_REF;
            shipmentDetails.DeclaredValueOfGoodsSpecified = true;
            shipmentDetails.DeclaredValueOfGoods = TD_VALUE_GOODS;
            shipmentDetails.DeclaredValueOfGoodsCurrency = TD_CURRENCY;

            return shipmentDetails;
        }


        public static ShipmentItemTDType createDefaultShipmentItemTDType()
        {
            ShipmentItemTDType shipmentItem = new ShipmentItemTDType();
            shipmentItem.WeightInKG = Decimal.Parse("3");
            shipmentItem.LengthInCM = "50";
            shipmentItem.WidthInCM = "30";
            shipmentItem.HeightInCM = "15";
            return shipmentItem;
        }


        public static ShipmentItemDDType createDefaultShipmentItemDDType()
        {
            ShipmentItemDDType shipmentItem = new ShipmentItemDDType();
            shipmentItem.WeightInKG = Decimal.Parse("3");
            shipmentItem.LengthInCM = "50";
            shipmentItem.WidthInCM = "30";
            shipmentItem.HeightInCM = "15";
            shipmentItem.PackageType = "PK";
            return shipmentItem;
        }


        public static ExportDocumentTDType createDefaultExportDocTDType(String date)
        {
            ExportDocumentTDType exportDoc = new ExportDocumentTDType();
            exportDoc.InvoiceType = ExportDocumentTDTypeInvoiceType.commercial;
            exportDoc.InvoiceDate = date;
            exportDoc.InvoiceNumber = INVOICE_NUMBER;
            exportDoc.ExportType = ExportDocumentTDTypeExportType.P;
            exportDoc.SignerTitle = SIGNER_TITLE;
            exportDoc.ExportReason = EXPORT_REASON;

            return exportDoc;
        }


        public static CreateShipmentDDRequest createDefaultShipmentDDRequest()
        {

            // create empty request
            CreateShipmentDDRequest createShipmentDDRequest = new CreateShipmentDDRequest();
            // set version element


            createShipmentDDRequest.Version = createVersion();
            // create shipment order object
            ShipmentOrderDDType shipmentOrderDDType = new ShipmentOrderDDType();


            shipmentOrderDDType.SequenceNumber = "1";

            ShipmentOrderDDTypeShipment shipment = new ShipmentOrderDDTypeShipment();
            shipmentOrderDDType.Shipment = shipment;
            shipment.ShipmentDetails = createShipmentDetailsDDType(1);

            ShipperDDType shipper = new ShipperDDType();

            shipper.Company = createShipperCompany();
            shipper.Address = createShipperNativeAddressType();
            shipper.Communication = createShipperCommunicationType();
            shipment.Shipper = shipper;

            ReceiverDDType receiver = new ReceiverDDType();

            receiver.Company = createReceiverCompany(true);
            receiver.Item = createReceiverNativeAddressType(false);
            receiver.Communication = createReceiverCommunicationType();

            shipment.Receiver = receiver;

            shipmentOrderDDType.LabelResponseType = ShipmentOrderDDTypeLabelResponseType.URL;

            ShipmentOrderDDType[] shOrder = new ShipmentOrderDDType[1];

            // Shipment Order zum Request hinzufügen
            shOrder[0] = shipmentOrderDDType;
            createShipmentDDRequest.ShipmentOrder = shOrder;
            return createShipmentDDRequest;
        }


        public static CreateShipmentTDRequest createDefaultShipmentTDRequest()
        {
            DateTime today = DateTime.Today;

            // Leeres Request erstellen
            CreateShipmentTDRequest createShipmentTDRequest = new CreateShipmentTDRequest();
            // set version element
            createShipmentTDRequest.Version = createVersion();
            // create shipment order object
            ShipmentOrderTDType shipmentOrderTDType = new ShipmentOrderTDType();
            shipmentOrderTDType.SequenceNumber = "1";
            ShipmentOrderTDTypeShipment shipment = new ShipmentOrderTDTypeShipment();

            shipment.ShipmentDetails = GeschaeftskundenversandRequestBuilder.createShipmentDetailsTDType(1);
            shipment.ExportDocument = createDefaultExportDocTDType(today.ToString(SDF));
            ShipperTDType shipper = new ShipperTDType();

            shipment.Shipper = shipper;
            shipper.Company = createShipperCompany();
            shipper.Address = createShipperNativeAddressType();
            shipper.Communication = createShipperCommunicationType();

            ReceiverTDType receiver = new ReceiverTDType();
            shipment.Receiver = receiver;
            receiver.Company = createReceiverCompany(true);
            receiver.Item = createReceiverNativeAddressType(true);
            receiver.Communication = createReceiverCommunicationType();

            shipmentOrderTDType.Shipment = shipment;
            shipmentOrderTDType.LabelResponseType = ShipmentOrderTDTypeLabelResponseType.URL;

            // add Shipment Order Object to request
            ShipmentOrderTDType[] shOrders = new ShipmentOrderTDType[1];
            shOrders[0] = shipmentOrderTDType;
            createShipmentTDRequest.ShipmentOrder = shOrders;

            return createShipmentTDRequest;
        }


        public static GetLabelDDRequest getDefaultLabelDDRequest(String shipmentId)
        {
            GetLabelDDRequest ddRequest = new GetLabelDDRequest();
            ddRequest.Version = createVersion();
            ShipmentNumberType shNumber = new ShipmentNumberType();
            shNumber.ItemElementName = ItemChoiceType7.shipmentNumber;
            if (shipmentId != "")
            {
                shNumber.Item = shipmentId;
            }
            else
            {
                shNumber.Item = DUMMY_SHIPMENT_NUMBER;
            }

            ShipmentNumberType[] shNumbers = new ShipmentNumberType[1];
            shNumbers[0] = shNumber;
            ddRequest.ShipmentNumber = shNumbers;
            return ddRequest;
        }


        public static DeleteShipmentDDRequest getDeleteShipmentDDRequest(
                String shipmentId)
        {
            DeleteShipmentDDRequest ddRequest = new DeleteShipmentDDRequest();
            ddRequest.Version = createVersion();
            ShipmentNumberType shNumber = new ShipmentNumberType();
            shNumber.ItemElementName = ItemChoiceType7.shipmentNumber;
            if (shipmentId != "")
                shNumber.Item = shipmentId;
            else
                shNumber.Item = DUMMY_SHIPMENT_NUMBER;

            ShipmentNumberType[] shNumbers = new ShipmentNumberType[1];
            shNumbers[0] = shNumber;
            ddRequest.ShipmentNumber = shNumbers;
            return ddRequest;
        }


        public static DeleteShipmentTDRequest getDeleteShipmentTDRequest(
                String shipmentId)
        {
            DeleteShipmentTDRequest tdRequest = new DeleteShipmentTDRequest();
            tdRequest.Version = createVersion();
            ShipmentNumberType shNumber = new ShipmentNumberType();
            shNumber.ItemElementName = ItemChoiceType7.airwayBill;
            if (shipmentId != "")
                shNumber.Item = shipmentId;
            else
                shNumber.Item = DUMMY_AIRWAY_BILL;

            ShipmentNumberType[] shNumbers = new ShipmentNumberType[1];
            shNumbers[0] = shNumber;
            tdRequest.ShipmentNumber = shNumbers;
            return tdRequest;
        }
    }
}
