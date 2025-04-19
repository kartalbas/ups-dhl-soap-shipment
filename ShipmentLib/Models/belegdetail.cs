namespace ShipmentLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("lewiaa.belegdetails")]
    public partial class belegdetail
    {
        public int ID { get; set; }

        public int? MID { get; set; }

        public int? FID { get; set; }

        public int ArtikelID { get; set; }

        public int AdressenID { get; set; }

        public int? PosNummer { get; set; }

        public int? Kontonummer { get; set; }

        public int? Artikelnummer { get; set; }

        [StringLength(50)]
        public string Lieferantenartikelnummer { get; set; }

        [StringLength(200)]
        public string Matchcode { get; set; }

        [StringLength(16777215)]
        public string Artikeltext1 { get; set; }

        [StringLength(16777215)]
        public string Artikeltext2 { get; set; }

        [StringLength(16777215)]
        public string Text { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Belegdatum { get; set; }

        public int? BelegeID { get; set; }

        public int? BelegartID { get; set; }

        public double? Einzelpreis { get; set; }

        public double? Anzahl { get; set; }

        public double? Rabatt { get; set; }

        public double? Endpreis { get; set; }

        public double? Steuersatz { get; set; }

        public double? Steuerbetrag { get; set; }

        public int? ChargenID { get; set; }

        [StringLength(50)]
        public string Chargennummer { get; set; }

        public double? AnzEinheit { get; set; }

        public double? Inhalt { get; set; }

        [StringLength(50)]
        public string Einheit { get; set; }

        public DateTime? ZS { get; set; }

        public double? EKPreis { get; set; }

        public int? AbteilungID { get; set; }

        public double? Gewinn { get; set; }

        [StringLength(200)]
        public string BarcodeNr { get; set; }

        public double? EKSoll { get; set; }

        [StringLength(1073741823)]
        public string TextilString { get; set; }

        public double? ISTBestand { get; set; }

        public int? ArtikelgruppenID { get; set; }

        [StringLength(100)]
        public string Artikelgruppe { get; set; }

        public double? Gewicht { get; set; }

        public int? PosLIEID { get; set; }

        public double? Volumen { get; set; }

        public int? ArtikelartID { get; set; }

        [StringLength(100)]
        public string Status { get; set; }

        public int? PosReasonsID { get; set; }

        [StringLength(1073741823)]
        public string PosReasonsText { get; set; }

        public int? PosAbbuchen { get; set; }

        public int? LagerFilialenID { get; set; }

        public double? Retoure { get; set; }

        public double? VKNetto { get; set; }

        public double? VKBrutto { get; set; }

        public double? EndpreisNetto { get; set; }

        public double? EndpreisBrutto { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Geaendert { get; set; }

        public int? PersonalID { get; set; }

        public int? Personalnummer { get; set; }

        [StringLength(200)]
        public string Personal { get; set; }

        [StringLength(200)]
        public string Belegnummer { get; set; }

        public int? Rabattierbar { get; set; }

        public double? Rueckstand { get; set; }

        public int? VerpackungsartID { get; set; }

        [StringLength(200)]
        public string Verpackungsart { get; set; }

        public int? Inventuranzahl { get; set; }

        public double? MittlererEK { get; set; }

        public int? REingefuegt { get; set; }

        public double? WKurs { get; set; }

        public double? EinzelpreisWaehrung { get; set; }

        [StringLength(200)]
        public string Waehrung { get; set; }

        [StringLength(10)]
        public string WaehrungSymbol { get; set; }

        [StringLength(200)]
        public string Regal { get; set; }

        [StringLength(200)]
        public string Warennummer { get; set; }

        [StringLength(16777215)]
        public string Seriennummer { get; set; }

        public int? SeriennummerAnzahl { get; set; }

        [StringLength(200)]
        public string FremdartikelNr { get; set; }

        [StringLength(255)]
        public string Fremdartikeltext { get; set; }

        public long? ForeColor { get; set; }

        public long? BackColor { get; set; }

        public int? Verfahrenscode { get; set; }

        public int? Laendercode { get; set; }

        public int? IntrastatistikID { get; set; }

        public int? HerkunftsLaenderCodeID { get; set; }

        [StringLength(20)]
        public string HerkunftsLaenderCode { get; set; }

        [StringLength(200)]
        public string Herkunftsland { get; set; }

        public int? Bundesland { get; set; }

        [StringLength(50)]
        public string ArtDesGeschaefts { get; set; }

        public int? Deaktivieren { get; set; }

        public int? VorbestellBelegdetailsID { get; set; }

        public int? VorbestellStatus { get; set; }

        [StringLength(200)]
        public string HerstellerartikelNr { get; set; }

        public int? Verfuegbar { get; set; }

        public int? AnbieterID { get; set; }

        public double? Punkte { get; set; }

        public int? KommissionZeit { get; set; }

        public int? MixArtikelID { get; set; }

        [StringLength(100)]
        public string AmazonTransaktionsNr { get; set; }

        public double? Amazonkosten { get; set; }

        public int? Kommissionsverkauf { get; set; }

        [StringLength(100)]
        public string ChargenNr { get; set; }
    }
}
