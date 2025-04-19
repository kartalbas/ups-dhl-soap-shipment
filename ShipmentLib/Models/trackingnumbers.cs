using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipmentLib
{
    [Table("lewiaa.trackingnumbers")]
    public partial class trackingnumbers
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int? RechnungsNr { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Datum { get; set; }

        [StringLength(255)]
        public string TrackingNr { get; set; }

        public TrackingStatus Status { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Geaendert { get; set; }
    }

    public enum TrackingStatus
    {
        Created = 0,
        Voided = 1,
    }
}
