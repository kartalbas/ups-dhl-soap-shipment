namespace ShipmentLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("lewiaa.belege")]
    public partial class belege
    {
        public int ID { get; set; }

        public int? MID { get; set; }

        public int? FID { get; set; }

        public int? IDPointer { get; set; }

        public int? IDVorgang { get; set; }

        [StringLength(200)]
        public string WeitergefuehrtVon { get; set; }

        public int? GutschriftID { get; set; }

        public int? Belegnummer { get; set; }

        public int? PersonalID { get; set; }

        [StringLength(200)]
        public string Benutzername { get; set; }

        public int? AdressenID { get; set; }

        [StringLength(50)]
        public string Kundennummer { get; set; }

        public int? BelegartID { get; set; }

        [StringLength(50)]
        public string Belegarttext { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Belegdatum { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Rechnungsdatum { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Bestelldatum { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Lieferdatum { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Versanddatum { get; set; }

        public int? VersandartenID { get; set; }

        [StringLength(200)]
        public string Versandart { get; set; }

        public int? LieferadressenID { get; set; }

        [StringLength(255)]
        public string LieferName1 { get; set; }

        [StringLength(255)]
        public string LieferName2 { get; set; }

        [StringLength(255)]
        public string LieferStrasse { get; set; }

        [StringLength(255)]
        public string LieferOrt { get; set; }

        [StringLength(255)]
        public string LieferPLZ { get; set; }

        [StringLength(255)]
        public string LieferLand { get; set; }

        [StringLength(255)]
        public string Name1 { get; set; }

        [StringLength(255)]
        public string Name2 { get; set; }

        [StringLength(255)]
        public string Strasse { get; set; }

        [StringLength(255)]
        public string Ort { get; set; }

        [StringLength(255)]
        public string PLZ { get; set; }

        [StringLength(255)]
        public string Land { get; set; }

        [StringLength(1073741823)]
        public string Belegkopf { get; set; }

        public int? ZahlungsartID { get; set; }

        [StringLength(200)]
        public string Zahlungsart { get; set; }

        public double? Gesamtpreis { get; set; }

        public double? Gesamtsteuer { get; set; }

        public double? Gesamtpreis7 { get; set; }

        public double? Gesamtpreis16 { get; set; }

        public double? Gesamtpreis19 { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(1073741823)]
        public string Zahlungsarttext { get; set; }

        [StringLength(50)]
        public string Besteuerung { get; set; }

        [StringLength(50)]
        public string EGID { get; set; }

        [StringLength(50)]
        public string Preisgruppe { get; set; }

        [StringLength(50)]
        public string Zahlungsbedingung { get; set; }

        public int UStPflicht { get; set; }

        public double? ZahlungBezahlt { get; set; }

        public double? ZahlungOffen { get; set; }

        public double? ZahlungVermerk { get; set; }

        public double? Gesamtbetrag { get; set; }

        public double? Gutschrift { get; set; }

        public double? KundenRabatt { get; set; }

        public int? Mahnstufe { get; set; }

        [Column(TypeName = "date")]
        public DateTime? LetzteMahnung { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Zahlungsfrist { get; set; }

        [StringLength(200)]
        public string Typ { get; set; }

        [StringLength(200)]
        public string IdentNr { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Annahmetermin { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Abgabetermin { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Annahmezeit { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Abgabezeit { get; set; }

        public double? Bargeld { get; set; }

        public double? ECKarte { get; set; }

        public double? Geldkarte { get; set; }

        public double? Kreditkarte { get; set; }

        public double? Scheck { get; set; }

        public double? Gegeben { get; set; }

        public double? Rueckgeld { get; set; }

        [StringLength(50)]
        public string Kasse { get; set; }

        public DateTime? ZS { get; set; }

        [StringLength(16)]
        public string ts { get; set; }

        public double? Gutschein { get; set; }

        public int? AnmeldeID { get; set; }

        public int? Administrator { get; set; }

        public int? GutscheinID { get; set; }

        public double? MwSt7 { get; set; }

        public double? MwSt16 { get; set; }

        public double? MwSt19 { get; set; }

        public double? Zwischensumme { get; set; }

        [StringLength(11)]
        public string Kontennummer { get; set; }

        public int? TourenID { get; set; }

        [StringLength(250)]
        public string Tour { get; set; }

        public double? Gewinn { get; set; }

        public double? Gesamtgewicht { get; set; }

        public int? Adressenart { get; set; }

        public int? VertreterID { get; set; }

        public int? VertreterREErstellt { get; set; }

        public int? Versandartberechnung { get; set; }

        [StringLength(100)]
        public string Rechnungsnummer { get; set; }

        [StringLength(100)]
        public string Weitergefuehrt { get; set; }

        public double? ProvisionBezahlt { get; set; }

        public double? Nettobetrag19 { get; set; }

        public double? Nettobetrag16 { get; set; }

        public double? Nettobetrag7 { get; set; }

        public double? Nettobetrag { get; set; }

        public int? Gesperrt { get; set; }

        public int? StornoBelegartID { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Geaendert { get; set; }

        [StringLength(16777215)]
        public string Kundenbemerkung { get; set; }

        public double? Zusatzkosten { get; set; }

        public int? FilialenID { get; set; }

        public int? BELagerBuchen { get; set; }

        public double? KundenRabattBetrag { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ZahlungszielDatum { get; set; }

        [StringLength(200)]
        public string VorherigerBelegnummer { get; set; }

        public int? LeergutMwSt { get; set; }

        public double? LeergutBetrag { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Zahlungsziel { get; set; }

        [StringLength(20)]
        public string Gegenkonto { get; set; }

        public int? RegionenID { get; set; }

        public double? Bruttobetrag19 { get; set; }

        public double? Bruttobetrag16 { get; set; }

        public double? Bruttobetrag7 { get; set; }

        public int? KostenverteilungID { get; set; }

        [StringLength(200)]
        public string Lieferscheinnummer { get; set; }

        public int? InBearbeitung { get; set; }

        public int? JournalID { get; set; }

        public int? JournalAuswahlID { get; set; }

        [StringLength(50)]
        public string StatusDruck { get; set; }

        [StringLength(200)]
        public string StatusVertrieb { get; set; }

        [StringLength(200)]
        public string StatusLager { get; set; }

        public int? Einzelbeleg { get; set; }

        public double? Gesamtkoli { get; set; }

        public double? Gesamtvolumen { get; set; }

        [StringLength(200)]
        public string Spediteur { get; set; }

        [StringLength(1073741823)]
        public string Kosteninfo { get; set; }

        [StringLength(1073741823)]
        public string Info { get; set; }

        [StringLength(100)]
        public string GedrucktVon { get; set; }

        [StringLength(200)]
        public string Kostenart { get; set; }

        [StringLength(20)]
        public string OP { get; set; }

        public int? AdressenartID { get; set; }

        public double? Gesamtkosten { get; set; }

        public double? Skontobetrag { get; set; }

        [StringLength(200)]
        public string Tourkostenart { get; set; }

        public int? BelegeIDVorher { get; set; }

        public int? BelegnummerVorher { get; set; }

        [StringLength(200)]
        public string BelegarttextVorher { get; set; }

        public int? BelegeIDNachher { get; set; }

        public int? BelegnummerNachher { get; set; }

        [StringLength(200)]
        public string BelegarttextNachher { get; set; }

        [StringLength(200)]
        public string Kommissionierer { get; set; }

        [StringLength(200)]
        public string Bestellnummer { get; set; }

        [StringLength(200)]
        public string Internetbestellnummer { get; set; }

        public double? Versandkostenfreibetrag { get; set; }

        public int? VerkaufsberaterID { get; set; }

        [StringLength(200)]
        public string Verkaufsberater { get; set; }

        [StringLength(200)]
        public string StatusBearbeitung { get; set; }

        [StringLength(200)]
        public string VersendetMit { get; set; }

        [StringLength(200)]
        public string Waehrung { get; set; }

        [StringLength(10)]
        public string WaehrungSymbol { get; set; }

        public double? WKurs { get; set; }

        [StringLength(200)]
        public string Barcodedruck { get; set; }

        [StringLength(16777215)]
        public string Zahlungsinfo { get; set; }

        [StringLength(200)]
        public string Mahnstatus { get; set; }

        [StringLength(16777215)]
        public string Mahninfo { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Mahndatum1 { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Mahndatum2 { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Mahndatum3 { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Mahndatum { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Zahlungserinnerung { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Gerichtsmahnbescheid { get; set; }

        public int? Faelligstufe { get; set; }

        public int? MahnungID { get; set; }

        public int? OPTage { get; set; }

        public int? MaxOPTage { get; set; }

        [StringLength(200)]
        public string MahnKorrektur { get; set; }

        [StringLength(16777215)]
        public string MahnKorrekturInfo { get; set; }

        public int? DTAAbbuchung { get; set; }

        public int? DTASperre { get; set; }

        public int? Bundesland { get; set; }

        public int? ArtDesGeschaefts { get; set; }

        public int? Verfahrenscode { get; set; }

        public int? Laendercode { get; set; }

        public int? IntrastatistikID { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Abschreibungsdatum { get; set; }

        public int? Vorbestellstatus { get; set; }

        public int? VorbestellLieferantID { get; set; }

        public int? VorbestellLieferantnummer { get; set; }

        [StringLength(200)]
        public string VorbestellLieferant { get; set; }

        [StringLength(16777215)]
        public string BelegNotiz { get; set; }

        public int? P13B { get; set; }

        [Column(TypeName = "date")]
        public DateTime? AbschreibungDatum { get; set; }

        [Column(TypeName = "date")]
        public DateTime? InsolvenzDatum { get; set; }

        [StringLength(255)]
        public string trackingnumbers { get; set; }

        public int? PaketAnzahl { get; set; }

        public double? Buchungsbetrag { get; set; }

        public double? EbayGuthaben { get; set; }

        public int? KommissionsbelegID { get; set; }

        [Column(TypeName = "date")]
        public DateTime? LieferbarDatum { get; set; }

        [StringLength(32)]
        public string Transaktionscode { get; set; }

        public int? VerkaeuferID { get; set; }

        public double? Zahlungseingang { get; set; }

        public int? ZahlungseingangDTAAbbuchung { get; set; }

        public int? StatusFarbe { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Zahlungsdatum { get; set; }

        [StringLength(50)]
        public string VersandStatus { get; set; }

        public double? Gesamtpunkte { get; set; }

        public double? Buchungsguthaben { get; set; }

        public int? BuchungsguthabenBelegeID { get; set; }

        public double? Nettobetrag1 { get; set; }

        public double? Nettobetrag2 { get; set; }

        public double? Nettobetrag3 { get; set; }

        public double? Nettobetrag4 { get; set; }

        public double? Nettobetrag5 { get; set; }

        public double? MwSt1 { get; set; }

        public double? MwSt2 { get; set; }

        public double? MwSt3 { get; set; }

        public double? MwSt4 { get; set; }

        public double? MwSt5 { get; set; }

        public double? Steuersatz1 { get; set; }

        public double? Steuersatz2 { get; set; }

        public double? Steuersatz3 { get; set; }

        public double? Steuersatz4 { get; set; }

        public double? Steuersatz5 { get; set; }

        public double? Bruttobetrag1 { get; set; }

        public double? Bruttobetrag2 { get; set; }

        public double? Bruttobetrag3 { get; set; }

        public double? Bruttobetrag4 { get; set; }

        public double? Bruttobetrag5 { get; set; }

        public int? Gegenkonto1 { get; set; }

        public int? Gegenkonto2 { get; set; }

        public int? Gegenkonto3 { get; set; }

        public int? Gegenkonto4 { get; set; }

        public int? Gegenkonto5 { get; set; }

        public int? SS1 { get; set; }

        public int? SS2 { get; set; }

        public int? SS3 { get; set; }

        public int? SS4 { get; set; }

        public int? SS5 { get; set; }

        public double? Nachnahme { get; set; }

        public double? NachnahmeSteuersatz { get; set; }

        public double? NachnahmeMwSt { get; set; }

        public double? NachnahmeBrutto { get; set; }

        public double? Versandkosten { get; set; }

        public double? VersandkostenSteuersatz { get; set; }

        public double? VersandkostenMwSt { get; set; }

        public double? VersandkostenBrutto { get; set; }

        public double? LeergutSteuersatz1 { get; set; }

        public double? LeergutSteuersatz2 { get; set; }

        public double? LeergutSteuer1 { get; set; }

        public double? LeergutSteuer2 { get; set; }

        public double? LeergutSteuer { get; set; }

        public double? LeergutNetto1 { get; set; }

        public double? LeergutNetto2 { get; set; }

        public int? ErstelltVonID { get; set; }

        [StringLength(200)]
        public string ErstelltVon { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ErstelltAm { get; set; }

        public DateTime? ErstelltUm { get; set; }

        public int? AkontoZahlungID { get; set; }

        public int? AnbieterDirektZahlung { get; set; }

        public int? SepaKunde { get; set; }

        public int? SepaStatus { get; set; }

        public int? BerechnungGesperrt { get; set; }

        public int? BelegeShopID { get; set; }

        public int? BuchungsbetragNichtBerechnen { get; set; }

        public int? AnbieterTeilung { get; set; }

        [StringLength(100)]
        public string AmazonTransaktionsNr { get; set; }

        public int? DAKSperre { get; set; }

        public int? DAKAbbuchung { get; set; }

        public double? Amazonkosten { get; set; }

        public int? ZahlungAnAnbieter { get; set; }

        public int? InOPAnzeigen { get; set; }
    }
}
