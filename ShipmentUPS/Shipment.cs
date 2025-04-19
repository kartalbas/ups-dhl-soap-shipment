using System;
using System.Collections.Generic;
using ShipmentUPS.ShipWebReference;

namespace ShipmentUPS
{
    public class Shipment : ShipmentLib.ShipmentData
    {
        public Shipment(string _AccessLicenseNumber, string _Username, string _Password)
        {
            AccessLicenseNumber = _AccessLicenseNumber;
            Username = _Username;
            ShipperNumber = _Username;
            Password = _Password;
            Packages = 1;
        }

        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public ShipmentResponse Response { get; set; }

        //User
        public string AccessLicenseNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ShipperNumber { get; set; }
        public int Packages { get; set; }
        public string ShipperAddress { get; set; }

        public string Gif_File { get; set; }
        public string TurnIn_HtmlFile { get; set; }
        public string Label_HtmlFile { get; set; }

        //Request
        public string[] RequestOption { get; set; }         //nonvalidate = No address validation. validate = Fail on failed address validation. Defaults to validate
        public string ShipmentDescription { get; set; }     //The Description of Goods for the shipment. Applies to international shipments only. Provide a detailed description of items being shipped for documents and non-documents. Provide specific descriptions, such as annual reports and 9 mm steel screws.
        public string ShipmentChargeType { get; set; }      //Values are 01 = Transportation, 02 = Duties and Taxes
        public string ShipmentReferenceNumber { get; set; }

        //shipper general
        public string[] ShipperAddressLine { get; set; }    //The shipper's street address, including name and number (when applicable). For forward Shipment 35 characters are accepted, but only 30 characters will be printed on the label for AddressLine elements.
                                                            //Address Line 1 of the shipper. For forward Shipment 35 characters are accepted, but only 30 characters will be printed on the label.
                                                            //Address Line 2 of the shipper. Usually Room/Floor information. For forward Shipment 35 characters are accepted, but only 30 characters will be printed on the label.
                                                            //Address Line 3 of the shipper. Usually department information. For forward Shipment 35 characters are accepted, but only 30 characters will be printed on the label.
        public string ShipperCity { get; set; }             //The shipper's City. For forward Shipment 30 characters are accepted, but only 15 characters will be printed on the label.
        public string ShipperPostalCode { get; set; }       //is optional and must be no more than 9 alphanumeric characters long.
        public string ShipperStateProvinceCode { get; set; }//Shipper's state or province code. For forward Shipment 5 characters are accepted, but only 2 characters will be printed on the label.
        public string ShipperCountryCode { get; set; }      //https://www.pld-certify.ups.com/CerttoolHelp/PLD0200/WebHelp_pld0200/ISOCountryCodes.htm
        public string ShipperName { get; set; }             //Shipper's company name. For forward Shipment 35 characters are accepted, but only 30 characters will be printed on the label.
        public string ShipperAttentionName { get; set; }    //Shipper's Attention Name. For forward Shipment 35 characters are accepted, but only 30 characters will be printed on the label.
        public string ShipperPhoneNumber { get; set; }      //Shipper's phone Number.
        public string ShipperEmail { get; set; }

        //shipper from
        public String[] ShipperFromAddressLine { get; set; }
        public string ShipperFromCity { get; set; }
        public string ShipperFromPostalCode { get; set; }
        public string ShipperFromStateProvinceCode { get; set; }
        public string ShipperFromCountryCode { get; set; }
        public string ShipperFromName { get; set; }
        public string ShipperFromAttentionName { get; set; }//Contact name at the consignee’s location. For RFA Shipment 35 characters are accepted, but only 25 characters will be printed on the label.
        public string ShipperFromPhoneNumber { get; set; }
        public string ShipperFromEmail { get; set; }

        //ship to
        public string[] ShipToAddressLine { get; set; }
        public string ShipToAccountNumber { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToPostalCode { get; set; }
        public string ShipToStateProvinceCode { get; set; }
        public string ShipToCountryCode { get; set; }
        public string ShipToName { get; set; }
        public string ShipToAttentionName { get; set; }
        public string ShipToPhoneNumber { get; set; }
        public string ShipToEmail { get; set; }

        //Package
        public string ServiceTypeName { get; set; }
        public string ServiceTypeCode { get; set; }         //Values are: 01 = Next Day Air, 02 = 2nd Day Air, 03 = Ground, 07 = Express, 08 = Expedited, 11 = UPS Standard, 12 = 3 Day Select, 13 = Next Day Air Saver, 14 = Next Day Air Early AM, 54 = Express Plus, 59 = 2nd Day Air A.M., 65 = UPS Saver, 82 = UPS Today Standard, 83 = UPS Today Dedicated Courier, 84 = UPS Today Intercity, 85 = UPS Today Express, 86 = UPS Today Express Saver. Note: Only service code 03 is used for Ground Freight Pricing shipments
        public string ServiceNotificationCode { get; set; }
        public string ServiceDescription { get; set; }
        public string CODFundsCode { get; set; }
        public string CODAmountCurrencyCode { get; set; }
        public string CODAmountMonetaryValue { get; set; }

        public double PackageWeight { get; set; }           //Applies to CO and SED forms only. Valid characters are 0-9 and '.' (Decimal point). Limit to 1 digit after the decimal. The maximum length of the field is 5 including '.' and can hold up to 1 decimal place.
        public string ShipUnitOfMeasurementTypeCode { get; set; } //Applies to CO and SED forms only. Possible values: KGS, LBS.
        public string ShipUnitOfMeasurementTypeDescription { get; set; }
        public string PackageTypeCode { get; set; }         //Package types. Values are: 01 = UPS Letter, 02 = Customer Supplied Package, 03 = Tube, 04 = PAK, 21 = UPS Express Box, 24 = UPS 25KG Box, 25 = UPS 10KG Box, 30 = Pallet, 2a = Small Express Box, 2b = Medium Express Box, 2c = Large Express Box. Note: Only packaging type code 02 is applicable to Ground Freight Pricing
        public string LabelStockSizeHeight { get; set; }    //Height of the label image. For IN, use whole inches.
        public string LabelStockSizeWidth { get; set; }     //Width of the label image. For IN, use whole inches.
        public string LabelImageFormatCode { get; set; }    //Label print method code that the Labels are to be generated for EPL2 formatted Labels use EPL, for SPL formatted Labels use SPL, for ZPL formatted Labels use ZPL and for image formats use GIF, for Star Printer format formatted Labels use STARPL.
        public string LabelPrinter { get; set; }
        public string LaserPrinter { get; set; }
    }
}
