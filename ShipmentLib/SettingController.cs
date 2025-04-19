
namespace ShipmentLib
{
    public static class SettingController
    {
        public static string LogFolder { get { return Properties.Settings.Default.LogFolder; } }
        public static string OpenNotepadOnException { get { return Properties.Settings.Default.OpenNotepadOnException; } }
        public static string UPS_ShippingModule_ShipWebReference_ShipService { get { return Properties.Settings.Default.UPS_TestMode ? Properties.Settings.Default.UPS_ShippingModule_ShipWebReference_ShipService_Test : Properties.Settings.Default.UPS_ShippingModule_ShipWebReference_ShipService_Prod; } }
        public static string UPS_ShippingModule_VoidWebReference_VoidService { get { return Properties.Settings.Default.UPS_TestMode ? Properties.Settings.Default.UPS_ShippingModule_VoidWebReference_VoidService_Test : Properties.Settings.Default.UPS_ShippingModule_VoidWebReference_VoidService_Prod; } }
        public static string DownloadFolder { get { return Properties.Settings.Default.DownloadFolder; } }
        public static string LaserPrinter { get { return Properties.Settings.Default.LaserPrinter; } }
        public static string LabelPrinter { get { return Properties.Settings.Default.LabelPrinter; } }
        public static string PackageTypeCode { get { return Properties.Settings.Default.PackageTypeCode; } }
        public static string ServiceNotificationCode { get { return Properties.Settings.Default.ServiceNotificationCode; } }
        public static string ServiceDescription { get { return Properties.Settings.Default.ServiceDescription; } }
        public static string CODAmountCurrencyCode { get { return Properties.Settings.Default.CODAmountCurrencyCode; } }
        public static string ShipUnitOfMeasurementTypeCode { get { return Properties.Settings.Default.ShipUnitOfMeasurementTypeCode; } }
        public static string ShipUnitOfMeasurementTypeDescription { get { return Properties.Settings.Default.ShipUnitOfMeasurementTypeDescription; } }
        public static string LabelImageFormatCode { get { return Properties.Settings.Default.LabelImageFormatCode; } }
        public static string LabelStockSizeHeightaa11 { get { return Properties.Settings.Default.LabelStockSizeHeight; } }
        public static string LabelStockSizeWidth { get { return Properties.Settings.Default.LabelStockSizeWidth; } }
        public static string LabelStockSizeHeight { get { return Properties.Settings.Default.LabelStockSizeHeight; } }
        public static string ShipperAddressLine { get { return Properties.Settings.Default.ShipperAddressLine; } }
        public static string ShipmentDescription { get { return Properties.Settings.Default.ShipmentDescription; } }
        public static string ShipmentChargeType { get { return Properties.Settings.Default.ShipmentChargeType; } }
        public static string ShipperName { get { return Properties.Settings.Default.ShipperName; } }
        public static string ShipperAttentionName { get { return Properties.Settings.Default.ShipperAttentionName; } }
        public static string ShipperPostalCode { get { return Properties.Settings.Default.ShipperPostalCode; } }
        public static string ShipperCity { get { return Properties.Settings.Default.ShipperCity; } }
        public static string ShipperCountryCode { get { return Properties.Settings.Default.ShipperCountryCode; } }
        public static string ShipperPhoneNumber { get { return Properties.Settings.Default.ShipperPhoneNumber; } }
        public static string ShipperStateProvinceCode { get { return Properties.Settings.Default.ShipperStateProvinceCode; } }
        public static string ShipperEmail { get { return Properties.Settings.Default.ShipperEmail; } }
        public static string ShipperAccountOwner { get { return Properties.Settings.Default.ShipperAccountOwner; } }
        public static string ShipperAccountNumber { get { return Properties.Settings.Default.ShipperAccountNumber; } }
        public static string ShipperBankCode { get { return Properties.Settings.Default.ShipperBankCode; } }
        public static string ShipperBankName { get { return Properties.Settings.Default.ShipperBankName; } }
        public static string ShipperIban { get { return Properties.Settings.Default.ShipperIban; } }
        public static string ShipperBic { get { return Properties.Settings.Default.ShipperBic; } }      
        public static string LLDesign { get { return Properties.Settings.Default.LLDesign; } }
        public static string LLPreview { get { return Properties.Settings.Default.LLPreview; } }
        public static string FileLLSummaryUPS { get { return Properties.Settings.Default.FileLLSummaryUPS; } }
        public static string FileLLListUPS { get { return Properties.Settings.Default.FileLLListUPS; } }
        public static string FileLLSummaryDHL { get { return Properties.Settings.Default.FileLLSummaryDHL; } }
        public static string FileLLManifestedDHL { get { return Properties.Settings.Default.FileLLManifestedDHL; } }
        public static string FileLLListDHL { get { return Properties.Settings.Default.FileLLListDHL; } }
        public static string ShipperFromName { get { return Properties.Settings.Default.ShipperFromName; } }
        public static string ShipperFromAttentionName { get { return Properties.Settings.Default.ShipperFromAttentionName; } }
        public static string ShipperFromAddressLine { get { return Properties.Settings.Default.ShipperFromAddressLine; } }
        public static string ShipperFromPostalCode { get { return Properties.Settings.Default.ShipperFromPostalCode; } }
        public static string ShipperFromCity { get { return Properties.Settings.Default.ShipperFromCity; } }
        public static string ShipperFromCountryCode { get { return Properties.Settings.Default.ShipperFromCountryCode; } }
        public static string ShipperFromStateProvinceCode { get { return Properties.Settings.Default.ShipperFromStateProvinceCode; } }
        public static string ShipperFromEmail { get { return Properties.Settings.Default.ShipperFromEmail; } }
        public static string Retries { get { return Properties.Settings.Default.Retries; } }
        public static string PrintBarcode { get { return Properties.Settings.Default.PrintBarcode; } }
        public static string PrintTurnIn { get { return Properties.Settings.Default.PrintTurnIn; } }
        public static string FileLLHtml { get { return Properties.Settings.Default.FileLLHtml; } }
        public static string FileLLPdf { get { return Properties.Settings.Default.FileLLPdf; } }
        public static string FileLLPic { get { return Properties.Settings.Default.FileLLPic; } }
        public static string FileLLExportDocumentPdf { get { return Properties.Settings.Default.FileLLExportDocumentPdf; } }
        public static string UPS_AccessLicenseNumber { get { return Properties.Settings.Default.UPS_AccessLicenseNumber; } }
        public static string UPS_Username { get { return Properties.Settings.Default.UPS_Username; } }
        public static string UPS_Password { get { return Properties.Settings.Default.UPS_Password; } }
        public static string DHL_IntraShipUser { get { return Properties.Settings.Default.DHL_IntraShipUser; } }
        public static string DHL_IntraShipPassword { get { return Properties.Settings.Default.DHL_IntraShipPassword; } }
        public static string DHL_CigUser { get { return Properties.Settings.Default.DHL_CigUser; } }
        public static string DHL_CigPassword { get { return Properties.Settings.Default.DHL_CigPassword; } }
        public static string ShipperStreet { get { return Properties.Settings.Default.ShipperStreet; } }
        public static string ShipperStreetNr { get { return Properties.Settings.Default.ShipperStreetNr; } }
        public static string DHL_EKP_Prod { get { return Properties.Settings.Default.DHL_EKP_Prod; } }
        public static string DHL_EKP_Test { get { return Properties.Settings.Default.DHL_EKP_Test; } }
        public static string DHL_ApplicationsID { get { return Properties.Settings.Default.DHL_ApplicationsID; } }
        public static string DHL_Applicationstoken { get { return Properties.Settings.Default.DHL_Applicationstoken; } }
        public static string DHL_PDF_Filename { get { return Properties.Settings.Default.DHL_PDF_Filename; } }
        public static string DHL_Manifest_Filename { get { return Properties.Settings.Default.DHL_Manifest_Filename; } }
        public static string Shipper_VATID { get { return Properties.Settings.Default.Shipper_VATID; } }
        public static bool DHL_TestMode { get { return Properties.Settings.Default.DHL_TestMode; } }
        public static bool UPS_TestMode { get { return Properties.Settings.Default.UPS_TestMode; } }
        public static string DHL_ShippingModule_ShipWebReference_ShipService_Test { get { return Properties.Settings.Default.DHL_ShippingModule_ShipWebReference_ShipService_Test; } }
        public static string DHL_ShippingModule_ShipWebReference_ShipService_Prod { get { return Properties.Settings.Default.DHL_ShippingModule_ShipWebReference_ShipService_Prod; } }
        public static string DatabaseCulture { get { return Properties.Settings.Default.DatabaseCulture; } }
        public static string Paket_Length { get { return Properties.Settings.Default.Paket_Length; } }
        public static string Paket_Width { get { return Properties.Settings.Default.Paket_Width; } }
        public static string Paket_Height { get { return Properties.Settings.Default.Paket_Height; } }
        public static string DHL_PartnerID { get { return Properties.Settings.Default.DHL_PartnerID; } }
        public static string DHL_ExportDocument_Filename { get { return Properties.Settings.Default.DHL_ExportDocument_Filename; } }
        public static string DHL_Username { get { return Properties.Settings.Default.DHL_Username; } }
        public static string LLLicenseInfo { get { return Properties.Settings.Default.LLLicenseInfo; } }
        public static string DHL_PartnerCode_NAT { get { return Properties.Settings.Default.DHL_PartnerCode_NAT; } }
        public static string DHL_PartnerCode_INT { get { return Properties.Settings.Default.DHL_PartnerCode_INT; } }
        public static string DHL_PartnerCode_BP_INT { get { return Properties.Settings.Default.DHL_PartnerCode_BP_INT; } }
        public static string DHL_PartnerCode_TestMode { get { return Properties.Settings.Default.DHL_PartnerCode_TestMode; } }
        public static double PauschalBetrag { get { return Properties.Settings.Default.PauschalBetrag; } }
    }
}
