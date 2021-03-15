namespace ShipmentLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("lewiaa.zahlungsarten")]
    public partial class zahlungsarten
    {
        public int ID { get; set; }

        public int? MID { get; set; }

        public int? FID { get; set; }

        public int? ZahlungsartNr { get; set; }

        [StringLength(50)]
        public string Zahlungsart { get; set; }

        [StringLength(200)]
        public string Zahlungsarttext { get; set; }

        [Column(TypeName = "char")]
        [StringLength(3)]
        public string DTATextschluessel { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Geaendert { get; set; }
    }
}
