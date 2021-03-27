namespace ShipmentDHL
{
    public class ItemPosition
    {
        public string Amount { get; set; }
        public decimal CustomsValue { get; set; }
        public string Description { get; set; }
        public string CommodityCode { get; set; }
        public string CountryCodeOrigin { get; set; }
        public string CustomsCurrency { get; set; }
        public decimal GrossWeightInKG { get; set; }
        public decimal NetWeightInKG { get; set; }
    }
}
