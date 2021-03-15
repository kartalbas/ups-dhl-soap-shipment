namespace ShipmentLib.Models
{
    public class ReportData
    {
        public string Kundennummer { get; set; }
        public string Versandart { get; set; }
        public double? Gesamtgewicht { get; set; }
        public string Trackingnumbers { get; set; }
        public int? LieferadressenID { get; set; }
    }
}
