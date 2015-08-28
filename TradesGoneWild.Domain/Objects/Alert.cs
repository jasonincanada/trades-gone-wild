using System;
using System.Collections.Generic;
using System.Linq;

namespace TradesGoneWild.Domain
{
    public class Instrument
    {
        public int InstrumentID { get; set; }
        public string Market { get; set; }
        public string Symbol { get; set; }

        public override int GetHashCode()
        {
            return InstrumentID;
        }
    }

    public enum AlertEventType
    {
        CloserToTarget,
        TargetReached
    }
    
    public class AlertEventArgs 
    {
        public Alert Alert { get; set; }
        public Instrument Instrument { get; set; }
        public decimal Price { get; set; }
        public AlertEventType EventType { get; set; }
    }
        
    public class Alert
    {
        public int AlertID { get; set; }
        public Instrument Instrument { get; set; }        
        public decimal PriceTarget { get; set; }
        public decimal ClosestPrice { get; set; }                
        public DateTime OpeningDate { get; set; }        
        public bool Finished { get; set; }

        /// <summary>
        /// +1 for above current price, 0 for not yet known, -1 for below current price
        /// </summary>
        public int TargetDirection { get; set; }

        public bool HitsOrExceedsTarget(decimal price)
        {
            if (TargetDirection == 0)
            {
                if (PriceTarget > price)
                    TargetDirection = 1;
                else
                    TargetDirection = -1;

                return false;
            }

            return (TargetDirection == -1 && price <= PriceTarget)
                || (TargetDirection == 1 && price >= PriceTarget);
        }

        public bool IsCloserToTarget(decimal price)
        {
            if (ClosestPrice == 0)
            {
                ClosestPrice = price;
                return true;
            }

            return Math.Abs(PriceTarget - price) < Math.Abs(PriceTarget - ClosestPrice);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}:{2} {3} to {4}",
                                AlertID,
                                Instrument.Market,
                                Instrument.Symbol,
                                TargetDirection,
                                PriceTarget);
        }
    }

    public class AlertManager
    {
        public AlertManager()
        {
            _alerts = new List<Alert>();
        }

        /// <summary>
        /// A new tick has come in for the provided instrument, poll all alerts
        /// </summary>
        public void NewTick(Tick tick)
        {
            DateTime date = tick.Date;
            Instrument instrument = tick.Instrument;
            decimal price = tick.Price;
                        
            if (instrument == null)
                return;

            var alerts = _alerts.Where(a => a.Instrument == instrument && !a.Finished);

            foreach (var alert in alerts)
            {
                if (alert.Finished)
                    continue;                

                if (alert.HitsOrExceedsTarget(price))
                {
                    alert.Finished = true;

                    ThrowEvent(this, new AlertEventArgs
                    {
                        Alert = alert,
                        Instrument = instrument,
                        Price = price,
                        EventType = AlertEventType.TargetReached
                    });
                }

                // Check if new closest known price since the plan started
                else if (alert.IsCloserToTarget(price))
                {
                    alert.ClosestPrice = price;

                    ThrowEvent(this, new AlertEventArgs
                    {
                        Alert = alert,
                        Instrument = instrument,
                        Price = price,
                        EventType = AlertEventType.CloserToTarget
                    });
                }
            }
        }

        private List<Alert> _alerts;

        public void AddAlert(Alert alert)
        {
            _alerts.Add(alert);
        }

        public delegate void EventHandler(object sender, AlertEventArgs args);
        public event EventHandler ThrowEvent = delegate { };

        public IEnumerable<Alert> AllOrderedPlans()
        {
            return _alerts
                .OrderBy(p => p.OpeningDate)
                .ToList();
        }

        public IEnumerable<Alert> Alerts()
        {
            return _alerts;
        }
    }
}
