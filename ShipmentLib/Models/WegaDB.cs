namespace ShipmentLib
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public partial class WegaDB : DbContext
    {
        public WegaDB()
            : base("name=WegaDB")
        {
            Database.CommandTimeout = 300;
        }

        public virtual DbSet<adressen> adressens { get; set; }
        public virtual DbSet<belegdetail> belegdetails { get; set; }
        public virtual DbSet<belege> beleges { get; set; }
        public virtual DbSet<land> lands { get; set; }
        public virtual DbSet<lieferadressen> lieferadressens { get; set; }
        public virtual DbSet<versandarten> versandartens { get; set; }
        public virtual DbSet<zahlungsarten> zahlungsartens { get; set; }
        public virtual DbSet<trackingnumbers> trackingnumbers { get; set; }

        public static void MoveTrackingnumbers(DateTime objDateStart, DateTime objDateStop)
        {
            int iCount;
            using (var objDb = new WegaDB())
            {
                objDb.Database.ExecuteSqlCommand("set net_write_timeout=99999; set net_read_timeout=99999");

                //Deleteing existing Trackingnubmers from table trackingnumbers
                iCount = 0;
                var cobjTrackingNumbers = objDb.trackingnumbers.Where(b => (b.Datum >= objDateStart && b.Datum <= objDateStop));
                int iCountTrackingNumbers = cobjTrackingNumbers.Count();
                foreach (var objTrackingNumber in cobjTrackingNumbers)
                {
                    objDb.trackingnumbers.Remove(objTrackingNumber);
                    Console.WriteLine(iCount + "/" + iCountTrackingNumbers + " | Delete: " + objTrackingNumber.RechnungsNr + " | " + objTrackingNumber.Datum + " | " + objTrackingNumber.TrackingNr);
                    iCount++;
                }
                Console.WriteLine(iCount + "/" + iCountTrackingNumbers + " " + DateTime.Now + " | Deleting...");
                objDb.SaveChanges();
                Console.WriteLine(iCount + "/" + iCountTrackingNumbers + " " + DateTime.Now + " | Deleted");

                //Moving Trackingnumbers from belege to trackingnumbers
                iCount = 0;
                var cobjBelege = objDb.beleges.Where(b => b.BelegartID == 1 && b.trackingnumbers.Length > 0 && (b.Belegdatum >= objDateStart && b.Belegdatum <= objDateStop));
                int iCountBelege = cobjBelege.Count();

                foreach(var objBeleg in cobjBelege)
                {
                    var objTrackingnumbers = new trackingnumbers
                    {
                        Datum = objBeleg.Belegdatum,
                        RechnungsNr = objBeleg.Belegnummer,
                        TrackingNr = objBeleg.trackingnumbers,
                        Status = TrackingStatus.Created
                    };

                    objDb.trackingnumbers.Add(objTrackingnumbers);
                    Console.WriteLine(iCount + "/" + iCountBelege + " | Add: " + objTrackingnumbers.RechnungsNr + " | " + objTrackingnumbers.Datum + " | " + objTrackingnumbers.TrackingNr);
                    iCount++;
                }

                Console.WriteLine(iCount + "/" + iCountBelege + " " + DateTime.Now + " | Saving...");
                objDb.SaveChanges();
                Console.WriteLine(iCount + "/" + iCountBelege + " " + DateTime.Now + " | Saved");
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<adressen>()
                .Property(e => e.Mandatsreferenznummer)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Kundencode)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Anrede)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Name1)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Name2)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Position)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Strasse)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Ort)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Region)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.PLZ)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Land)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Telefon1)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Telefon2)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Telefax)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Mobil)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.eMail)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Homepage)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Info)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Zahlungsart)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.SteuerID)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Verkaufsberater)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Preisgruppe)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Versandart)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Zahlungsbedingung)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Sprache)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Inhaber)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Finanzamt)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Steuernummer)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Nationalitaet)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Ausweisnummer)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Ausstellungsort)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Passwort)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Vertretername)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Rechnungsgruppe)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Rechtsabteilung)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Tour)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Factoringtext)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.VersendetMit)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Mahnstatus)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.BankInhaber)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.BankName)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.BankKonto)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.BankBLZ)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.BankSwiftcode)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.BankIban)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.BankKartennummer)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.AfterbuyKundennummer)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Beschwerden)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Massnahmen)
                .IsUnicode(false);

            modelBuilder.Entity<adressen>()
                .Property(e => e.Rechnungsmail)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Lieferantenartikelnummer)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Matchcode)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Artikeltext1)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Artikeltext2)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Text)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Chargennummer)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Einheit)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.BarcodeNr)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.TextilString)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Artikelgruppe)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Status)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.PosReasonsText)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Personal)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Belegnummer)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Verpackungsart)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Waehrung)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.WaehrungSymbol)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Regal)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Warennummer)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Seriennummer)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.FremdartikelNr)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Fremdartikeltext)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.HerkunftsLaenderCode)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.Herkunftsland)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.ArtDesGeschaefts)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.HerstellerartikelNr)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.AmazonTransaktionsNr)
                .IsUnicode(false);

            modelBuilder.Entity<belegdetail>()
                .Property(e => e.ChargenNr)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.WeitergefuehrtVon)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Benutzername)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Kundennummer)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Belegarttext)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Versandart)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.LieferName1)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.LieferName2)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.LieferStrasse)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.LieferOrt)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.LieferPLZ)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.LieferLand)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Name1)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Name2)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Strasse)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Ort)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.PLZ)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Land)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Belegkopf)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Zahlungsart)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Status)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Zahlungsarttext)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Besteuerung)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.EGID)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Preisgruppe)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Zahlungsbedingung)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Typ)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.IdentNr)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Kasse)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.ts)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Kontennummer)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Tour)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Rechnungsnummer)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Weitergefuehrt)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Kundenbemerkung)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.VorherigerBelegnummer)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Gegenkonto)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Lieferscheinnummer)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.StatusDruck)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.StatusVertrieb)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.StatusLager)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Spediteur)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Kosteninfo)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Info)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.GedrucktVon)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Kostenart)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.OP)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Tourkostenart)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.BelegarttextVorher)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.BelegarttextNachher)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Kommissionierer)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Bestellnummer)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Internetbestellnummer)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Verkaufsberater)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.StatusBearbeitung)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.VersendetMit)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Waehrung)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.WaehrungSymbol)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Barcodedruck)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Zahlungsinfo)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Mahnstatus)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Mahninfo)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.MahnKorrektur)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.MahnKorrekturInfo)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.VorbestellLieferant)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.BelegNotiz)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.trackingnumbers)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.Transaktionscode)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.VersandStatus)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.ErstelltVon)
                .IsUnicode(false);

            modelBuilder.Entity<belege>()
                .Property(e => e.AmazonTransaktionsNr)
                .IsUnicode(false);

            modelBuilder.Entity<land>()
                .Property(e => e.Land1)
                .IsUnicode(false);

            modelBuilder.Entity<land>()
                .Property(e => e.Kuerzel)
                .IsUnicode(false);

            modelBuilder.Entity<lieferadressen>()
                .Property(e => e.Anrede)
                .IsUnicode(false);

            modelBuilder.Entity<lieferadressen>()
                .Property(e => e.Name1)
                .IsUnicode(false);

            modelBuilder.Entity<lieferadressen>()
                .Property(e => e.Name2)
                .IsUnicode(false);

            modelBuilder.Entity<lieferadressen>()
                .Property(e => e.Strasse)
                .IsUnicode(false);

            modelBuilder.Entity<lieferadressen>()
                .Property(e => e.HausNr)
                .IsUnicode(false);

            modelBuilder.Entity<lieferadressen>()
                .Property(e => e.Plz)
                .IsUnicode(false);

            modelBuilder.Entity<lieferadressen>()
                .Property(e => e.Ort)
                .IsUnicode(false);

            modelBuilder.Entity<lieferadressen>()
                .Property(e => e.Land)
                .IsUnicode(false);

            modelBuilder.Entity<versandarten>()
                .Property(e => e.Versandart)
                .IsUnicode(false);

            modelBuilder.Entity<zahlungsarten>()
                .Property(e => e.Zahlungsart)
                .IsUnicode(false);

            modelBuilder.Entity<zahlungsarten>()
                .Property(e => e.Zahlungsarttext)
                .IsUnicode(false);

            modelBuilder.Entity<zahlungsarten>()
                .Property(e => e.DTATextschluessel)
                .IsUnicode(false);
        }
    }
}
