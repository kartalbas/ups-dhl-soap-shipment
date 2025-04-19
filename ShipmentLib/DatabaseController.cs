using ShipmentLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ShipmentLib
{
    public class DatabaseController
    {
        private string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        public List<ReportData> GetShippedOrders(DateTime? dtDateBegin, DateTime? dtDateEnd, string strService)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;


            List<ReportData> objResult = new List<ReportData>();

            using (var objDb = new WegaDB())
            {
                if (strService.Equals("UPS"))
                {
                    objResult = objDb.trackingnumbers
                        .Join(objDb.beleges, t => t.RechnungsNr, b => b.Belegnummer, (t, b) => new { t, b })
                        .Where(a => (a.t.Datum >= dtDateBegin && a.t.Datum <= dtDateEnd) && a.t.TrackingNr.StartsWith("1Z"))
                        .Select(a => new ReportData { Kundennummer = a.b.Kundennummer, LieferadressenID = a.b.LieferadressenID, Versandart = a.b.Versandart, Gesamtgewicht = a.b.Gesamtgewicht, Trackingnumbers = a.b.trackingnumbers })
                        .ToList();
                }
                else if (strService.Equals("DHL"))
                {
                    objResult = objDb.trackingnumbers
                        .Join(objDb.beleges, t => t.RechnungsNr, b => b.Belegnummer, (t, b) => new { t, b })
                        .Where(a => (a.t.Datum >= dtDateBegin && a.t.Datum <= dtDateEnd) && !a.t.TrackingNr.StartsWith("1Z"))
                        .Select(a => new ReportData { Kundennummer = a.b.Kundennummer, LieferadressenID = a.b.LieferadressenID, Versandart = a.b.Versandart, Gesamtgewicht = a.b.Gesamtgewicht, Trackingnumbers = a.b.trackingnumbers })
                        .ToList();
                }
            }

            if(objResult == null || objResult.Count == 0)
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Invoices could not be found!");
            else
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Count of Invoices: " + objResult.Count);

            return objResult;
        }

        public belege SaveTrackingnumbers(string strTrackingnumbers, string strOrderNr)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var objDb = new WegaDB();

            int iOrderNr = ShipmentTools.SafeParseInt(strOrderNr);

            var objResult = (from o in objDb.beleges
                            where o.Belegnummer == iOrderNr
                            select o).FirstOrDefault();

            if (objResult == null)
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Error during saving trackingnumbers back to database! order with Ordernumber=" + iOrderNr.ToString() + " not found!");

            objResult.trackingnumbers = strTrackingnumbers;

            var objTrackingnumbers = new trackingnumbers
            {
                Datum = DateTime.Now,
                RechnungsNr = iOrderNr,
                TrackingNr = strTrackingnumbers,
                Status = TrackingStatus.Created
            };

            objDb.trackingnumbers.Add(objTrackingnumbers);

            objDb.SaveChanges();
            objDb.Dispose();

            if (objResult == null)
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Invoice could not be found!");
            else
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Found Invoice: " + objResult.Belegnummer);

            return objResult;
        }

        public belege RemoveTrackingnumber(string strTrackingnumber)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var objDb = new WegaDB();

            var cobjBelege = (from o in objDb.beleges
                         where o.trackingnumbers.Contains(strTrackingnumber)
                         select o);

            var objTrackingnumbers = (from t in objDb.trackingnumbers
                              where t.TrackingNr.Contains(strTrackingnumber)
                              select t).FirstOrDefault();


            if (cobjBelege == null || objTrackingnumbers == null)
                return null;

            belege objRetBeleg = null;

            foreach (belege objBeleg in cobjBelege)
            {
                objRetBeleg = objBeleg;

                if (objBeleg.Belegnummer == objTrackingnumbers.RechnungsNr)
                {
                    objTrackingnumbers.TrackingNr = objTrackingnumbers.TrackingNr.Replace(strTrackingnumber, "");
                    objTrackingnumbers.TrackingNr = RemoveCommas(objTrackingnumbers.TrackingNr);
                }

                objBeleg.trackingnumbers = objBeleg.trackingnumbers.Replace(strTrackingnumber, "");
                objBeleg.trackingnumbers = RemoveCommas(objBeleg.trackingnumbers);
            }

            //save result
            objDb.SaveChanges();
            objDb.Dispose();

            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Trackingnumber " + strTrackingnumber + " removed from database");
            return objRetBeleg;
        }

        private string RemoveCommas(string strValue)
        {
            //remove double comma because value between was removed?
            strValue = strValue.Replace(",,", ",");

            //remove comma at beginning
            if (strValue.Length > 1 && strValue.Substring(0, 1).Equals(","))
                strValue = strValue.Substring(1, strValue.Length - 1);

            //remove comma at end
            if (strValue.Length > 1 && strValue.Substring(strValue.Length - 1, 1).Equals(","))
                strValue = strValue.Substring(0, strValue.Length - 1);

            return strValue;
        }

        public adressen GetAdress(int? iAddressId)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var objDb = new WegaDB();

            var objAddress = (from a in objDb.adressens
                           where a.ID == iAddressId
                           select a).FirstOrDefault();

            objDb.Dispose();

            if (objAddress == null)
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Address could not be found!");
            else
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Found Address: " + objAddress.Kundennummer);

            return objAddress;
        }

        public lieferadressen GetLieferadressen(int? LieferadressenId)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var objDb = new WegaDB();

            var objResults = (from a in objDb.lieferadressens
                              where a.ID == LieferadressenId
                              select a).FirstOrDefault();

            objDb.Dispose();

            //if (objResults == null)
            //    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Lieferadress could not be found!");
            //else
            //    Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Found Lieferadress for " + objResults.Name1);

            return objResults;
        }

        public land GetLand(string strCountryname)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            if (strCountryname.Equals("Cyprus"))
                strCountryname = "Zypern";

            var objDb = new WegaDB();

            var objCountry = (from c in objDb.lands
                           where c.Land1 == strCountryname
                           select c).FirstOrDefault();

            objDb.Dispose();

            if (objCountry == null || objCountry.Kuerzel.Length < 2)
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Country could not be found!");
            else
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Found Country: " + objCountry.Kuerzel);

            return objCountry;
        }

        public string GetCountryCode(string strLand)
        {
            var objResult = GetLand(strLand);
            if (objResult != null)
                return objResult.Kuerzel;

            return string.Empty;
        }

        public belege GetOrder(int iOrderId)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var objDb = new WegaDB();

            var objOrder = (from o in objDb.beleges
                         where o.Belegnummer == iOrderId
                         select o).FirstOrDefault();

            objDb.Dispose();

            if (objOrder == null)
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Order could not be found!");
            else
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Found Order: " + objOrder.Belegnummer);

            return objOrder;
        }

        public List<belegdetail> GetOrderdetails(int iOrderId)
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            var objDb = new WegaDB();

            var objOrderdetails = (from o in objDb.belegdetails
                     where o.Belegnummer == iOrderId.ToString()
                     select o).OrderBy(a => a.PosNummer).ToList();

            objDb.Dispose();

            if (objOrderdetails == null || objOrderdetails.Count == 0)
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Orderdetails could not be found!");
            else
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Count of Orderdetails: " + objOrderdetails.Count);

            return objOrderdetails;
        }

    }
}
