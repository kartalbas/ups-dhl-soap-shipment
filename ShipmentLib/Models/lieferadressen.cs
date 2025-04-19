namespace ShipmentLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("lewiaa.lieferadressen")]
    public partial class lieferadressen
    {
        public int ID { get; set; }

        public int? MID { get; set; }

        public int? FID { get; set; }

        public int AdressenID { get; set; }

        [StringLength(50)]
        public string Anrede { get; set; }

        [StringLength(50)]
        public string Name1 { get; set; }

        [StringLength(50)]
        public string Name2 { get; set; }

        [StringLength(50)]
        public string Strasse { get; set; }

        [StringLength(100)]
        public string HausNr { get; set; }

        [StringLength(50)]
        public string Plz { get; set; }

        [StringLength(50)]
        public string Ort { get; set; }

        [StringLength(50)]
        public string Land { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Geaendert { get; set; }
    }
}
