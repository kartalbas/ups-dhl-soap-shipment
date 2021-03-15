namespace ShipmentLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("lewiaa.adressen")]
    public partial class adressen
    {
        public int ID { get; set; }

        public int? FID { get; set; }

        public int? MID { get; set; }

        public int? Kundennummer { get; set; }

        public int? Adressennummer { get; set; }

        [StringLength(50)]
        public string Mandatsreferenznummer { get; set; }

        [StringLength(200)]
        public string Kundencode { get; set; }

        [StringLength(40)]
        public string Anrede { get; set; }

        [StringLength(200)]
        public string Name1 { get; set; }

        [StringLength(200)]
        public string Name2 { get; set; }

        [StringLength(30)]
        public string Position { get; set; }

        [StringLength(200)]
        public string Strasse { get; set; }

        [StringLength(200)]
        public string Ort { get; set; }

        [StringLength(200)]
        public string Region { get; set; }

        [StringLength(30)]
        public string PLZ { get; set; }

        [StringLength(200)]
        public string Land { get; set; }

        [StringLength(200)]
        public string Telefon1 { get; set; }

        [StringLength(200)]
        public string Telefon2 { get; set; }

        [StringLength(200)]
        public string Telefax { get; set; }

        [StringLength(200)]
        public string Mobil { get; set; }

        [StringLength(200)]
        public string eMail { get; set; }

        [StringLength(200)]
        public string Homepage { get; set; }

        [StringLength(1073741823)]
        public string Info { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Aufnahmedatum { get; set; }

        [StringLength(50)]
        public string Zahlungsart { get; set; }

        [StringLength(50)]
        public string SteuerID { get; set; }

        public int? Besteuerung { get; set; }

        public int? VerkaufsberaterID { get; set; }

        [StringLength(200)]
        public string Verkaufsberater { get; set; }

        [StringLength(50)]
        public string Preisgruppe { get; set; }

        public int? KontoID { get; set; }

        [StringLength(50)]
        public string Versandart { get; set; }

        [StringLength(50)]
        public string Zahlungsbedingung { get; set; }

        public int? Umsatzsteuerpflicht { get; set; }

        public int? NettoTage { get; set; }

        public double? Skonto1 { get; set; }

        public double? Skonto2 { get; set; }

        public double? Skonto3 { get; set; }

        public int? Skonto1Tage { get; set; }

        public int? Skonto2Tage { get; set; }

        public int? Skonto3Tage { get; set; }

        public int? Mahnungsart { get; set; }

        public int? Karenztage { get; set; }

        public double? Mahngeb1 { get; set; }

        public double? Mahngeb2 { get; set; }

        public double? Mahngeb3 { get; set; }

        public int? Mahnfrist { get; set; }

        public double? AdrRabatt { get; set; }

        public int? LieferungSperren { get; set; }

        public int? AutoHinzu { get; set; }

        public double? Kreditlimit { get; set; }

        public int? KundengruppeID { get; set; }

        public int? SpracheID { get; set; }

        [StringLength(200)]
        public string Sprache { get; set; }

        public int? WaehrungID { get; set; }

        [StringLength(200)]
        public string Inhaber { get; set; }

        [StringLength(200)]
        public string Finanzamt { get; set; }

        [StringLength(100)]
        public string Steuernummer { get; set; }

        [StringLength(200)]
        public string Nationalitaet { get; set; }

        [StringLength(200)]
        public string Ausweisnummer { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Ausweisgueltigkeit { get; set; }

        [StringLength(200)]
        public string Ausstellungsort { get; set; }

        public int? TourenID { get; set; }

        [StringLength(50)]
        public string Passwort { get; set; }

        public int? Etikett { get; set; }

        public int? FilialenID { get; set; }

        public int? Adressenart { get; set; }

        public int? VertreterID { get; set; }

        public int? Provisionsart { get; set; }

        public double? Provision { get; set; }

        public int? Abrechnungsart { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Geburtsdatum { get; set; }

        public double? Gesamtpunkte { get; set; }

        public double? Gesamtguthaben { get; set; }

        public double? Gesamtumsatz { get; set; }

        public double? Gesamtoffen { get; set; }

        public double? Punkteeingeloest { get; set; }

        public double? Guthabeneingeloest { get; set; }

        public double? PunkteGratuliert { get; set; }

        [Column(TypeName = "date")]
        public DateTime? GeburtstagGratuliert { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Zielpunktdatum { get; set; }

        public int? RegionID { get; set; }

        [StringLength(200)]
        public string Vertretername { get; set; }

        public int? Vorlage { get; set; }

        [StringLength(200)]
        public string Rechnungsgruppe { get; set; }

        public int? OberePreisgruppe { get; set; }

        public int? UnterePreisgruppe { get; set; }

        public int? Stationsnummer { get; set; }

        public int? GruppenpreislisteID { get; set; }

        public int? Insolvent { get; set; }

        [StringLength(200)]
        public string Rechtsabteilung { get; set; }

        [StringLength(200)]
        public string Tour { get; set; }

        [StringLength(1073741823)]
        public string Factoringtext { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Geaendert { get; set; }

        public int? LieferscheinDrucken { get; set; }

        public double? Versandkostenfreibetrag { get; set; }

        public double? Mindestbestellmenge { get; set; }

        [StringLength(200)]
        public string VersendetMit { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Abschreibungsdatum { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Insolvenzdatum { get; set; }

        public int? Zahlungserinnerung { get; set; }

        public int? Mahnstufe { get; set; }

        [StringLength(200)]
        public string Mahnstatus { get; set; }

        public int? MahnungVerkaufSperren { get; set; }

        public int? OffenePostenTage { get; set; }

        public double? Umsatz3Monate { get; set; }

        public int? PreiseBearbeiten { get; set; }

        public double? Umsatz21Tage { get; set; }

        [StringLength(200)]
        public string BankInhaber { get; set; }

        [StringLength(200)]
        public string BankName { get; set; }

        [StringLength(200)]
        public string BankKonto { get; set; }

        [StringLength(200)]
        public string BankBLZ { get; set; }

        [StringLength(200)]
        public string BankSwiftcode { get; set; }

        [StringLength(200)]
        public string BankIban { get; set; }

        [StringLength(200)]
        public string BankKartennummer { get; set; }

        public int? PreisSpezialID { get; set; }

        [StringLength(200)]
        public string AfterbuyKundennummer { get; set; }

        public int? WachstumStatus { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Kontaktdatum { get; set; }

        [StringLength(16777215)]
        public string Beschwerden { get; set; }

        [StringLength(16777215)]
        public string Massnahmen { get; set; }

        public int? ShopSperre { get; set; }

        public int? KommissionsgruppeID { get; set; }

        public int? Kommissionskunde { get; set; }

        public int? Endkunde { get; set; }

        public int? KommissionsartikelMax { get; set; }

        public int? Datenweitervergabe { get; set; }

        public int? Buchungsart { get; set; }

        public int? ArtikelKlasseA { get; set; }

        public int? ArtikelKlasseB { get; set; }

        public int? ArtikelKlasseC { get; set; }

        public int? SepaKunde { get; set; }

        public int? SepaStatus { get; set; }

        public int? NoSync { get; set; }

        public int? KommissionskundeZeit { get; set; }

        [StringLength(200)]
        public string Rechnungsmail { get; set; }

        public int? Newsletter { get; set; }

        public int? Amazonkunde { get; set; }

        public int? EmailFehlerhaft { get; set; }

        public int? ZahlungAnAnbieter { get; set; }
    }
}
