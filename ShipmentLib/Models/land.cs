namespace ShipmentLib
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("lewiaa.land")]
    public partial class land
    {
        public int ID { get; set; }

        public int? Nummer { get; set; }

        [Column("Land", TypeName = "char")]
        [StringLength(200)]
        public string Land1 { get; set; }

        [Column(TypeName = "char")]
        [StringLength(200)]
        public string Kuerzel { get; set; }

        public int? Laendercode { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Geaendert { get; set; }
    }
}
