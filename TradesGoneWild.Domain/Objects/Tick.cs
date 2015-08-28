using System;

namespace TradesGoneWild.Domain
{
    public class Tick
    {
        public DateTime Date { get; set; }
        public Instrument Instrument { get; set; }
        public decimal Price { get; set; }
    }
}
