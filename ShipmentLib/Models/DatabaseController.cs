using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ShipmentLib
{
    public class DatabaseController
    {
        public List<belege> GetShippedOrders(DateTime? dateBegin, DateTime? dateEnd)
        {
            //Get UPS shipped orders from DB
            //var db = new WegaDBEntities();
            var db = new WegaDB();

            var orders = (from o in db.beleges
                            where (o.Belegdatum >= dateBegin && o.Belegdatum <= dateEnd) && o.trackingnumbers.Length > 1 && o.BelegartID == 1
                            orderby o.Versandart
                            select o).ToList();

            db.Dispose();

            return orders;
        }

        public belege SaveTrackingnumbers(string trackingnumbers, string ordernr)
        {
            var db = new WegaDB();

            int oid = ShipmentTools.SafeParseInt(ordernr);

            var order = (from o in db.beleges
                            where o.Belegnummer == oid
                            select o).FirstOrDefault();

            if (order == null)
                new ShipmentException(null, "Fatal error during saving trackingnumbers back to database! order with Ordernumber=" + oid.ToString() + " not found!");

            order.trackingnumbers = trackingnumbers;

            db.SaveChanges();
            db.Dispose();

            return order;
        }

        public belege RemoveTrackingnumber(string trackingnumber)
        {
            var db = new WegaDB();

            var order = (from o in db.beleges
                         where o.trackingnumbers.Contains(trackingnumber)
                         select o).FirstOrDefault();

            order.trackingnumbers = order.trackingnumbers.Replace(trackingnumber, "");

            //remove komma
            order.trackingnumbers = order.trackingnumbers.Replace(",,", ",");

            if (order.trackingnumbers.Length > 1 && order.trackingnumbers.Substring(0, 1).Equals(","))
                order.trackingnumbers = order.trackingnumbers.Substring(1, order.trackingnumbers.Length - 1);

            if (order.trackingnumbers.Length > 1 && order.trackingnumbers.Substring(order.trackingnumbers.Length - 1, 1).Equals(","))
                order.trackingnumbers = order.trackingnumbers.Substring(0, order.trackingnumbers.Length - 1);

            //save result
            db.SaveChanges();
            db.Dispose();

            return order;
        }

        public adressen GetAdress(int? iAddressId)
        {
            var db = new WegaDB();

            var address = (from a in db.adressens
                           where a.ID == iAddressId
                           select a).FirstOrDefault();

            var clonedAddress = new adressen();
            db.Entry(clonedAddress).CurrentValues.SetValues(address);
            db.Dispose();

            return clonedAddress;
        }

        public land GetLand(string sLand)
        {
            var db = new WegaDB();

            var country = (from c in db.lands
                           where c.Land1 == sLand
                           select c).FirstOrDefault();

            var clonedCountry = new land();
            db.Entry(clonedCountry).CurrentValues.SetValues(country);
            db.Dispose();

            return clonedCountry;
        }

        public belege GetOrder(int iOrderId)
        {
            var db = new WegaDB();

            var order = (from o in db.beleges
                         where o.Belegnummer == iOrderId
                         select o).FirstOrDefault();

            var clonedOrder = new belege();
            db.Entry(clonedOrder).CurrentValues.SetValues(order);
            db.Dispose();

            return clonedOrder;
        }
    }
}
