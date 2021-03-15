namespace ShipmentLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("lewiaa.versandarten")]
    public partial class versandarten
    {
        public int ID { get; set; }

        public int? MID { get; set; }

        public int? FID { get; set; }

        [StringLength(255)]
        public string Versandart { get; set; }

        public double? Nachnahme { get; set; }

        public double? Porto { get; set; }

        public double? Verpackung { get; set; }

        public double? Versicherung { get; set; }

        public double? Eilzuschlag { get; set; }

        public double? Volumen { get; set; }

        public double? Gewicht { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Geaendert { get; set; }

        public double? Gebuehr { get; set; }

        public double? GebuehrProzent { get; set; }

        public double? LogistikGebuehr { get; set; }
    }
}
