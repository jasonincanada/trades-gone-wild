using System;
using System.Collections.Generic;
using System.Linq;

namespace TradesGoneWild.Domain
{
    public class Store
    {
        public TradesGoneWildEntities _context;

        public Store()
        {
            _context = new TradesGoneWildEntities();
        }

        /// <summary>
        /// Return all known instruments
        /// </summary>
        /// <returns></returns>
        public List<Instrument> GetInstruments()
        {
            return _context
                .StoreInstruments
                .Select(s => new Instrument
                {
                    InstrumentID = s.InstrumentID,
                    Market = s.Market,
                    Symbol = s.Symbol
                })
                .ToList();
        }
                
        /// <summary>
        /// Get all open alerts
        /// </summary>
        /// <returns></returns>
        public List<StoreAlert> GetOpenAlerts()
        {
            return _context
                .StoreAlerts
                .Include("Instrument")
                .Where(a => a.DateClosed == null)
                .ToList();
        }

        /// <summary>
        /// Add a new Alert record to the store and sets the AlertID in the passed alert
        /// </summary>
        /// <param name="alert"></param>
        public void AddAlert(Alert alert)
        {
            var store = new StoreAlert
            {
                FKInstrumentID = alert.Instrument.InstrumentID,
                ClosestPrice = alert.ClosestPrice,
                DateOpened = alert.OpeningDate,
                PriceTarget = alert.PriceTarget,
                TargetDirection = alert.TargetDirection
            };

            _context
                .StoreAlerts
                .Add(store);

            _context.SaveChanges();

            alert.AlertID = store.AlertID;
        }

        /// <summary>
        /// Persist changes to the store
        /// </summary>
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Update the volatile fields of the passed alert in the store
        /// </summary>
        /// <param name="alert"></param>
        public void UpdateAlert(Alert alert)
        {
            var store = _context
                .StoreAlerts
                .Where(a => a.AlertID == alert.AlertID)
                .FirstOrDefault();

            if (store == null)
                return;
                        
            store.ClosestPrice = alert.ClosestPrice;
            store.TargetDirection = alert.TargetDirection;

            if (alert.Finished == true)
            {
                if (store.DateClosed == null)
                    store.DateClosed = DateTime.UtcNow;
            }
        }
    }
}
