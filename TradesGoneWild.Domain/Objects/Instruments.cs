using System.Collections.Generic;
using System.Linq;

namespace TradesGoneWild.Domain
{
    /// <summary>
    /// Central location for markets/instruments
    /// </summary>
    public static class AllInstruments
    {
        private static List<Instrument> _instruments;

        static AllInstruments()
        {
            _instruments = new List<Instrument>();
        }

        public static void SetInstruments(List<Instrument> list)
        {
            _instruments.AddRange(list);
        }

        public static Instrument GetInstrument(string market, string symbol)
        {
            return _instruments
                .Where(i => i.Market == market && i.Symbol == symbol)
                .FirstOrDefault();
        }
    }
}
