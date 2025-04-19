using ShipmentLib;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ShipmentDHL
{
    public class Shipment : ShipmentData
    {
        public Shipment()
        {
            Packages = new List<Package>();
            ItemPositions = new List<ItemPosition>();
            DownloadedFiles = new List<string>();

            ShipperCompanyName = SettingController.ShipperName;
            ShipperName = string.Empty; //SettingController.ShipperAttentionName;
            ShipperStreet = SettingController.ShipperStreet;
            ShipperStreetNr = SettingController.ShipperStreetNr;
            ShipperZip = SettingController.ShipperPostalCode;
            ShipperCity = SettingController.ShipperCity;
            ShipperCountryCode = SettingController.ShipperCountryCode;
            ShipperPhone = SettingController.ShipperPhoneNumber;
            ShipperEMail = SettingController.ShipperEmail;

            MajorRelease = "3";
            MinorRelease = "1";
            SDF = "yyyy-MM-dd";

            Trackingnumber = string.Empty;
            PackageCount = 1;

            if (!Directory.Exists(SettingController.DownloadFolder))
                Directory.CreateDirectory(SettingController.DownloadFolder);

            ShipmentTools.DeleteFiles(SettingController.DownloadFolder, "*.pdf");

            ExecutedByShipmentTests = false;
        }

        public string GetPartnerCode()
        {
            if (TestMode)
                return SettingController.DHL_PartnerCode_TestMode;

            return SettingController.DHL_PartnerCode_NAT;
        }

        public class Package
        {
            public Package() : this(1, "25", "15", "1")
            {
            }

            public Package(double? dWeightInKG, string strLengthInCM, string strWidthInCM, string strHeightInCM)
            {
                string strLocale = SettingController.DatabaseCulture;
                WeightInKG = dWeightInKG < 1 ? "1" : dWeightInKG.ToString();

                if (strLengthInCM.Length > 0)
                    LengthInCM = int.Parse(strLengthInCM, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);
                else
                    LengthInCM = int.Parse(SettingController.Paket_Length, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);

                if (strWidthInCM.Length > 0)
                    WidthInCM = int.Parse(strWidthInCM, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);
                else
                    WidthInCM = int.Parse(SettingController.Paket_Width, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);

                if (strHeightInCM.Length > 0)
                    HeightInCM = int.Parse(strHeightInCM, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);
                else
                    HeightInCM = int.Parse(SettingController.Paket_Height, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);
            }

            public string WeightInKG { get; set; }
            public string LengthInCM { get; set; }
            public string WidthInCM { get; set; }
            public string HeightInCM { get; set; }
        }

        public List<Package> Packages { get; set; }

        public class ItemPosition
        {
            public string Amount { get; set; }
            public decimal CustomsValue { get; set; }
            public string Description { get ; set; }
            public string CommodityCode { get; set; }
            public string CountryCodeOrigin { get; set; }
            public string CustomsCurrency { get; set; }
            public decimal GrossWeightInKG { get; set; }
            public decimal NetWeightInKG { get; set; }
        }

        public List<ItemPosition> ItemPositions { get; set; }

        public string ProdCode_V01PAK_DHLPaket { get { return "V01PAK"; } }
        public string ProdCode_V54EPAK_DHLPaketInternational { get { return "V54EPAK"; } }
        public string ProdCode_V53WPAK_DHLPaketInternational { get { return "V53WPAK"; } }
        public string ProdCode_V62WP_DHLWarenpost { get { return "V62WP"; } }

        public string ProdCode { get; set; }

        public bool TestMode { get { return SettingController.DHL_TestMode; } }

        public string Endpoint { get { return TestMode ? SettingController.DHL_ShippingModule_ShipWebReference_ShipService_Test : SettingController.DHL_ShippingModule_ShipWebReference_ShipService_Prod; } }

        public string EKP { get { return TestMode ? SettingController.DHL_EKP_Test : SettingController.DHL_EKP_Prod; } }

        public int PackageCount { get; set; }

        public string InvoiceNumber { get; set; }

        public bool ExecutedByShipmentTests { get; set; }

        public string ShipperStreet { get; set; }
        public string ShipperStreetNr { get; set; }
        public string ShipperCity { get; set; }
        public string ShipperZip { get; set; }
        public string ShipperCountryCode { get; set; }
        public string ShipperEMail { get; set; }
        public string ShipperName { get; set; }
        public string ShipperPhone { get; set; }
        public string ShipperCompanyName { get; set; }

        public string MajorRelease { get; set; }
        public string MinorRelease { get; set; }
        public string SDF { get; set; }

        public string ShipmentDesc { get; set; }

        public string ReceiverCompanyName { get; set; }
        public string SignerTitle { get; set; }
        public string ReceiverFirstName { get; set; }
        public string ReceiverLastName { get; set; }
        public string ReceiverStreet { get; set; }
        public string ReceiverStreentNr { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverZip { get; set; }
        public string ReceiverCountry { get; set; }
        public string ReceiverCountryCode { get; set; }

        public string ReceiverContactEMail { get; set; }
        public string ReceiverContactName { get; set; }
        public string ReceiverContactPhone { get; set; }

        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Trackingnumber { get; set; }
        public string LabelURL { get; set; }
        public List<string> DownloadedFiles { get; set; }
    }
}
