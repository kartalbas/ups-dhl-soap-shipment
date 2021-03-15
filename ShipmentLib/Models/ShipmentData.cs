using System.Collections.Generic;

namespace ShipmentLib
{
    public class ShipmentData
    {
        public belege Order { get; set; }
        public List<belegdetail> Orderdetails { get; set; }
        public adressen Address { get; set; }
        public lieferadressen Lieferaddress { get; set; }
        public string OrderCountryCode { get; set; }
    }
}
