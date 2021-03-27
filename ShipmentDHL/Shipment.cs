using ShipmentLib;
using System.Collections.Generic;
using System.IO;

namespace ShipmentDHL
{
    /*
        CreateShipmentDD
        ----------------

        CreateShipmentDD ist der Operationsaufruf, mit dem DD-Sendungen und die relevanten DHL Paket Etiketten erzeugt werden. Anders als seine Bezeichnung nahe legt, werden über diesen Aufruf auch von DHL Express (Officepack, Express Paket und Express Ident) angebotene nationale TD-Produkte gebucht. Alle internationalen TD-Produkte und die DHL Domestic Express Produkte DOM und DXM müssen hingegen mit CreateShipmentTD erzeugt werden.

        Wenn die Auftragsdaten korrekt sind, d.h. wenn der Benutzer über die relevanten Berechtigungen verfügt, meldet der Service eine Sendungsnummer und Sendungsdaten für jeden Sendungsauftrag im Aufruf zurück. Die URLs verweisen auf druckfertige Etiketten im pdf-Format. Zusammen mit der Antwort der API werden alle angeforderten Sendungen angelegt und zwischengespeichert. Am Tagesende verarbeitet ein automatischer Manifestierungsauftrag alle zwischengespeicherten Sendungen und überträgt sie an das relevante DHL Backend-Produktionssystem zur endgültigen Buchung.

        Eine breite Palette von Produkten, Diensten und Benutzerberechtigungen macht die Verwendung dieser Operation komplex. Ob ein Produkt benutzt werden kann, wird von Absender- und Empfängermerkmalen, den Berechtigungen des DHL Accounts und kominatiorischen Einschränkungen bestimmt. Es könnte also erforderlich sein, ein bestimmtes Produkt durch den DHL Customer Support freischalten zu lassen, ehe über die API darauf zugegriffen werden kann. Darüber hinaus muss sorgfältig geprüft werden, ob ein ausgewähltes Produkt generell mit dem ausgewählten Dienst kombiniert werden kann und ob Dienste nicht miteinander kollidieren.
        (siehe FAQ Seite Übersicht der zulässigen DHL Produkte und Services)

        Die Anfrage- bzw. Antwortparameter werden bei den IO-Referenzen erläutert und in Form eines Beispiels dargestellt.
    */

    /*
        CreateShipmentTD
        ----------------

        Mit CreateShipmentTD werden TD-Sendungen angelegt und die relevanten DHL Express Etiketten erstellt. Mit der Ausnahme der beiden nationalen Produkte DHL Express DOM und DXM wird diese Funktion vor allem für internationale DHL Express Produkte eingesetzt. Es können jedoch nicht alle DHL Express Produkte über CreateShipmentTD bestellt werden: Officepack, Express Paket und Express Ident müssen über CreateShipmentDD aufgerufen werden.

        Wenn der Benutzer über die relevanten Berechtigungen verfügt und die Auftragsdaten korrekt sind, meldet der Service eine AirwayBill-Nummer und Sendungsdaten für jede im Aufruf enthaltene Sendung zurück. Die URLs verweisen auf druckfertige Etiketten im pdf-Format. Zusammen mit der Antwort der API werden alle angeforderten Sendungen angelegt und zwischengespeichert. Am Tagesende verarbeitet ein automatischer Manifestierungsauftrag alle zwischengespeicherten Sendungen und überträgt sie an das relevante DHL Backend-Produktionssystem zur endgültigen Buchung. Versandaufträge können also so lange noch gelöscht werden bis sie manifestiert wurden.

        Bei TD-Produkten werden über die Geschäftskundenversand API zwei Dienste angeboten: Saturday Delivery und Insurance (Samstagslieferung und Versicherung). Einem Client stehen jedoch nicht alle TD-Produkte und -Dienste zur Verfügung, wenn die relevanten Berechtigungen nicht erteilt wurden. Hierzu muss der DHL Customer Support kontaktiert werden.

        Die Anfrage- bzw. Antwortparameter werden bei den IO-Referenzen erläutert und in Form eines Beispiels dargestellt.
    */

    public partial class Shipment : ShipmentData
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

            MajorRelease = "1";
            MinorRelease = "0";
            SDF = "yyyy-MM-dd";

            Trackingnumber = string.Empty;
            PackageCount = 1;
            CODAmount = 0.0M;
            CODCurrency = "EUR";

            if (!Directory.Exists(SettingController.DownloadFolder))
                Directory.CreateDirectory(SettingController.DownloadFolder);

            ShipmentTools.DeleteFiles(SettingController.DownloadFolder, "*.pdf");

            ExecutedByShipmentTests = false;
        }

        public string GetPartnerCode()
        {
            if (TestMode)
                return SettingController.DHL_PartnerCode_TestMode;

            if (DDProdCode.Equals(DDProdCode_EPN_EUROPACK_NATIONAL) || DDProdCode.Equals(ProdCode_V62WP_DHLWarenpost))
                return SettingController.DHL_PartnerCode_NAT;
            else if (DDProdCode.Equals(DDProdCode_EPI_EUROPAK_INTERNATIONAL))
                return SettingController.DHL_PartnerCode_INT;
            else if (DDProdCode.Equals(DDProdCode_BPI_BUSINESS_PAKET_INTERNATIONAL))
                return SettingController.DHL_PartnerCode_BP_INT;

            return string.Empty;
        }

        public List<Package> Packages { get; set; }

        public List<ItemPosition> ItemPositions { get; set; }

        public string ProdCode_V01PAK_DHLPaket { get { return "V01PAK"; } }
        public string ProdCode_V54EPAK_DHLPaketInternational { get { return "V54EPAK"; } }
        public string ProdCode_V53WPAK_DHLPaketInternational { get { return "V53WPAK"; } }
        public string ProdCode_V62WP_DHLWarenpost { get { return "V62WP"; } }

        public string DDProdCode_EPN_EUROPACK_NATIONAL { get { return "EPN"; } }
        public string DDProdCode_BPI_BUSINESS_PAKET_INTERNATIONAL { get { return "BPI"; } }
        public string DDProdCode_EPI_EUROPAK_INTERNATIONAL { get { return "EPI"; } }
        public string DDProdCode { get; set; }

        public string IncoTerm_EXW_Ab_Werk { get { return "EXW"; } }
        public string IncoTerm_FCA_Frei_Frachtfuehrer { get { return "FCA"; } }
        public string IncoTerm_CPT_Frachtfrei { get { return "CPT"; } }
        public string IncoTerm_CIP_Frachtfrei_Versichert { get { return "CIP"; } }
        public string IncoTerm_DAT_Geliefert_Terminal { get { return "DAT"; } }
        public string IncoTerm_DAP_Geliefert_Benannter_Ort { get { return "DAP"; } }
        public string IncoTerm_DDP_Geliefert_Verzollt { get { return "DDP"; } }
        public string IncoTerm_FAS_Frei_Laengsseite_Schiff { get { return "FAS"; } }
        public string IncoTerm_FOB_Frei_An_Board { get { return "FOB"; } }
        public string IncoTerm_CFR_Kosten_Und_Fracht { get { return "CFR"; } }
        public string IncoTerm_CIF_Kosten_Versicherung_Und_Fracht { get { return "CIF"; } }

        public string IncoTerm { get; set; }

        public bool ExecutedByShipmentTests { get; set; }
        public bool TestMode { get { return SettingController.DHL_TestMode; } }

        public string Endpoint { get { return TestMode ? SettingController.DHL_ShippingModule_ShipWebReference_ShipService_Test : SettingController.DHL_ShippingModule_ShipWebReference_ShipService_Prod; } }

        public string EKP { get { return TestMode ? SettingController.DHL_EKP_Test : SettingController.DHL_EKP_Prod; } }

        public int PackageCount { get; set; }

        public string InvoiceNumber { get; set; }

        public string ExportDocumentAmount { get; set; }
        public decimal ExportDocumentCustomsValue { get; set; }
        public decimal ExportDocumentAdditionalFee { get; set; }
        public string ExportDocumentDescription { get; set; }
        public string ExportDocumentTermsOfTrade { get; set; }
        public string ExportDocumentCustomsCurrency { get; set; }
        public string ExportDocumentAttestationNumber { get; set; }
        public string ExportDocumentCommodityCode { get; set; }
        public string ExportDocumentExportTypeDescription { get; set; }
        public string ExportDocumentMRNNumber { get; set; }
        public string ExportDocumentPermitNumber { get; set; }
        public string ExportDocumentInvoiceNumber { get; set; }
        public string ExportDocumentInvoiceDate { get; set; }
        public string ExportDocumentCountryCodeOrigin { get; set; }
        public string ExportDocumentDownloadedFile { get; set; }

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

        public decimal CODAmount { get; set; }
        public string CODCurrency { get; set; }

        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Trackingnumber { get; set; }
        public string LabelURL { get; set; }
        public List<string> DownloadedFiles { get; set; }
    }
}
