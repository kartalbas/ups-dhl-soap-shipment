using System;
using System.Collections.Generic;

namespace ShipmentLib
{
    public class ShipmentSummary
    {
        public static ShipmentSummary CreateDefault()
        {
            ShipmentSummary objShipmentSummary = new ShipmentSummary();
            objShipmentSummary.RequestOption = new string[] { "nonvalidate" };
            objShipmentSummary.ShipperAddress = SettingController.ShipperAddressLine;
            objShipmentSummary.ShipmentDescription = SettingController.ShipmentDescription;
            objShipmentSummary.ShipmentChargeType = SettingController.ShipmentChargeType;
            objShipmentSummary.ShipperName = SettingController.ShipperName;
            objShipmentSummary.ShipperAttentionName = SettingController.ShipperAttentionName;
            objShipmentSummary.ShipperAddressLine = new string[] { SettingController.ShipperAddressLine };
            objShipmentSummary.ShipperPostalCode = SettingController.ShipperPostalCode;
            objShipmentSummary.ShipperCity = SettingController.ShipperCity;
            objShipmentSummary.ShipperCountryCode = SettingController.ShipperCountryCode;
            objShipmentSummary.ShipperPhoneNumber = SettingController.ShipperPhoneNumber;
            objShipmentSummary.ShipperStateProvinceCode = SettingController.ShipperStateProvinceCode;
            objShipmentSummary.ShipperEmail = SettingController.ShipperEmail;
            return objShipmentSummary;
        }

        public string[] RequestOption { get; set; }
        public string ShipperAddress { get; set; }
        public string ShipmentDescription { get; set; }
        public string ShipmentChargeType { get; set; }
        public string ShipperName { get; set; }
        public string ShipperAttentionName { get; set; }
        public string[] ShipperAddressLine { get; set; }
        public string ShipperPostalCode { get; set; }
        public string ShipperCity { get; set; }
        public string ShipperCountryCode { get; set; }
        public string ShipperPhoneNumber { get; set; }
        public string ShipperStateProvinceCode { get; set; }
        public string ShipperEmail { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }

        public List<ShipmentSummaryDetail> ShipmentSummaryDetails { get; set; }
        public List<ManifestedDetail> ManifestedDetails { get; set; }
    }

    public class ShipmentSummaryDetail
    {
        public string AddressNumber { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public string Versandart { get; set; }
        public double Weigth { get; set; }
        public string Trackingnumber { get; set; }

        private List<ShipmentSummaryDetail> _cobjItems { get; set; }

        public ShipmentSummaryDetail()
        {
            _cobjItems = new List<ShipmentSummaryDetail>();
        }

        public void Add(ShipmentSummaryDetail item)
        {
            _cobjItems.Add(item);
        }

        public List<ShipmentSummaryDetail> GetItems()
        {
            return _cobjItems;
        }
    }

    public class ManifestedDetail
    {
        public ManifestedDetail(string strTrackingnumber, string strStatusCode, string strStatusMessage)
        {
            Trackingnumber = strTrackingnumber;
            StatusCode = strStatusCode;
            StatusMessage = strStatusMessage;
        }
        public string Trackingnumber { get; set; }
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }

}

